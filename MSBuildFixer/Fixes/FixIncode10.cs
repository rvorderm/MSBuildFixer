using Microsoft.Build.Construction;
using MSBuildFixer.Helpers;
using System.Collections.Generic;

namespace MSBuildFixer.Fixes
{
	public class FixIncode10 : IFix
	{
		public List<IPackageConfigHelper> PackageBuilders = new List<IPackageConfigHelper>();
		public void AttachTo(SolutionWalker walker)
		{
			walker.OnVisitProjectItem_Other += Walker_OnVisitProjectItem_Compile;
			walker.OnVisitProjectRootItem += Walker_OnVisitProjectRootItem;
			walker.OnVisitProjectItem_Reference += Walker_OnVisitProjectItem_Reference;
			walker.OnSave += Walker_OnSave;
		}

		private void Walker_OnVisitProjectItem_Compile(ProjectItemElement projectItemElement)
		{
			if (projectItemElement.Include.EndsWith("packages.config"))
			{
				projectItemElement.Parent.RemoveChild(projectItemElement);
			}
		}

		private void Walker_OnVisitProjectItem_Reference(ProjectItemElement projectItemElement)
		{
			string assemblyName = ProjectItemElementHelpers.GetAssemblyName(projectItemElement);
			if (assemblyName.Equals("Api.Core") || assemblyName.Equals("Tyler.API"))
			{
				projectItemElement.Include = assemblyName;
				ProjectItemElementHelpers.AddOrUpdateMetaData(projectItemElement, "Private", false.ToString());
				ProjectItemElementHelpers.AddOrUpdateMetaData(projectItemElement, "HintPath", "$(OutputPath)");
			}

			if (assemblyNames.Contains(assemblyName))
			{
				projectItemElement.Parent.RemoveChild(projectItemElement);
			}
		}

		public void Walker_OnSave()
		{
			foreach (IPackageConfigHelper packageConfigHelper in PackageBuilders)
			{
				packageConfigHelper.SavePackageFile(packageConfigHelper.GetPackageDocument());
			}
		}

		private HashSet<string> assemblyNames = new HashSet<string>()
		{
			"Castle.Core",
			"Microsoft.ApplicationInsights",
			"Microsoft.Owin",
			"Microsoft.Owin.Cors",
			"Microsoft.Owin.FileSystems",
			"Microsoft.Owin.Host.HttpListener",
			"Microsoft.Owin.Host.SystemWeb",
			"Microsoft.Owin.Hosting",
			"Microsoft.Owin.Security",
			"Microsoft.Owin.Security.Jwt",
			"Microsoft.Owin.Security.OAuth",
			"Microsoft.Owin.StaticFiles",
			"Microsoft.ServiceBus",
			"Newtonsoft.Json",
			"Owin",
			"Serilog",
			"Serilog.FullNetFx",
			"System.IdentityModel.Tokens.Jwt",
			"System.Net.Http.Formatting",
			"System.Web.Cors",
			"System.Web.Http.Owin",
			"System.Web.Http.WebHost",
		};


		private void Walker_OnVisitProjectRootItem(ProjectRootElement rootElement)
		{
			string packagePath = ProjectRootElementHelpers.GetNugetPackagePath(rootElement);
			var packageConfigHelper = new PackageConfigHelper(packagePath);
			packageConfigHelper.Remove("Castle.Core");
			packageConfigHelper.Remove("Microsoft.ApplicationInsights");
			packageConfigHelper.Remove("Microsoft.AspNet.Cors");
			packageConfigHelper.Remove("Microsoft.AspNet.WebApi.Client");
			packageConfigHelper.Remove("Microsoft.AspNet.WebApi.Core");
			packageConfigHelper.Remove("Microsoft.AspNet.WebApi.Owin");
			packageConfigHelper.Remove("Microsoft.AspNet.WebApi.OwinSelfHost");
			packageConfigHelper.Remove("Microsoft.AspNet.WebApi.WebHost");
			packageConfigHelper.Remove("Microsoft.Owin");
			packageConfigHelper.Remove("Microsoft.Owin.Cors");
			packageConfigHelper.Remove("Microsoft.Owin.FileSystems");
			packageConfigHelper.Remove("Microsoft.Owin.Host.HttpListener");
			packageConfigHelper.Remove("Microsoft.Owin.Host.SystemWeb");
			packageConfigHelper.Remove("Microsoft.Owin.Hosting");
			packageConfigHelper.Remove("Microsoft.Owin.Security");
			packageConfigHelper.Remove("Microsoft.Owin.Security.Jwt");
			packageConfigHelper.Remove("Microsoft.Owin.Security.OAuth");
			packageConfigHelper.Remove("Microsoft.Owin.StaticFiles");
			packageConfigHelper.Remove("Newtonsoft.Json");
			packageConfigHelper.Remove("Owin");
			packageConfigHelper.Remove("Serilog");
			packageConfigHelper.Remove("System.IdentityModel.Tokens.Jwt");
			packageConfigHelper.Remove("WindowsAzure.ServiceBus");

			packageConfigHelper.Remove("Public.API.Core");
			packageConfigHelper.Remove("Tyler.API");

			PackageBuilders.Add(packageConfigHelper);
		}
	}
}

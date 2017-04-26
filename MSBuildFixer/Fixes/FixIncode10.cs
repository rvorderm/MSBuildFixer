using System;
using Microsoft.Build.Construction;
using MSBuildFixer.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Evaluation;

namespace MSBuildFixer.Fixes
{
	public class FixIncode10 : IFix
	{
		public List<IPackageConfigHelper> PackageBuilders = new List<IPackageConfigHelper>();
		IList<ProjectItemElement> items = new List<ProjectItemElement>();
		public void AttachTo(SolutionWalker walker)
		{
			_solutionWalker = walker;
//			_solutionWalker.OnOpenSolution += Walker_OnOpenSolution;
			_solutionWalker.OnVisitProjectRootItem += Walker_OnVisitProjectRootItem;
			_solutionWalker.OnVisitProjectItem_Reference += Walker_OnVisitProjectItem_Reference;
			_solutionWalker.OnSave += Walker_OnSave;
		}

		private void Walker_OnOpenSolution(string solutionPath)
		{
			string fileName = Path.GetFileName(solutionPath);
			if (!fileName.StartsWith("Incode10")) return;
//			_solutionWalker.OnVisitProjectItem_Other -= Walker_OnVisitProjectItem_Compile;
			_solutionWalker.OnVisitProjectRootItem -= Walker_OnVisitProjectRootItem;
			_solutionWalker.OnVisitProjectItem_Reference -= Walker_OnVisitProjectItem_Reference;
			_solutionWalker.OnSave -= Walker_OnSave;
		}

		private void Walker_OnVisitProjectItem_Reference(ProjectItemElement projectItemElement)
		{
			string assemblyName = ProjectItemElementHelpers.GetAssemblyName(projectItemElement);

			if (!assemblyNames.Any(x => assemblyName.Contains(x))) return;

			projectItemElement.Parent.RemoveChild(projectItemElement);
			items.Add(projectItemElement);

			ProjectRootElement rootElement = projectItemElement.ContainingProject;
			if (rootElement.Imports.Any(x => x.Project.EndsWith("Nunit.Targets"))) return;

			string path = PathHelpers.MakeRelativePath(rootElement.FullPath,
				TargetsPath);
			rootElement.AddImport(path);
		}

		public void Walker_OnSave()
		{
			foreach (IPackageConfigHelper packageConfigHelper in PackageBuilders)
			{
				packageConfigHelper.SavePackageFile(packageConfigHelper.GetPackageDocument());
			}

			ProjectRootElement projectRootElement;
			if (File.Exists(TargetsPath))
			{
				projectRootElement = ProjectRootElement.Open(TargetsPath);
			}
			else
			{
				projectRootElement = ProjectRootElement.Create();

			}

			var alreadyIncluded = new HashSet<string>();

			foreach (ProjectItemElement projectItemElement in items)
			{
				if(alreadyIncluded.Contains(projectItemElement.IncludeLocation.LocationString)) continue;
				alreadyIncluded.Add(projectItemElement.IncludeLocation.LocationString);
				Dictionary<string, string> metadata = projectItemElement.Metadata.ToDictionary(x=>x.Name, x=>x.Value);
				projectRootElement.AddItem(projectItemElement.ItemType, projectItemElement.Include, metadata);
			}


			projectRootElement.Save(TargetsPath);
			
		}

		private readonly HashSet<string> assemblyNames = new HashSet<string>()
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
			"System.Web.Http",
			"System.Web.Http.Owin",
			"System.Web.Http.WebHost",
			"nunit",
		};

		public ProjectRootElement _firstElement;
		private SolutionWalker _solutionWalker;
		private static readonly string TargetsPath = @"C:\Repos\Incode10\Foundation\MSBuild\NUnit.Targets";

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
			packageConfigHelper.Remove("Public.API.Dispatch.Service");
			packageConfigHelper.Remove("Public.API.Security.Service");
			packageConfigHelper.Remove("Public.API.Express.Service");
			packageConfigHelper.Remove("Tyler.API");

			packageConfigHelper.Remove("NUnit");

			PackageBuilders.Add(packageConfigHelper);

			
			string fileName = Path.GetFileNameWithoutExtension(rootElement.FullPath);

//			if (!fileName.Contains("DispatchService") && !fileName.Contains("ExpressService") &&
//			    !fileName.Contains("SecurityService") && !fileName.Equals("Windows")) return;
			
		}
	}
}

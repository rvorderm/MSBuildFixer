using Microsoft.Build.Construction;
using MSBuildFixer.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MSBuildFixer.Fixes
{
	class FixAPICore : IFix
	{
		private readonly HashSet<string> _projectPaths = new HashSet<string>();
		private const string Version = "1.0.1.108";
		private static readonly string Include = $"Api.Core, Version={Version}, Culture=neutral, PublicKeyToken=b8761d376eb50bae, processorArchitecture=MSIL";
		private static readonly string HintPath = $@"..\..\packages\Public.API.Core.{Version}\lib\net461\Api.Core.dll";
		private static readonly List<string> Services = new List<string>() {"Api.Dispatch.Service", "Api.Security.Service", "ExpressApi"};

		public void AttachTo(SolutionWalker walker)
		{
			walker.OnAfterVisitSolution += Walker_OnAfterVisitSolution;
			walker.OnVisitProjectRootItem += replaceAPICore;
			walker.OnVisitProjectRootItem += replaceAPIService;
			walker.OnVisitProjectRootItem += replaceNewtonSoft;
			walker.OnVisitProjectItem += Walker_OnVisitProjectItem;
		}

		private void Walker_OnVisitProjectItem(ProjectItemElement projectItemElement)
		{
			if (!projectItemElement.Include.Contains("Api.Core.csproj")) return;

			projectItemElement.Parent.RemoveChild(projectItemElement);
			ProjectItemElement itemElement = projectItemElement.ContainingProject.AddItem("Reference", Include);
			ProjectItemElementHelpers.AddOrUpdateReference(itemElement, Include, HintPath);
		}

		private void Walker_OnAfterVisitSolution(SolutionFile solutionFile)
		{
			foreach (string projectPath in _projectPaths)
			{
				string projectDirectory = Path.GetDirectoryName(projectPath);
				string packageFilePath = Path.Combine(projectDirectory, "packages.config");

				new PackageConfigHelper(packageFilePath)
					.AddOrUpdate("Public.API.Core", Version)
					.AddOrUpdate("Newtonsoft.Json", "8.0.3")
					.Update("Public.API.Express.Service", Version)
					.Update("Public.API.Security.Service", Version)
					.Update("Public.API.Dispatch.Service", Version)

					.SavePackageFile();
			}
		}

		private void replaceNewtonSoft(ProjectRootElement projectRootElement)
		{
			List<ProjectItemElement> oldRef = projectRootElement.Items.Where(x => x.Include.Contains("Newtonsoft.Json")).ToList();
			if (!oldRef.Any()) return;
			_projectPaths.Add(projectRootElement.FullPath);
			foreach (ProjectItemElement projectItemElement in oldRef)
			{
				projectItemElement.Include =
					"Newtonsoft.Json, Version=8.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed, processorArchitecture=MSIL";
				ProjectMetadataElement hintPath = projectItemElement.Metadata.FirstOrDefault(x => x.Name.Equals("HintPath"));
				if (hintPath != null) hintPath.Value = @"..\..\packages\Newtonsoft.Json.8.0.3\lib\net45\Newtonsoft.Json.dll";
			}
		}

		private void replaceAPIService(ProjectRootElement projectRootElement)
		{
			List<ProjectItemElement> oldApiRef = projectRootElement.Items.Where(ServicesPredicate).ToList();
			if (!oldApiRef.Any()) return;
			foreach (ProjectItemElement projectItemElement in oldApiRef)
			{
				string oldVersion = ProjectItemElementHelpers.GetIncludeVersion(projectItemElement.Include);
				projectItemElement.Include = projectItemElement.Include.Replace(oldVersion, Version);
			}
		}

		private void replaceAPICore(ProjectRootElement projectRootElement)
		{
			List<ProjectItemElement> oldApiRef = projectRootElement.Items.Where(CorePredicate).ToList();
			if (!oldApiRef.Any()) return;
			_projectPaths.Add(projectRootElement.FullPath);
			foreach (ProjectItemElement projectItemElement in oldApiRef)
			{
				ProjectItemElementHelpers.AddOrUpdateReference(projectItemElement, Include, HintPath);
			}
		}

		private static bool ServicesPredicate(ProjectItemElement x)
		{
			string assemblyName = ProjectItemElementHelpers.GetAssemblyName(x.Include);
			return Services.Contains(assemblyName);
		}

		private static bool CorePredicate(ProjectItemElement x)
		{
			return x.Include.StartsWith("Api.Core");
		}
	}
}

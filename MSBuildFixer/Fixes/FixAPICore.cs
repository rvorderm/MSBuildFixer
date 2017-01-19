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
		private const string Version = "1.0.1.103";
		private static readonly string Include = $"Api.Core, Version={Version}, Culture=neutral, PublicKeyToken=b8761d376eb50bae, processorArchitecture=MSIL";
		private static readonly string HintPath = $@"..\..\packages\Public.API.Core.{Version}\lib\net461\Api.Core.dll";

		public void AttachTo(SolutionWalker walker)
		{
			walker.OnAfterVisitSolution += Walker_OnAfterVisitSolution;
			walker.OnVisitProjectRootItem += replaceAPI;
			walker.OnVisitProjectRootItem += replaceNewtonSoft;
			walker.OnVisitProjectItem += Walker_OnVisitProjectItem;
		}

		private void Walker_OnVisitProjectItem(ProjectItemElement projectItemElement)
		{
//			if (!projectItemElement.ItemType.Equals("ProjectReference")) return;
			if (!projectItemElement.Include.Contains("Api.Core.csproj")) return;

			projectItemElement.Parent.RemoveChild(projectItemElement);
			ProjectItemElement itemElement = projectItemElement.ContainingProject.AddItem("Reference", Include);
			AddOrUpdateReference(itemElement);
		}

		private void Walker_OnAfterVisitSolution(SolutionFile solutionFile)
		{
			foreach (string projectPath in _projectPaths)
			{
				string projectDirectory = Path.GetDirectoryName(projectPath);
				string packageFilePath = Path.Combine(projectDirectory, "packages.config");

				new PackageConfigHelper(packageFilePath)
					.WithPackage("Public.API.Core", Version)
					.WithPackage("Newtonsoft.Json", "8.0.3")
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

		private void replaceAPI(ProjectRootElement projectRootElement)
		{
			List<ProjectItemElement> oldApiRef = projectRootElement.Items.Where(Predicate).ToList();
			if (!oldApiRef.Any()) return;
			_projectPaths.Add(projectRootElement.FullPath);
			foreach (ProjectItemElement projectItemElement in oldApiRef)
			{
				AddOrUpdateReference(projectItemElement);
			}
		}

		private static void AddOrUpdateReference(ProjectItemElement projectItemElement)
		{
			projectItemElement.Include = Include;
			AddOrUpdateMetaData(projectItemElement, "HintPath", HintPath);
			AddOrUpdateMetaData(projectItemElement, "SpecificVersion", false.ToString());
		}

		private static void AddOrUpdateMetaData(ProjectItemElement projectItemElement, string name, string value)
		{
			ProjectMetadataElement hintPath = projectItemElement.Metadata.FirstOrDefault(x => x.Name.Equals(name));
			if (hintPath == null)
				hintPath = projectItemElement.AddMetadata(name, value);
			else
			{
				hintPath.Value = value;
			}
		}

		private static bool Predicate(ProjectItemElement x)
		{
			return x.Include.StartsWith("Api.Core");
		}
	}
}

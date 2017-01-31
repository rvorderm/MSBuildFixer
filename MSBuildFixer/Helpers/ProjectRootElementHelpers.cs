// ReSharper disable StringIndexOfIsCultureSpecific.1
// ReSharper disable StringIndexOfIsCultureSpecific.2

using System.IO;
using Microsoft.Build.Construction;
using System.Linq;
using MSBuildFixer.Fixes;

namespace MSBuildFixer.Helpers
{
	public static class ProjectRootElementHelpers
	{
		public static string GetAssemblyName(ProjectRootElement rootElement)
		{
			return rootElement.Properties.FirstOrDefault(x=>x.Name.Equals("AssemblyName"))?.Value;
		}

		public static void AddProjectReference(ProjectRootElement projectRootElement, ProjectInSolution project)
		{
			string relativePath = FixHintPath.MakeRelativePath(projectRootElement.FullPath, project.AbsolutePath);
			ProjectItemElement itemElement = projectRootElement.AddItem("ProjectReference", relativePath);
			itemElement.AddMetadata("Project", project.ProjectGuid);
			itemElement.AddMetadata("Name", project.ProjectName);
		}

		public static string GetNugetPackagePath(ProjectRootElement rootElement)
		{
			string directoryName = Path.GetDirectoryName(rootElement.FullPath);
			string packagePath = Path.Combine(directoryName, "packages.config");
			return packagePath;
		}
	}
}

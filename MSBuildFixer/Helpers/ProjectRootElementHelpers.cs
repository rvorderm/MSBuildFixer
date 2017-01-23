// ReSharper disable StringIndexOfIsCultureSpecific.1
// ReSharper disable StringIndexOfIsCultureSpecific.2

using System.IO;
using Microsoft.Build.Construction;
using System.Linq;

namespace MSBuildFixer.Helpers
{
	public static class ProjectRootElementHelpers
	{
		public static string GetAssemblyName(ProjectRootElement rootElement)
		{
			return rootElement.Properties.FirstOrDefault(x=>x.Name.Equals("Assembly"))?.Value;
		}

		public static void AddProjectReference(ProjectRootElement projectRootElement, ProjectInSolution project)
		{
			ProjectItemElement itemElement = projectRootElement.AddItem("ProjectReference", project.RelativePath);
			itemElement.AddMetadata("Project", project.ProjectGuid);
		}

		public static string GetNugetPackagePath(ProjectRootElement rootElement)
		{
			string directoryName = Path.GetDirectoryName(rootElement.FullPath);
			string packagePath = Path.Combine(directoryName, "packages.config");
			return packagePath;
		}
	}
}

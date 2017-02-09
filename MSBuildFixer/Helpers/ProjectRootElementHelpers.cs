// ReSharper disable StringIndexOfIsCultureSpecific.1
// ReSharper disable StringIndexOfIsCultureSpecific.2

using Microsoft.Build.Construction;
using MSBuildFixer.Fixes;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MSBuildFixer.Helpers
{
	public static class ProjectRootElementHelpers
	{
		public static string GetAssemblyName(ProjectRootElement rootElement)
		{
			return GetProperties(rootElement, "AssemblyName").FirstOrDefault()?.Value;
		}

		public static IEnumerable<ProjectPropertyElement> GetProperties(ProjectRootElement rootElement, string assemblyname)
		{
			return rootElement.Properties.Where(x=>x.Name.Equals(assemblyname));
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

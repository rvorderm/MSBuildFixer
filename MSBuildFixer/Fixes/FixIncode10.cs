using Microsoft.Build.Construction;
using MSBuildFixer.Helpers;
using System.Collections.Generic;
using System.Linq;
using MSBuildFixer.Configuration;

namespace MSBuildFixer.Fixes
{
	public class FixIncode10 : IFix
	{
		public Dictionary<string, ProjectInSolution> Projects { get; set; }

		public void VisitProjects(IReadOnlyList<ProjectInSolution> projectsInSolution)
		{
			Projects = projectsInSolution.Where(x=>x.ProjectType != SolutionProjectType.SolutionFolder).ToDictionary(x=>x.ProjectName);
		}

		public void OnVisitReference(ProjectItemElement projectItemElement)
		{
			string assembly = ProjectItemElementHelpers.GetAssemblyName(projectItemElement);
			if (assembly.StartsWith("Tyler.Telemetry"))
			{
				projectItemElement.Parent.RemoveChild(projectItemElement);
			}
		}

		private void Walker_OnVisitProjectRootItem(ProjectRootElement rootElement)
		{
			string packagePath = ProjectRootElementHelpers.GetNugetPackagePath(rootElement);
			var packageConfigHelper = new PackageConfigHelper(packagePath);
			foreach (Package package in PackagesConfiguration.Instance.Packages)
			{
				packageConfigHelper.Update(package.PackageName, package.Version, PackagesConfiguration.Instance.TargetFramework);
			}
		}

		public void AttachTo(SolutionWalker walker)
		{
			walker.OnVisitProjects += VisitProjects;
			walker.OnVisitProjectItem_Reference += OnVisitReference;
		}
	}
}

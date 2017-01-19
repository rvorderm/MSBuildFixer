using Microsoft.Build.Construction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MSBuildFixer.Fixes
{
	public class FixProjectRefences : IFix
	{
		public List<Dictionary<string, ProjectInSolution>> ProjectSets { get; } =
			new List<Dictionary<string, ProjectInSolution>>();

		public void VisitProjects(IReadOnlyList<ProjectInSolution> projectsInSolution)
		{
			ProjectSets.Add(projectsInSolution.Where(x=>x.ProjectType != SolutionProjectType.SolutionFolder).ToDictionary(x=>x.ProjectName));
		}

		public void VisitProjectItem(ProjectItemElement projectItemElement)
		{
			if (!projectItemElement.ItemType.Equals("Reference")) return;

			foreach (var projectSet in ProjectSets)
			{
				var assembly = GetAssemblyFrom(projectItemElement.Include);
				ProjectInSolution project;
				if (!projectSet.TryGetValue(assembly, out project)) continue;

				projectItemElement.Parent.RemoveChild(projectItemElement);
				AddProjectReference(projectItemElement, project);
				return;
			}
		}

		private static void AddProjectReference(ProjectItemElement projectItemElement, ProjectInSolution project)
		{
			var containingProject = projectItemElement.ContainingProject;

			var itemElement = containingProject.AddItem("ProjectReference", project.RelativePath);
			itemElement.AddMetadata("Project", project.ProjectGuid);
			itemElement.AddMetadata("Name", project.ProjectName);
		}

		public static string GetAssemblyFrom(string include)
		{
			var indexOf = include.IndexOf(",", StringComparison.CurrentCulture);
			if (indexOf > 0)
			{
				return include.Substring(0, indexOf);
			}
			return include;
		}

		public void AttachTo(SolutionWalker walker)
		{
			walker.OnVisitProjects += VisitProjects;
			walker.OnVisitProjectItem += VisitProjectItem;
		}
	}
}

using Microsoft.Build.Construction;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MSBuildFixer.Fixes
{
	public class FixProjectRefences : IFix
	{
		private List<Dictionary<string, ProjectInSolution>> projectSets = new List<Dictionary<string, ProjectInSolution>>();
		public void VisitProjects(object sender, EventArgs e)
		{
			var projectInSolution = sender as ICollection<ProjectInSolution>;
			if (projectInSolution == null) return;
			projectSets.Add(projectInSolution.Where(x=>x.ProjectType != SolutionProjectType.SolutionFolder).ToDictionary(x=>x.ProjectName));
		}

		public void VisitProjectItem(object sender, EventArgs e)
		{
			var projectItemElement = sender as ProjectItemElement;
			if (projectItemElement == null) return;
			if (!projectItemElement.ItemType.Equals("Reference")) return;

			foreach (var projectSet in projectSets)
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
			itemElement.AddMetadata("Project", "{" + project.ProjectGuid + "}");
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

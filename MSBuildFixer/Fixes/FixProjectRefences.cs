using Microsoft.Build.Construction;
using MSBuildFixer.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace MSBuildFixer.Fixes
{
	public class FixProjectRefences : IFix
	{
		public Dictionary<string, ProjectInSolution> Projects { get; set; }

		public void VisitProjects(IReadOnlyList<ProjectInSolution> projectsInSolution)
		{
			Projects = projectsInSolution.Where(x=>x.ProjectType != SolutionProjectType.SolutionFolder).ToDictionary(x=>x.ProjectName);
		}

		public void OnVisitReference(ProjectItemElement projectItemElement)
		{
			string assembly = ProjectItemElementHelpers.GetAssemblyName(projectItemElement.Include);
			ProjectInSolution project;
			if (!Projects.TryGetValue(assembly, out project)) return;

			projectItemElement.Parent.RemoveChild(projectItemElement);
			AddProjectReference(projectItemElement, project);
		}

		private static void AddProjectReference(ProjectItemElement projectItemElement, ProjectInSolution project)
		{
			ProjectRootElement containingProject = projectItemElement.ContainingProject;

			ProjectItemElement itemElement = containingProject.AddItem("ProjectReference", project.RelativePath);
			itemElement.AddMetadata("Project", project.ProjectGuid);
		}

		public void AttachTo(SolutionWalker walker)
		{
			walker.OnVisitProjects += VisitProjects;
			walker.OnVisitProjectItem_Reference += OnVisitReference;
		}
	}
}

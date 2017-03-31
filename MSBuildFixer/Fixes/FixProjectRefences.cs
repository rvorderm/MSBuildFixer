using System;
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
			string assembly = ProjectItemElementHelpers.GetAssemblyName(projectItemElement);
			ProjectInSolution project;
			if (!Projects.TryGetValue(assembly, out project)) return;

			projectItemElement.Parent.RemoveChild(projectItemElement);
			ProjectRootElementHelpers.AddProjectReference(projectItemElement.ContainingProject, project);
		}

		private void OnVisitProjectReference(ProjectItemElement projectItemElement)
		{
			ProjectMetadataElement name = ProjectItemElementHelpers.GetMetadataElement(projectItemElement, "Name");
			ProjectMetadataElement project = ProjectItemElementHelpers.GetMetadataElement(projectItemElement, "Project");
			ProjectInSolution existingProject;
			if (!Projects.TryGetValue(name.Value, out existingProject)) return;
			if (!project.Value.Equals(existingProject.ProjectGuid, StringComparison.CurrentCultureIgnoreCase))
				project.Value = existingProject.ProjectGuid;
			string relativePath = PathHelpers.MakeRelativePath(projectItemElement.ContainingProject.FullPath, existingProject.AbsolutePath);
			if (!projectItemElement.Include.Equals(relativePath, StringComparison.InvariantCultureIgnoreCase))
				projectItemElement.Include = relativePath;
			if (!project.Value.Equals(existingProject.ProjectGuid, StringComparison.InvariantCultureIgnoreCase))
				project.Value = existingProject.ProjectGuid;
		}

		public void AttachTo(SolutionWalker walker)
		{
			walker.OnVisitProjects += VisitProjects;
			walker.OnVisitProjectItem_Reference += OnVisitReference;
			walker.OnVisitProjectItem_ProjectReference += OnVisitProjectReference;
		}
	}
}

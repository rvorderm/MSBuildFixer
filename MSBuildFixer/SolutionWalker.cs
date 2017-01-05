using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Construction;
using MSBuildFixer.SampleFeatureToggles;

namespace MSBuildFixer
{
	public class SolutionWalker
	{
		public event EventHandler OnOpenSolution;
		public event EventHandler OnAfterVisitSolution;

		public void VisitSolution(string solutionDirectory, string solutionFilename)
		{
			if(string.IsNullOrEmpty(solutionDirectory)) throw new ArgumentNullException(nameof(solutionDirectory));
			if(string.IsNullOrEmpty(solutionFilename)) throw new ArgumentNullException(nameof(solutionFilename));

			var solutionFilePath = Path.Combine(solutionDirectory, solutionFilename);
			OnOpenSolution?.Invoke(solutionFilePath, EventArgs.Empty);
			var solutionFile = SolutionFile.Parse(solutionFilePath);
			if (solutionFile == null) return;
		    var projectRootElements = VisitProjects(solutionFile.ProjectsInOrder);
            OnAfterVisitSolution?.Invoke(solutionFile, EventArgs.Empty);
		    foreach (var projectRootElement in projectRootElements)
		    {
		        projectRootElement?.Save();
		    }
		}

		public event EventHandler OnVisitProjects;
		public IEnumerable<ProjectRootElement> VisitProjects(IReadOnlyList<ProjectInSolution> projects)
		{
			OnVisitProjects?.Invoke(projects, EventArgs.Empty);
		    return projects.Select(VisitProject).ToList();
		}

		public event EventHandler OnOpenProjectFile;
		public ProjectRootElement VisitProject(ProjectInSolution project)
		{
			if (project.ProjectType == SolutionProjectType.SolutionFolder) return null;
			var absolutePath = project.AbsolutePath;
			if (project.ProjectType == SolutionProjectType.WebProject) return null;
			OnOpenProjectFile?.Invoke(absolutePath, EventArgs.Empty);
			var projectRootElement = ProjectRootElement.Open(absolutePath);
			if (projectRootElement == null) return null;
			VisitPropertyGroups(projectRootElement.PropertyGroups);
			VisitProjectItemGroups(projectRootElement.ItemGroups);
			return projectRootElement;
		}

		private void VisitProjectItemGroups(ICollection<ProjectItemGroupElement> projectItemGroup)
		{
			foreach (var projectItemGroupElement in projectItemGroup)
			{
				VisitProjectItemGroup(projectItemGroupElement);
			}
		}

		private void VisitProjectItemGroup(ProjectItemGroupElement projectItemGroupElement)
		{
			VisitProjectItems(projectItemGroupElement.Items);
		}

		public void VisitProjectItems(ICollection<ProjectItemElement> projectItem)
		{
			foreach (var projectItemElement in projectItem)
			{
				VisitProjectItem(projectItemElement);
			}
		}

		public event EventHandler OnVisitProjectItem;

		public void VisitProjectItem(ProjectItemElement projectItemElement)
		{
			OnVisitProjectItem?.Invoke(projectItemElement, EventArgs.Empty);
			VisitMetadataCollection(projectItemElement.Metadata);
		}

		public event EventHandler OnVisitMetadataCollection;
		public void VisitMetadataCollection(ICollection<ProjectMetadataElement> metadata)
		{
			OnVisitMetadataCollection?.Invoke(metadata, EventArgs.Empty);
			foreach (var projectElement in metadata)
			{
				VisitMetadata(projectElement);
			}
		}

		public event EventHandler OnVisitMetadata;
		public void VisitMetadata(ProjectMetadataElement projectMetadataElement)
		{
			OnVisitMetadata?.Invoke(projectMetadataElement, EventArgs.Empty);
		}

		public void VisitPropertyGroups(ICollection<ProjectPropertyGroupElement> projectPropertyGroupElements)
		{
			foreach (var projectPropertyGroupElement in projectPropertyGroupElements)
			{
				VisitPropertyGroups(projectPropertyGroupElement);
			}
		}

		public void VisitPropertyGroups(ProjectPropertyGroupElement projectPropertyGroupElement)
		{
			VisitProperties(projectPropertyGroupElement.Properties);
		}

		private void VisitProperties(ICollection<ProjectPropertyElement> projectPropertyElements)
		{
			foreach (var projectPropertyElement in projectPropertyElements)
			{
				VisitProperty(projectPropertyElement);
			}
		}

		public event EventHandler OnVisitProperty;
		public void VisitProperty(ProjectPropertyElement projectPropertyElement)
		{
			OnVisitProperty?.Invoke(projectPropertyElement, EventArgs.Empty);
		}
	}
}
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
		public void VisitSolution(string solutionDirectory, string solutionFilename)
		{
			if(string.IsNullOrEmpty(solutionDirectory)) throw new ArgumentNullException(nameof(solutionDirectory));
			if(string.IsNullOrEmpty(solutionFilename)) throw new ArgumentNullException(nameof(solutionFilename));

			var solutionFile = SolutionFile.Parse(Path.Combine(solutionDirectory, solutionFilename));
			if (solutionFile == null) return;
			VisitProjects(solutionFile.ProjectsInOrder);
		}

		public void VisitProjects(IReadOnlyList<ProjectInSolution> projects)
		{
			foreach (var projectInSolution in projects)
			{
				var projectRootElement = VisitProject(projectInSolution);
				projectRootElement?.Save();
			}
		}

		public EventHandler OnVisitComplete;

		public ProjectRootElement VisitProject(ProjectInSolution project)
		{
			if (project.ProjectType == SolutionProjectType.SolutionFolder) return null;
			var projectRootElement = ProjectRootElement.Open(project.AbsolutePath);
			if (projectRootElement == null) return null;
			VisitPropertyGroups(projectRootElement.PropertyGroups);
			VisitProjectItemGroups(projectRootElement.ItemGroups);
			OnVisitComplete?.Invoke(projectRootElement, EventArgs.Empty);
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
using Microsoft.Build.Construction;
using MSBuildFixer.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MSBuildFixer
{
	public class SolutionWalker
	{
		public delegate void OnOpenSolutionHandler(string solutionPath);
		public event OnOpenSolutionHandler OnOpenSolution;
		public delegate void AfterVisitSolutionHandler(SolutionFile solutionFile);
		public event AfterVisitSolutionHandler OnAfterVisitSolution;

		public void VisitSolution(string solutionDirectory, string solutionFilename)
		{
			if(string.IsNullOrEmpty(solutionDirectory)) throw new ArgumentNullException(nameof(solutionDirectory));
			if(string.IsNullOrEmpty(solutionFilename)) throw new ArgumentNullException(nameof(solutionFilename));
			var solutionFilePath = Path.Combine(solutionDirectory, solutionFilename);
			OnOpenSolution?.Invoke(solutionFilePath);
			var solutionFile = SolutionFile.Parse(solutionFilePath);
			if (solutionFile == null) return;
		    var projectRootElements = VisitProjects(solutionFile.ProjectsInOrder);
            OnAfterVisitSolution?.Invoke(solutionFile);
		    foreach (var projectRootElement in projectRootElements)
		    {
		        projectRootElement?.Save();
		    }
		}

		public delegate void VisitProjectsCollectionHandler(IReadOnlyList<ProjectInSolution> projects);
		public event VisitProjectsCollectionHandler OnVisitProjects;
		public IEnumerable<ProjectRootElement> VisitProjects(IReadOnlyList<ProjectInSolution> projects)
		{
			OnVisitProjects?.Invoke(projects);
		    return projects.Select(VisitProject).ToList();
		}

		public delegate void VisitProjectFileHandler(string projectPath);
		public event VisitProjectFileHandler OnOpenProjectFile;
		public delegate void VisitProjectRootElementHandler(ProjectRootElement rootElement);
		public event VisitProjectRootElementHandler OnVisitProjectRootItem;
		public ProjectRootElement VisitProject(ProjectInSolution project)
		{
			if (project.ProjectType == SolutionProjectType.SolutionFolder) return null;
			var absolutePath = project.AbsolutePath;
			if (project.ProjectType == SolutionProjectType.WebProject) return null;
			if (ExclusionConfiguration.IsExcludedProject(absolutePath)) return null;
			OnOpenProjectFile?.Invoke(absolutePath);
			ProjectRootElement projectRootElement = ProjectRootElement.Open(absolutePath);
			if (projectRootElement == null) return null;
			OnVisitProjectRootItem?.Invoke(projectRootElement);
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

		public delegate void VisitProjectItemHandler(ProjectItemElement projectItemElement);
		public event VisitProjectItemHandler OnVisitProjectItem;

		public void VisitProjectItem(ProjectItemElement projectItemElement)
		{
			OnVisitProjectItem?.Invoke(projectItemElement);
			VisitMetadataCollection(projectItemElement.Metadata);
		}

		public delegate void VisitMetadataCollectionHandler(IEnumerable<ProjectMetadataElement> metadata);
		public event VisitMetadataCollectionHandler OnVisitMetadataCollection;
		public void VisitMetadataCollection(ICollection<ProjectMetadataElement> metadata)
		{
			OnVisitMetadataCollection?.Invoke(metadata);
			foreach (var projectElement in metadata)
			{
				VisitMetadata(projectElement);
			}
		}

		public delegate void VisitMetadataHandler(ProjectMetadataElement projectMetadataElement);
		public event VisitMetadataHandler OnVisitMetadata;
		public void VisitMetadata(ProjectMetadataElement projectMetadataElement)
		{
			OnVisitMetadata?.Invoke(projectMetadataElement);
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

		public delegate void VisitPropertyHandler(ProjectPropertyElement projectPropertyElement);
		public event VisitPropertyHandler OnVisitProperty;
		public void VisitProperty(ProjectPropertyElement projectPropertyElement)
		{
			OnVisitProperty?.Invoke(projectPropertyElement);
		}
	}
}
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
		public void VisitSolution(string solutionDirectory, string solutionFilename, string libraryFolder)
		{
			if(string.IsNullOrEmpty(solutionDirectory)) throw new ArgumentNullException(nameof(solutionDirectory));
			if(string.IsNullOrEmpty(solutionFilename)) throw new ArgumentNullException(nameof(solutionFilename));
			if(string.IsNullOrEmpty(libraryFolder)) throw new ArgumentNullException(nameof(libraryFolder));

			var libraryDirectory = Path.Combine(solutionDirectory, libraryFolder);
			
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

		public ProjectRootElement VisitProject(ProjectInSolution project)
		{
			if (project.ProjectType == SolutionProjectType.SolutionFolder) return null;
			var projectRootElement = ProjectRootElement.Open(project.AbsolutePath);
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

		private void VisitProjectItem(ProjectItemElement projectItemElement)
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
				VisitMetadata(projectElement, "libraryDirectory");
			}
		}

		public event EventHandler OnVisitMetadata;
		public void VisitMetadata(ProjectMetadataElement projectMetadataElement, string libraryDirectory)
		{
			OnVisitMetadata?.Invoke(projectMetadataElement, EventArgs.Empty);
			if (projectMetadataElement == null) return;
			switch (projectMetadataElement.Name)
			{
				case "HintPath":
					TryFixHintPath(projectMetadataElement, libraryDirectory);
					break;
			}
		}

		private static void TryFixHintPath(ProjectMetadataElement projectMetadataElement, string libraryDirectory)
		{
			if (!HintPathToggle.Enabled) return;
			if (string.IsNullOrEmpty(libraryDirectory)) return;
			var fileName = Path.GetFileName(projectMetadataElement.Value);
			var libraryPath = Directory.EnumerateFiles(libraryDirectory, fileName, SearchOption.AllDirectories).LastOrDefault();
			if (libraryPath != null)
			{
				projectMetadataElement.Value = MakeRelativePath(projectMetadataElement.ContainingProject.FullPath, libraryPath);
			}
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
			VisitProperties(projectPropertyGroupElement, projectPropertyGroupElement.Properties);
		}

		private void VisitProperties(ProjectPropertyGroupElement projectPropertyGroupElement, ICollection<ProjectPropertyElement> projectPropertyElements)
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

		/// <summary>
		/// 2/20/2016 From
		/// http://stackoverflow.com/questions/275689/how-to-get-relative-path-from-absolute-path
		/// Creates a relative path from one file or folder to another.
		/// </summary>
		/// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
		/// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
		/// <returns>The relative path from the start directory to the end path or <c>toPath</c> if the paths are not related.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="UriFormatException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		public static string MakeRelativePath(string fromPath, string toPath)
		{
			if (string.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
			if (string.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

			var fromUri = new Uri(fromPath);
			var toUri = new Uri(toPath);

			if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

			var relativeUri = fromUri.MakeRelativeUri(toUri);
			var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

			if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
			{
				relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			}

			return relativePath;
		}
	}
}
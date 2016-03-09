using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Build.Construction;
using MSBuildFixer.SampleFeatureToggles;

namespace MSBuildFixer
{
	public class ProjectFixer
	{
		public void FixSolution(string solutionDirectory, string solutionFilename, string libraryFolder)
		{
			if(string.IsNullOrEmpty(solutionDirectory)) throw new ArgumentNullException(nameof(solutionDirectory));
			if(string.IsNullOrEmpty(solutionFilename)) throw new ArgumentNullException(nameof(solutionFilename));
			if(string.IsNullOrEmpty(libraryFolder)) throw new ArgumentNullException(nameof(libraryFolder));

			var libraryDirectory = Path.Combine(solutionDirectory, libraryFolder);
			
			var solutionFile = SolutionFile.Parse(Path.Combine(solutionDirectory, solutionFilename));
			if (solutionFile == null) return;
			FixProjects(solutionFile.ProjectsInOrder, libraryDirectory);
		}

		public void FixProjects(IReadOnlyList<ProjectInSolution> projects, string libraryDirectory)
		{
			foreach (var projectInSolution in projects)
			{
				var projectRootElement = FixProject(projectInSolution, libraryDirectory);
				projectRootElement?.Save();
			}
		}

		public ProjectRootElement FixProject(ProjectInSolution project, string libraryDirectory)
		{
			if (project.ProjectType == SolutionProjectType.SolutionFolder) return null;
			var projectRootElement = ProjectRootElement.Open(project.AbsolutePath);
			FixPropertyGroups(projectRootElement);
			FixProjectItemGroups(projectRootElement, libraryDirectory);
			return projectRootElement;
		}

		private void FixProjectItemGroups(ProjectRootElement projectRootElement, string libraryDirectory)
		{
			foreach (var projectItemGroupElement in projectRootElement.ItemGroups)
			{
				FixProjectItemGroup(projectItemGroupElement, libraryDirectory);
			}
		}

		private void FixProjectItemGroup(ProjectItemGroupElement projectItemGroupElement, string libraryDirectory)
		{
			foreach (var projectItemElement in projectItemGroupElement.Items)
			{
				FixProjectItemElement(projectItemElement, libraryDirectory);
			}
		}

		private void FixProjectItemElement(ProjectItemElement projectItemElement, string libraryDirectory)
		{
			if (projectItemElement.ItemType.Equals("Reference"))
			{
				FixProjectItemElementReference(projectItemElement, libraryDirectory);
			}

			if (projectItemElement.ItemType.Equals("None"))
			{
				FixProjectItemElementNone(projectItemElement);
			}
		}

		/// <summary>
		/// Things that are set to copy always will cause automatic rebuilds. These should be set to PreserveNewest to prevent that.
		/// </summary>
		/// <param name="projectItemElement"></param>
		private void FixProjectItemElementNone(ProjectItemElement projectItemElement)
		{
			if (!CopyToOutputDirectoryToggle.Enabled) return;
			foreach (var projectElement in projectItemElement.Metadata)
			{
				if (projectElement.Name.Equals("CopyToOutputDirectory") && projectElement.Value.Equals("Always"))
				{
					projectElement.Value = "PreserveNewest";
				}
			}
		}

		private void FixProjectItemElementReference(ProjectItemElement projectItemElement, string libraryDirectory)
		{
			foreach (var projectElement in projectItemElement.Metadata)
			{
				FixMetadataElement(projectElement, libraryDirectory);
			}
			if (projectItemElement.Metadata.Any()
			    && !projectItemElement.Metadata.Any(x => x.Name.Equals("Private"))
			    && !(projectItemElement.Include.StartsWith("System.")
			         || projectItemElement.Include.StartsWith("Microsoft")))
			{
				projectItemElement.AddMetadata("Private", false.ToString());
			}
		}

		private void FixMetadataElement(ProjectMetadataElement projectMetadataElement, string libraryDirectory)
		{
			if (projectMetadataElement == null) return;
			switch (projectMetadataElement.Name)
			{
				case "Private":
					MakePrivate(projectMetadataElement);
					break;
				case "HintPath":
					TryFixHintPath(projectMetadataElement, libraryDirectory);
					break;
			}
		}

		private static void TryFixHintPath(ProjectMetadataElement projectMetadataElement, string libraryDirectory)
		{
			if (!HintPathToggle.Enabled) return;
			if (string.IsNullOrEmpty(libraryDirectory)
			    || projectMetadataElement.Value.Contains(".lib\\")) return;

			var fileName = Path.GetFileName(projectMetadataElement.Value);
			var libraryPath = Directory.EnumerateFiles(libraryDirectory, fileName, SearchOption.AllDirectories).FirstOrDefault();
			if (libraryPath != null)
			{
				projectMetadataElement.Value = MakeRelativePath(projectMetadataElement.ContainingProject.FullPath, libraryPath);
			}
		}

		private static void MakePrivate(ProjectMetadataElement projectMetadataElement)
		{
			if (!CopyLocalToggle.Enabled) return;
			projectMetadataElement.Value = false.ToString();
		}

		public void FixPropertyGroups(ProjectRootElement projectRootElement)
		{
			foreach (var projectPropertyGroupElement in projectRootElement.PropertyGroups)
			{
				FixPropertyGroup(projectPropertyGroupElement);
			}
		}

		public void FixPropertyGroup(ProjectPropertyGroupElement projectPropertyGroupElement)
		{
			foreach (var projectPropertyElement in projectPropertyGroupElement.Properties)
			{
				FixOutputPath(projectPropertyElement, projectPropertyGroupElement.Condition);
			}
		}

		public void FixOutputPath(ProjectPropertyElement projectPropertyElement, string propertyGroupCondition)
		{
			if (OutputPathToggle.Enabled && projectPropertyElement.Name.Equals("OutputPath"))
			{
				FixOutputProperty(projectPropertyElement, propertyGroupCondition);
			}

			if (RunPostBuildEventToggle.Enabled && projectPropertyElement.Name.Equals("RunPostBuildEvent"))
			{
				FixRunPostBuildEvent(projectPropertyElement);
			}
		}

		private void FixRunPostBuildEvent(ProjectPropertyElement projectPropertyElement)
		{
			projectPropertyElement.Value = "OnOutputUpdated";
		}

		public void FixOutputProperty(ProjectPropertyElement projectPropertyElement, string propertyGroupCondition)
		{
			var configuration = GetConfiguration(propertyGroupCondition);
			projectPropertyElement.Value = Path.Combine("$(SolutionDir)", "bin", configuration);
		}

		public static string GetConfiguration(string propertyGroupCondition)
		{
			/* 
			Complicated regex that I want to match the configuration word and nothing else.
			For example, I want to find Debug in:
			'$(Configuration)|$(Platform)' == 'Debug|AnyCPU' 
			 */
			var match = Regex.Match(propertyGroupCondition, @"(?:\'\$\(Configuration\)\|\$\(Platform\)\' == \')(.*)(?:\|(?:.*)\')");
			if (match.Groups.Count < 2) return string.Empty;
			return match?.Groups[1].Value;
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
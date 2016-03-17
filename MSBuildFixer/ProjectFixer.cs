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
		private readonly string _libraryDirectory;
		private readonly SolutionFile _solutionFile;
		private readonly string _solutionDirectory;

		internal ProjectFixer()
		{
			
		}

		public ProjectFixer(string solutionDirectory, string solutionFilename, string libraryFolder)
		{
			if (string.IsNullOrEmpty(solutionDirectory)) throw new ArgumentNullException(nameof(solutionDirectory));
			if (string.IsNullOrEmpty(solutionFilename)) throw new ArgumentNullException(nameof(solutionFilename));
			if (string.IsNullOrEmpty(libraryFolder)) throw new ArgumentNullException(nameof(libraryFolder));

			_solutionDirectory = solutionDirectory;
			_libraryDirectory = Path.Combine(_solutionDirectory, libraryFolder);

			_solutionFile = SolutionFile.Parse(Path.Combine(_solutionDirectory, solutionFilename));
			if(_solutionFile == null) throw new Exception("Unable to open solution file");
		}

		public void FixSolution()
		{
			if(_solutionFile == null) throw new ArgumentNullException(nameof(_solutionFile));
			FixProjects(_solutionFile.ProjectsInOrder);
		}

		public void FixProjects(IReadOnlyList<ProjectInSolution> projects)
		{
			foreach (var projectInSolution in projects)
			{
				var projectRootElement = FixProject(projectInSolution);
				projectRootElement?.Save();
			}
		}

		public ProjectRootElement FixProject(ProjectInSolution project)
		{
			if (project.ProjectType == SolutionProjectType.SolutionFolder) return null;
			var projectRootElement = ProjectRootElement.Open(project.AbsolutePath);
			FixPropertyGroups(projectRootElement);
			FixProjectItemGroups(projectRootElement);
			return projectRootElement;
		}

		private void FixProjectItemGroups(ProjectRootElement projectRootElement)
		{
			foreach (var projectItemGroupElement in projectRootElement.ItemGroups)
			{
				FixProjectItemGroup(projectItemGroupElement);
			}
		}

		private void FixProjectItemGroup(ProjectItemGroupElement projectItemGroupElement)
		{
			foreach (var projectItemElement in projectItemGroupElement.Items)
			{
				FixProjectItemElement(projectItemElement);
			}
		}

		private void FixProjectItemElement(ProjectItemElement projectItemElement)
		{
			if (projectItemElement.ItemType.Equals("Reference"))
			{
				FixProjectItemElementReference(projectItemElement);
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

		private void FixProjectItemElementReference(ProjectItemElement projectItemElement)
		{
			foreach (var projectElement in projectItemElement.Metadata)
			{
				FixMetadataElement(projectElement);
			}
			AddMissingMetadataElements(projectItemElement);
		}

		private static void AddMissingMetadataElements(ProjectItemElement projectItemElement)
		{
			if (CopyLocalToggle.Enabled 
				&& projectItemElement.Metadata.Any()
			    && !projectItemElement.Metadata.Any(x => x.Name.Equals("Private"))
			    && !(projectItemElement.Include.StartsWith("System.")
			         || projectItemElement.Include.StartsWith("Microsoft")))
			{
				projectItemElement.AddMetadata("Private", false.ToString());
			}
		}

		private void FixMetadataElement(ProjectMetadataElement projectMetadataElement)
		{
			if (projectMetadataElement == null) return;
			switch (projectMetadataElement.Name)
			{
				case "Private":
					MakePrivate(projectMetadataElement);
					break;
				case "HintPath":
					TryFixHintPath(projectMetadataElement);
					break;
			}
		}

		private void TryFixHintPath(ProjectMetadataElement projectMetadataElement)
		{
			if (!HintPathToggle.Enabled) return;
			if (string.IsNullOrEmpty(_libraryDirectory)) throw new ArgumentException("Must specify a library directory to try fixing the hint path", nameof(_libraryDirectory));
			var fileName = Path.GetFileName(projectMetadataElement.Value);
			var libraryPath = Directory.EnumerateFiles(_libraryDirectory, fileName, SearchOption.AllDirectories).LastOrDefault();
			if (libraryPath != null)
			{
				projectMetadataElement.Value = libraryPath.Replace(_solutionDirectory, "$(SolutionDir)");
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
	}
}
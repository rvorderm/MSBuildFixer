using FeatureToggle.Core;
using Microsoft.Build.Construction;
using MSBuildFixer.Configuration;
using MSBuildFixer.FeatureToggles;
using MSBuildFixer.Fixes;
using MSBuildFixer.Reports;
using MSBuildFixer.SampleFeatureToggles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MSBuildFixer
{
	public class SolutionWalker
	{
		private readonly string _fullSolutionPath;

		public delegate void OnOpenSolutionHandler(string solutionPath);
		public event OnOpenSolutionHandler OnOpenSolution;
		public delegate void AfterVisitSolutionHandler(SolutionFile solutionFile);
		public event AfterVisitSolutionHandler OnAfterVisitSolution;

		public delegate void OnSaveHandler();

		public event OnSaveHandler OnSave;

		public SolutionWalker(string fullSolutionPath)
		{
			_fullSolutionPath = fullSolutionPath;
		}

		public IEnumerable<ProjectRootElement> VisitSolution(bool save = true)
		{
			OnOpenSolution?.Invoke(_fullSolutionPath);
			SolutionFile solutionFile = SolutionFile.Parse(_fullSolutionPath);
			if (solutionFile == null) return null;
			IEnumerable<ProjectRootElement> projectRootElements = VisitProjects(solutionFile.ProjectsInOrder);
			OnAfterVisitSolution?.Invoke(solutionFile);
			if (!save) return projectRootElements;
			OnSave?.Invoke();
			foreach (ProjectRootElement projectRootElement in projectRootElements)
			{
				projectRootElement?.Save();
			}
			return projectRootElements;
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
		public event VisitProjectItemHandler OnVisitProjectItem_Reference;
		public event VisitProjectItemHandler OnVisitProjectItem_Compile;
		public event VisitProjectItemHandler OnVisitProjectItem_ProjectReference;
		public event VisitProjectItemHandler OnVisitProjectItem_Other;

		public void VisitProjectItem(ProjectItemElement projectItemElement)
		{
			switch (projectItemElement.ItemType)
			{
				case "Reference":
					OnVisitProjectItem_Reference?.Invoke(projectItemElement);
					break;
				case "Compile":
					OnVisitProjectItem_Compile?.Invoke(projectItemElement);
					break;
				case "ProjectReference":
					OnVisitProjectItem_ProjectReference?.Invoke(projectItemElement);
					break;
				default:
					OnVisitProjectItem_Other?.Invoke(projectItemElement);
					break;
			}
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

		public static SolutionWalker CreateWalker(string fullSolutionPath)
		{
			if (!File.Exists(fullSolutionPath))
			{
				Console.WriteLine($"SolutionPath null or directory did not exist: {fullSolutionPath}");
				throw new ArgumentException(fullSolutionPath);
			}

			Console.WriteLine($"Opening {fullSolutionPath}");

			SolutionWalker walker = new SolutionWalker(fullSolutionPath);
			Attach<MergeBinFolders>(MergeBinFoldersToggle.Instance, walker);
			Attach<FixCopyLocal>(CopyLocalToggle.Instance, walker);
			Attach<FixColocateAssemblyInfo>(ColocateAssemblyInfoToggle.Instance, walker);
			Attach<FixCopyToOutputDirectory>(CopyToOutputDirectoryToggle.Instance, walker);
			Attach<FixHintPath>(HintPathToggle.Instance, walker);
			Attach<FixOutputPath>(OutputPathToggle.Instance, walker);
			Attach<FixProjectRefences>(ProjectReferencesToggle.Instance, walker);
			Attach<FixPackages>(PackagesConfiguration.Instance.Packages.Any(), walker);
			Attach<FixReferenceVersion>(ReferenceVersionToggle.Instance.Enabled, walker);
			Attach<FixXCopy>(FixXCopyToggle.Instance, walker);
			Attach<FixProperties>(FixesConfiguration.Instance.Properties.Any(), walker);
			//AttachScriptBuilder();
//			new ListUntrackedProjectFiles().AttachTo(walker);
			Attach<ListProjectsWithReferences>(!string.IsNullOrEmpty(ReportsConfiguration.Instance.ReferenceRegex), walker);
			Attach<FixReplaceProjectReferences>(FixesConfiguration.Instance.ProjectReferenceReplacements.Any(), walker);
			Attach<FixFileEncoding>(FixesConfiguration.Instance.FixProjectFileEncodings, walker);
			new FixIncode10().AttachTo(walker);
			return walker;
		}

		public static void Attach<T>(IFeatureToggle copyLocalToggle, SolutionWalker walker)
			where T : IFix, new()
		{
			Attach<T>(copyLocalToggle.FeatureEnabled, walker);
		}

		public static void Attach<T>(bool shouldAttach, SolutionWalker walker)
			where T : IFix, new()
		{
			if (shouldAttach) new T().AttachTo(walker);
		}

		public static void AttachScriptBuilder()
		{
			var scriptBuilder = new ScriptBuilder();
			if (BuildCopyScriptsToggle.Enabled)
			{
				scriptBuilder.BuildScripts();
			}
		}
	}
}
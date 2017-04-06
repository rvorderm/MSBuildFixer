using Microsoft.Build.Construction;
using MSBuildFixer.Configuration;
using MSBuildFixer.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml;

namespace MSBuildFixer.Fixes
{
	public enum CopyStyle
	{
		NoCopying,
		FirstOccurence,
		LastOccurence,
		MoveAllToFirstProject,
	    DoNothing
	}

	public class FixCopyLocal : IFix
	{
		private ProjectRootElement _firstElement;
		private readonly Dictionary<string, string> _originalHintPaths = new Dictionary<string, string>();
		private readonly HashSet<string> _visitedIncludes = new HashSet<string>();
		private readonly Dictionary<string, ProjectItemElement> _visitedElements = new Dictionary<string, ProjectItemElement>();
		private Dictionary<string, Dictionary<Version, string>> _packageFiles;
		public CopyStyle CopyStyle { get; set; } = FixesConfiguration.Instance.CopyStyle;

		public void OnVisitProjectItem(ProjectItemElement projectItemElement)
		{
			if (IsGacAssembly(projectItemElement)) return;
			switch (CopyStyle)
			{
				case CopyStyle.NoCopying:
					ProjectItemElementHelpers.AddOrUpdateMetaData(projectItemElement, "Private", false.ToString());
					break;
				case CopyStyle.FirstOccurence:
					CaseFirstOnly(projectItemElement);
					break;
				case CopyStyle.LastOccurence:
					CaseLastOnly(projectItemElement);
					break;
				case CopyStyle.MoveAllToFirstProject:
					CaseMoveAllToFirstProject(projectItemElement);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		private void CaseMoveAllToFirstProject(ProjectItemElement projectItemElement)
		{
			if (!projectItemElement.ItemType.Equals("Reference")) return;
			ProjectMetadataElement hintPath = ProjectItemElementHelpers.GetHintPath(projectItemElement);
			if (hintPath != null)
			{
				string include = ProjectItemElementHelpers.GetAssemblyName(projectItemElement);
				string value;
				if (!_originalHintPaths.TryGetValue(include, out value))
				{
					_originalHintPaths[include] = hintPath.Value;
				}
				else
				{
					string newPath = ProjectItemElementHelpers.GetHintPathVersion(projectItemElement);
					string existingPath = ProjectItemElementHelpers.GetHintPathVersion(value, include);
					if (existingPath == null && !string.IsNullOrEmpty(newPath))
					{
						_originalHintPaths[include] = newPath;
					}
					else if (!string.IsNullOrEmpty(existingPath) && !string.IsNullOrEmpty(newPath))
					{
						var newVersion = new Version(newPath);
						var existingVersion = new Version(existingPath);
						if (existingVersion < newVersion)
						{
							_originalHintPaths[include] = hintPath.Value;
						}
					}
				}
			}

			if (projectItemElement.ContainingProject == _firstElement)
			{
				projectItemElement.Parent.RemoveChild(projectItemElement);
//				ProjectItemElementHelpers.AddOrUpdateMetaData(projectItemElement, "Private", true.ToString());
//				projectItemElement.Include = ProjectItemElementHelpers.GetAssemblyName(projectItemElement);
			}
			else
			{
				projectItemElement.Include = ProjectItemElementHelpers.GetAssemblyName(projectItemElement);
				ProjectItemElementHelpers.AddOrUpdateMetaData(projectItemElement, "Private", false.ToString());
				ProjectItemElementHelpers.AddOrUpdateMetaData(projectItemElement, "HintPath", "$(OutputPath)");
			}
		}

		private void CaseLastOnly(ProjectItemElement projectItemElement)
		{
			ProjectItemElementHelpers.AddOrUpdateMetaData(projectItemElement, "Private", false.ToString());
			_visitedElements[projectItemElement.Include] = projectItemElement;
		}

		private void CaseFirstOnly(ProjectItemElement projectItemElement)
		{
			bool visited = _visitedIncludes.Contains(projectItemElement.Include);
			_visitedIncludes.Add(projectItemElement.Include);
			ProjectItemElementHelpers.AddOrUpdateMetaData(projectItemElement, "Private", (!visited).ToString());
		}

		public static bool IsGacAssembly(ProjectItemElement projectItemElement)
		{
			return projectItemElement.Include.StartsWith("System")
				   || projectItemElement.Include.StartsWith("Microsoft");
		}

		public void AttachTo(SolutionWalker walker)
		{
		    if (FixesConfiguration.Instance.CopyStyle == CopyStyle.DoNothing) return;
			walker.OnOpenSolution += Walker_OnOpenSolution;
			walker.OnVisitProjectItem_Reference += OnVisitProjectItem;
			walker.OnVisitProjectItem_ProjectReference += OnVisitProjectItem;
			walker.OnAfterVisitSolution += Walker_OnAfterVisitSolution;
			walker.OnVisitProjectRootItem += Walker_OnVisitProjectRootItem;
		}

		private void Walker_OnOpenSolution(string solutionPath)
		{
			if (CopyStyle != CopyStyle.MoveAllToFirstProject) return;
			string solutionDirectory = Path.GetDirectoryName(solutionPath);
			string nugetConfigPath = Path.Combine(solutionDirectory, "nuget.config");
			if (!File.Exists(nugetConfigPath)) return;
			var nugetConfig = new XmlDocument();
			nugetConfig.Load(nugetConfigPath);
			XmlNode xmlNode = nugetConfig.SelectSingleNode("//add[@key='repositoryPath']");
			if (xmlNode == null) return;
			XmlAttribute xmlNodeAttribute = xmlNode.Attributes["value"];
			string packagesFolder = Path.Combine(solutionDirectory, xmlNodeAttribute.Value);
			_packageFiles = new Dictionary<string, Dictionary<Version, string>>();
			foreach (string filePath in Directory.EnumerateFiles(packagesFolder, "", SearchOption.AllDirectories))
			{
				string fileName = Path.GetFileName(filePath);
				FileVersionInfo fileVersionInfo = FileVersionInfo.GetVersionInfo(filePath);
				var version = new Version(fileVersionInfo.FileVersion);
				Dictionary<Version, string> innerDictionary;
				if (!_packageFiles.TryGetValue(fileName, out innerDictionary))
				{
					_packageFiles[fileName] = innerDictionary = new Dictionary<Version, string>();
				}
				innerDictionary[version] = filePath;
			}
		}

		private void Walker_OnVisitProjectRootItem(ProjectRootElement rootElement)
		{
			if (_firstElement != null) return;
			_firstElement = rootElement;
		}

		private void Walker_OnAfterVisitSolution(SolutionFile solutionFile)
		{
			switch (CopyStyle)
			{
				case CopyStyle.NoCopying:
				case CopyStyle.FirstOccurence:
				case CopyStyle.LastOccurence:
					foreach (ProjectItemElement visitedElementsValue in _visitedElements.Values)
					{
						ProjectMetadataElement projectMetadataElement = ProjectItemElementHelpers.GetPrivate(visitedElementsValue);
						if (projectMetadataElement != null)
						{
							visitedElementsValue.RemoveChild(projectMetadataElement);
						}
					}
					break;
				case CopyStyle.MoveAllToFirstProject:

					var packageConfigHelper = new PackageConfigHelper(_firstElement.FullPath);
					foreach (KeyValuePair<string, string> originalHintPath in _originalHintPaths)
					{
						ProjectItemElement projectItemElement = _firstElement.Items.FirstOrDefault(x=>x.Include.Equals(originalHintPath.Key) || x.Include.StartsWith($"{originalHintPath.Key},"));
						if (projectItemElement != null)
						{
							ProjectItemElementHelpers.AddOrUpdateMetaData(projectItemElement, "Private", true.ToString());
						}
						else
						{
							ProjectItemElement itemElement = _firstElement.AddItem("Reference", originalHintPath.Key);
							ProjectItemElementHelpers.AddOrUpdateMetaData(itemElement, "Private", true.ToString());
							if (!originalHintPath.Value.Equals(@"$(OutputPath)"))
							{
								ProjectItemElementHelpers.AddOrUpdateMetaData(itemElement, "HintPath", originalHintPath.Value);

							}
						}
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}

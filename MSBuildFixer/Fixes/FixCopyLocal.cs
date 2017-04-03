using Microsoft.Build.Construction;
using MSBuildFixer.Configuration;
using MSBuildFixer.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

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
		private Dictionary<string, string> _originalHintPaths = new Dictionary<string, string>();
		private readonly HashSet<string> _visitedIncludes = new HashSet<string>();
		private readonly Dictionary<string, ProjectItemElement> _visitedElements = new Dictionary<string, ProjectItemElement>();
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
			if (projectItemElement.ContainingProject == _firstElement)
			{
				ProjectItemElementHelpers.AddOrUpdateMetaData(projectItemElement, "Private", true.ToString());
			}
			else
			{
				ProjectMetadataElement hintPath = ProjectItemElementHelpers.GetHintPath(projectItemElement);
				if (hintPath != null)
				{
				    string include = projectItemElement.Include.Replace(", processorArchitecture=MSIL", string.Empty);
				    _originalHintPaths[include] = hintPath.Value;
				}
				projectItemElement.Include = ProjectItemElementHelpers.GetAssemblyName(projectItemElement);
				ProjectItemElementHelpers.AddOrUpdateMetaData(projectItemElement, "Private", false.ToString());
				ProjectItemElementHelpers.AddOrUpdateMetaData(projectItemElement, "HintPath", @"$(OutputPath)");
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
			walker.OnVisitProjectItem_Reference += OnVisitProjectItem;
			walker.OnVisitProjectItem_ProjectReference += OnVisitProjectItem;
			walker.OnAfterVisitSolution += Walker_OnAfterVisitSolution;
			walker.OnVisitProjectRootItem += Walker_OnVisitProjectRootItem;
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
					foreach (KeyValuePair<string, string> originalHintPath in _originalHintPaths)
					{
						ProjectItemElement projectItemElement = _firstElement.Items.FirstOrDefault(x=>x.Include.Contains(originalHintPath.Key));
						if (projectItemElement != null)
						{
							ProjectItemElementHelpers.AddOrUpdateMetaData(projectItemElement, "Private", true.ToString());
						}
						else
						{
							ProjectItemElement itemElement = _firstElement.AddItem("Reference", originalHintPath.Key);
							ProjectItemElementHelpers.AddOrUpdateMetaData(itemElement, "Private", true.ToString());
							ProjectItemElementHelpers.AddOrUpdateMetaData(itemElement, "HintPath", originalHintPath.Value);
						}
					}
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
	}
}

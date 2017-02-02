using System;
using Microsoft.Build.Construction;
using System.Collections.Generic;
using System.Linq;
using MSBuildFixer.Helpers;

namespace MSBuildFixer.Fixes
{
	public enum CopyStyle
	{
		NoCopying,
		FirstOnly,
		LastOnly,
	}

	public class FixCopyLocal : IFix
	{
		private readonly HashSet<string> _visitedIncludes = new HashSet<string>();
		private readonly Dictionary<string, ProjectItemElement> _visitedElements = new Dictionary<string, ProjectItemElement>();
		public CopyStyle CopyStyle { get; set; } = CopyStyle.NoCopying;

		public void OnVisitProjectItem(ProjectItemElement projectItemElement)
		{
			if (IsGacAssembly(projectItemElement)) return;
			switch (CopyStyle)
			{
				case CopyStyle.NoCopying:
					ProjectItemElementHelpers.AddOrUpdateMetaData(projectItemElement, "Private", false.ToString());
					break;
				case CopyStyle.FirstOnly:
					CaseFirstOnly(projectItemElement);
					break;
				case CopyStyle.LastOnly:
					CaseLastOnly(projectItemElement);
					break;
				default:
					throw new ArgumentOutOfRangeException();
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
			walker.OnVisitProjectItem_Reference += OnVisitProjectItem;
			walker.OnVisitProjectItem_ProjectReference += OnVisitProjectItem;
			walker.OnAfterVisitSolution += Walker_OnAfterVisitSolution;
		}

		private void Walker_OnAfterVisitSolution(SolutionFile solutionFile)
		{
			foreach (ProjectItemElement visitedElementsValue in _visitedElements.Values)
			{
				ProjectMetadataElement projectMetadataElement = ProjectItemElementHelpers.GetPrivate(visitedElementsValue);
				if (projectMetadataElement != null)
				{
					visitedElementsValue.RemoveChild(projectMetadataElement);
				}
			}
		}
	}
}

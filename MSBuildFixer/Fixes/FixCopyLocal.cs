using Microsoft.Build.Construction;
using System.Collections.Generic;
using System.Linq;

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

		public void OnVisitMetadata(ProjectMetadataElement projectMetadataElement)
		{
			if (!projectMetadataElement.Name.Equals("Private")) return;
			projectMetadataElement.Value = false.ToString();
		}

		public void OnVisitProjectItem(ProjectItemElement projectItemElement)
		{
			if (IsGacAssembly(projectItemElement)) return;
			ProcessMetadata(projectItemElement);
		}

		private void RemovePrivateMetaData(ProjectItemElement projectItemElement)
		{
			ProjectMetadataElement privateMetadata = GetPrivateMetadata(projectItemElement);
			projectItemElement?.RemoveChild(privateMetadata);
		}

		private void ProcessMetadata(ProjectItemElement projectItemElement)
		{
		    switch (CopyStyle)
		    {
                case CopyStyle.NoCopying:
		            CaseNoCopying(projectItemElement);
		            break;
                case CopyStyle.FirstOnly:
                    CaseFirstOnly(projectItemElement);
		            break;
                case CopyStyle.LastOnly:
		            CaseLastOnly(projectItemElement);
		            break;
		    }
		}

	    private void CaseLastOnly(ProjectItemElement projectItemElement)
	    {
            CaseNoCopying(projectItemElement);
	        _visitedElements[projectItemElement.Include] = projectItemElement;
	    }

	    private void CaseFirstOnly(ProjectItemElement projectItemElement)
	    {
	        var projectMetadataElement = GetPrivateMetadata(projectItemElement);
	        var isPrivate = IsPrivate(projectItemElement);
	        if (projectMetadataElement == null)
	        {
	            if (isPrivate)
	            {
	                projectItemElement.AddMetadata("Private", false.ToString());
	            }
	        }
	        else
	        {
	            if (isPrivate)
	            {
	                projectMetadataElement.Value = false.ToString();
	            }
	            else
	            {
                    projectItemElement?.RemoveChild(projectMetadataElement);
                }
	        }
	    }

	    private void CaseNoCopying(ProjectItemElement projectItemElement)
	    {
	        var projectMetadataElement = GetPrivateMetadata(projectItemElement);
	        if (projectMetadataElement == null)
	        {
	            projectItemElement.AddMetadata("Private", false.ToString());
	        }
	        else
	        {
	            projectMetadataElement.Value = false.ToString();
	        }
	    }

	    private bool IsPrivate(ProjectItemElement projectItemElement)
		{
			string include = projectItemElement.Include;
			if (_visitedIncludes.Contains(include)) return true;
			_visitedIncludes.Add(include);
			return false;
		}

		public static ProjectMetadataElement GetPrivateMetadata(ProjectItemElement projectItemElement)
		{
			return projectItemElement.Metadata.FirstOrDefault(x => x.Name.Equals("Private"));
		}

		public static bool IsGacAssembly(ProjectItemElement projectItemElement)
		{
			return projectItemElement.Include.StartsWith("System")
			       || projectItemElement.Include.StartsWith("Microsoft");
		}

		public void AttachTo(SolutionWalker walker)
		{
//			walker.OnVisitMetadata += OnVisitMetadata;
			walker.OnVisitProjectItem_Reference += OnVisitProjectItem;
			walker.OnVisitProjectItem_ProjectReference += OnVisitProjectItem;
            walker.OnAfterVisitSolution += Walker_OnAfterVisitSolution;
		}

        private void Walker_OnAfterVisitSolution(SolutionFile solutionFile)
        {
            foreach (var visitedElementsValue in _visitedElements.Values)
            {
                RemovePrivateMetaData(visitedElementsValue);
            }
        }
    }
}

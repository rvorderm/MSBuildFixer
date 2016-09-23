using Microsoft.Build.Construction;
using MSBuildFixer.SampleFeatureToggles;
using System;
using System.Linq;

namespace MSBuildFixer.Fixes
{
	public class FixCopyLocal : IFix
	{
		public void OnVisitMetadata(object sender, EventArgs eventArgs)
		{
			var projectMetadataElement = sender as ProjectMetadataElement;
			if (projectMetadataElement == null) return;
			if (!CopyLocalToggle.Enabled) return;
			if (!projectMetadataElement.Name.Equals("Private")) return;
			
			projectMetadataElement.Value = false.ToString();
		}

		public void OnVisitProjectItem(object sender, EventArgs eventArgs)
		{
			var projectItemElement = sender as ProjectItemElement;
			if (projectItemElement == null) return;
			if (!CopyLocalToggle.Enabled) return;
			if (!(projectItemElement.ItemType.Equals("Reference") || projectItemElement.ItemType.Equals("ProjectReference"))) return;
			if (IsGacAssembly(projectItemElement)) return;
			if (HasPrivateMetadata(projectItemElement)) return;
			
			projectItemElement.AddMetadata("Private", false.ToString());
		}

		public static bool HasPrivateMetadata(ProjectItemElement projectItemElement)
		{
			return projectItemElement.Metadata.Any(x => x.Name.Equals("Private"));
		}

		public static bool IsGacAssembly(ProjectItemElement projectItemElement)
		{
			return projectItemElement.Include.StartsWith("System")
			       || projectItemElement.Include.StartsWith("Microsoft");
		}

		public void AttachTo(SolutionWalker walker)
		{
			walker.OnVisitMetadata += OnVisitMetadata;
			walker.OnVisitProjectItem += OnVisitProjectItem;
		}
	}
}

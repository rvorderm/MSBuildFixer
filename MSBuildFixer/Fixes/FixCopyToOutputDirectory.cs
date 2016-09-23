using Microsoft.Build.Construction;
using MSBuildFixer.SampleFeatureToggles;
using System;

namespace MSBuildFixer.Fixes
{
	public class FixCopyToOutputDirectory : IFix
	{
		public void OnVisitMetadata(object sender, EventArgs eventArgs)
		{
			var metadataElement = sender as ProjectMetadataElement;
			if (metadataElement == null) return;
			if (!CopyToOutputDirectoryToggle.Enabled) return;
			if (!metadataElement.Name.Equals("CopyToOutputDirectory")) return;

			metadataElement.Value = "PreserveNewest";
		}

		public void AttachTo(SolutionWalker walker)
		{
			walker.OnVisitMetadata += OnVisitMetadata;
		}
	}
}

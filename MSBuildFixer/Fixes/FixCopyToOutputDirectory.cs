using Microsoft.Build.Construction;
using MSBuildFixer.SampleFeatureToggles;

namespace MSBuildFixer.Fixes
{
	public class FixCopyToOutputDirectory : IFix
	{
		public void OnVisitMetadata(ProjectMetadataElement metadataElement)
		{
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

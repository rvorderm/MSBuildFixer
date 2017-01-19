using Microsoft.Build.Construction;
using MSBuildFixer.SampleFeatureToggles;

namespace MSBuildFixer.Fixes
{
	public class FixRunPostBuildEvent : IFix
	{
		public void OnVisitProperty(ProjectPropertyElement projectPropertyElement)
		{
			if (!RunPostBuildEventToggle.Enabled) return;
			if (!projectPropertyElement.Name.Equals("RunPostBuildEvent")) return;
			
			projectPropertyElement.Value = "OnOutputUpdated";
		}

		public void AttachTo(SolutionWalker walker)
		{
			walker.OnVisitProperty += OnVisitProperty;
		}
	}
}

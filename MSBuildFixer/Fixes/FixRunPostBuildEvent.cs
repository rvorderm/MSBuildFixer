using Microsoft.Build.Construction;
using MSBuildFixer.SampleFeatureToggles;
using System;

namespace MSBuildFixer.Fixes
{
	public class FixRunPostBuildEvent : IFix
	{
		public void OnVisitProperty(object sender, EventArgs e)
		{
			var projectPropertyElement = sender as ProjectPropertyElement;
			if (projectPropertyElement == null) return;
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

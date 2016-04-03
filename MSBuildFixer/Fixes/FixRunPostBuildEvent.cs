using System;
using System.IO;
using Microsoft.Build.Construction;
using MSBuildFixer.SampleFeatureToggles;

namespace MSBuildFixer.Fixes
{
	public class FixRunPostBuildEvent
	{
		public static void OnVisitProperty(object sender, EventArgs e)
		{
			var projectPropertyElement = sender as ProjectPropertyElement;
			if (projectPropertyElement == null) return;
			if (!RunPostBuildEventToggle.Enabled) return;
			if (!projectPropertyElement.Name.Equals("RunPostBuildEvent")) return;
			
			projectPropertyElement.Value = "OnOutputUpdated";
		}
	}
}

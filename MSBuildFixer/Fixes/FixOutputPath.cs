using Microsoft.Build.Construction;
using MSBuildFixer.SampleFeatureToggles;
using System;
using System.IO;

namespace MSBuildFixer.Fixes
{
	public class FixOutputPath : IFix
	{
		public void OnVisitProperty(object sender, EventArgs e)
		{
			var projectPropertyElement = sender as ProjectPropertyElement;
			if (projectPropertyElement == null) return;
			if (!OutputPathToggle.Enabled) return;
			if (!projectPropertyElement.Name.Equals("OutputPath")) return;

			projectPropertyElement.Value = Path.Combine("$(SolutionDir)", "bin", "$(Configuration)");
		}

		public void AttachTo(SolutionWalker walker)
		{
			walker.OnVisitProperty += OnVisitProperty;
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeatureToggle.Core;
using Microsoft.Build.Construction;
using MSBuildFixer.FeatureToggles;
using MSBuildFixer.SampleFeatureToggles;
using static System.String;

namespace MSBuildFixer.Fixes
{
	public class FixOutputPath
	{
		public void OnVisitProperty(object sender, EventArgs e)
		{
			var projectPropertyElement = sender as ProjectPropertyElement;
			if (projectPropertyElement == null) return;
			if (!OutputPathToggle.Enabled) return;
			if (!projectPropertyElement.Name.Equals("OutputPath")) return;

			projectPropertyElement.Value = Path.Combine("$(SolutionDir)", "bin", "$(Configuration)");
		}
	}
}

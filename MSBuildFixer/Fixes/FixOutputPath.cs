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

			if (!UseRelativePathing.Enabled)
			{
				projectPropertyElement.Value = Path.Combine("$(SolutionDir)", "bin", "$(Configuration)");
			}
			else
			{
				var relativePath = FixHintPath.MakeRelativePath(projectPropertyElement.ContainingProject.FullPath, solutionFolder);
				projectPropertyElement.Value = Path.Combine(relativePath, "bin", "$(Configuration)");
			}
		}

		private string solutionFolder;
		private void OnOpenSolution(Object sender, EventArgs e)
		{
			var path = sender as string;
			if (string.IsNullOrEmpty(path)) return;
			solutionFolder = Path.GetDirectoryName(path);
			if (!solutionFolder.EndsWith(@"\")) solutionFolder += @"\";
		}

		public void AttachTo(SolutionWalker walker)
		{
			walker.OnVisitProperty += OnVisitProperty;
			walker.OnOpenSolution += OnOpenSolution;
		}
	}
}

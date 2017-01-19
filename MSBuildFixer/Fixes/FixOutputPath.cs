using Microsoft.Build.Construction;
using MSBuildFixer.SampleFeatureToggles;
using System.IO;

namespace MSBuildFixer.Fixes
{
	public class FixOutputPath : IFix
	{
		public void OnVisitProperty(ProjectPropertyElement projectPropertyElement)
		{
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
		private void OnOpenSolution(string path)
		{
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

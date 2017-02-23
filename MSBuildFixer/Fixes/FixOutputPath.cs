using Microsoft.Build.Construction;
using MSBuildFixer.SampleFeatureToggles;
using System.IO;
using MSBuildFixer.Helpers;

namespace MSBuildFixer.Fixes
{
	public class FixOutputPath : IFix
	{
		public void OnVisitProperty(ProjectPropertyElement projectPropertyElement)
		{
			if (!projectPropertyElement.Name.Equals("OutputPath")) return;

			if (!UseRelativePathing.Enabled)
			{
				projectPropertyElement.Value = Path.Combine("$(SolutionDir)", "bin", "$(Configuration)");
			}
			else
			{
				string relativePath = PathHelpers.MakeRelativePath(projectPropertyElement.ContainingProject.FullPath, solutionFolder);
				projectPropertyElement.Value = Path.Combine(relativePath, "bin", "$(Configuration)");
			}
		}

		private string solutionFolder;

		public void OnOpenSolution(string path)
		{
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

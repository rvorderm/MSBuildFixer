using Microsoft.Build.Construction;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MSBuildFixer.Fixes
{
	public class FixFileEncoding : IFix
	{
		public void AttachTo(SolutionWalker walker)
		{
			walker.OnOpenSolution += Walker_OnOpenSolution;
		}

		private void Walker_OnOpenSolution(string solutionPath)
		{
			SolutionFile solutionFile = SolutionFile.Parse(solutionPath);
			IEnumerable<string> files = solutionFile.ProjectsInOrder.Where(x=>x.ProjectType.Equals(SolutionProjectType.KnownToBeMSBuildFormat)).Select(x=>x.AbsolutePath);
			foreach (string file in files)
			{
				string contents = File.ReadAllText(file);
				File.WriteAllText(file, contents, Encoding.UTF8);
			}
		}
	}
}

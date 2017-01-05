using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Configuration.ConfigurationManager;

namespace MSBuildFixer.Fixes
{
	class ListUntrackedProjectFiles : IFix
	{
		public ListUntrackedProjectFiles(string fullSolutionPath)
		{
			var solutionDirectory = Path.GetDirectoryName(fullSolutionPath);
			IEnumerable<string> files = Directory.EnumerateFiles(solutionDirectory, "*.csproj", SearchOption.AllDirectories);
			allFiles = new HashSet<string>(files);
		}

		public void AttachTo(SolutionWalker walker)
		{
			walker.OnOpenProjectFile += Walker_OnOpenProjectFile;
			walker.OnAfterVisitSolution += Walker_OnAfterVisitSolution;
		}

		private readonly HashSet<string> visitedPaths = new HashSet<string>();
		private HashSet<string> allFiles;

		private void Walker_OnAfterVisitSolution(object sender, EventArgs e)
		{
			IEnumerable<string> unbuiltFiles = allFiles.Except(visitedPaths).ToList();
			var path = Path.Combine(Environment.CurrentDirectory, "UnbuiltFiles.txt");
			File.WriteAllLines(path, unbuiltFiles);
		}

		private void Walker_OnOpenProjectFile(object sender, EventArgs e)
		{
			var path = sender as string;
			visitedPaths.Add(path);
		}
	}
}

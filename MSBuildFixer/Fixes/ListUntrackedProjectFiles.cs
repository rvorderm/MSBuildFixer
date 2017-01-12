using Microsoft.Build.Construction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

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

		private void Walker_OnAfterVisitSolution(SolutionFile solutionFile)
		{
			IEnumerable<string> unbuiltFiles = allFiles.Except(visitedPaths).ToList();
			var path = Path.Combine(Environment.CurrentDirectory, "UnbuiltFiles.txt");
			File.WriteAllLines(path, unbuiltFiles);
		}

		private void Walker_OnOpenProjectFile(string path)
		{
			visitedPaths.Add(path);
		}
	}
}

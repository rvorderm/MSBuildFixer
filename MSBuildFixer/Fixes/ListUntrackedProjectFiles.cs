using Microsoft.Build.Construction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MSBuildFixer.Fixes
{
	public class ListUntrackedProjectFiles : IFix
	{

		public void AttachTo(SolutionWalker walker)
		{
			walker.OnOpenProjectFile += Walker_OnOpenProjectFile;
			walker.OnAfterVisitSolution += Walker_OnAfterVisitSolution;
			walker.OnOpenSolution += Walker_OnOpenSolution;
		}

		private void Walker_OnOpenSolution(string solutionPath)
		{
			string solutionDirectory = Path.GetDirectoryName(solutionPath);
			IEnumerable<string> files = Directory.EnumerateFiles(solutionDirectory, "*.csproj", SearchOption.AllDirectories);
			foreach (string file in files)
			{
				_allFiles.Add(file);
			}
		}

		private readonly HashSet<string> _visitedPaths = new HashSet<string>();
		private readonly HashSet<string> _allFiles = new HashSet<string>();

		private void Walker_OnAfterVisitSolution(SolutionFile solutionFile)
		{
			IEnumerable<string> unbuiltFiles = _allFiles.Except(_visitedPaths).ToList();
			var path = Path.Combine(Environment.CurrentDirectory, "UnbuiltFiles.txt");
			File.WriteAllLines(path, unbuiltFiles);
		}

		private void Walker_OnOpenProjectFile(string path)
		{
			_visitedPaths.Add(path);
		}
	}
}

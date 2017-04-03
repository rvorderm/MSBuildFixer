using Microsoft.Build.Construction;
using Serilog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MSBuildFixer.Configuration;
using Serilog.Context;

namespace MSBuildFixer.Reports
{
	public class ListUntrackedProjectFiles : IFix
	{
		private readonly HashSet<string> _allFiles = new HashSet<string>();

		private readonly HashSet<string> _visitedPaths = new HashSet<string>();
		private string _solutionPath;

		public void AttachTo(SolutionWalker walker)
		{
			walker.OnOpenProjectFile += Walker_OnOpenProjectFile;
			walker.OnAfterVisitSolution += Walker_OnAfterVisitSolution;
			walker.OnOpenSolution += Walker_OnOpenSolution;
		}

		private void Walker_OnOpenSolution(string solutionPath)
		{
			_solutionPath = solutionPath;
			string solutionDirectory = Path.GetDirectoryName(_solutionPath);
			IEnumerable<string> files = Directory.EnumerateFiles(solutionDirectory, "*.csproj", SearchOption.AllDirectories);
			foreach (string file in files)
				_allFiles.Add(file);
		}

		private void Walker_OnAfterVisitSolution(SolutionFile solutionFile)
		{
			IEnumerable<string> unbuiltFiles = _allFiles.Except(_visitedPaths).ToList();

			using (LogContext.PushProperty(ReportsConfiguration.PropertyName, "Unbuilt files"))
			{
				if (unbuiltFiles.Any())
					Log.Information("The following files were found that are not built by the solution {solution}", _solutionPath);
				foreach (string unbuiltFile in unbuiltFiles)
					Log.Information("The solution does not build file {file}", unbuiltFile);
			}
		}

		private void Walker_OnOpenProjectFile(string path)
		{
			_visitedPaths.Add(path);
		}
	}
}
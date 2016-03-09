using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Microsoft.Build.Construction;
using MSBuildFixer.SampleFeatureToggles;
using static System.Configuration.ConfigurationManager;

namespace MSBuildFixer
{
	public class Program
	{
		static void Main(string[] args)
		{
			var fullSolutionPath = AppSettings["SolutionPath"];
			var libraryFolder = AppSettings["LibraryFolder"];

			var solutionDirectory = Path.GetDirectoryName(fullSolutionPath);
			var solutionFilename = Path.GetFileName(fullSolutionPath);
			if (solutionDirectory == null || !Directory.Exists(solutionDirectory))
			{
				Console.WriteLine($"SolutionPath null or directory did not exist: {fullSolutionPath}");
				return;
			}

			if (!File.Exists(fullSolutionPath)) return;

			var libraryPath = Path.Combine(solutionDirectory, libraryFolder);
			if (!Directory.Exists(libraryPath))
			{
				Console.WriteLine($"LibraryFolder did not exist: {libraryPath}");
				Console.WriteLine($"Hint path option will not be executed");
				HintPathToggle.Enabled = false;
			}

			Console.WriteLine($"Opening {fullSolutionPath}");

			Environment.CurrentDirectory = solutionDirectory;
			var projectFixer = new ProjectFixer();
			projectFixer.FixSolution(solutionDirectory, solutionFilename, libraryFolder);

			Console.WriteLine($"Finished");
		}
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Microsoft.Build.Construction;
using MSBuildFixer.Fixes;
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

			Console.WriteLine($"Opening {fullSolutionPath}");

			Environment.CurrentDirectory = solutionDirectory;
			var projectFixer = new SolutionWalker();
			AttachFixes(projectFixer);
			projectFixer.VisitSolution(solutionDirectory, solutionFilename);

			Console.WriteLine($"Finished");
		}

		private static void AttachFixes(SolutionWalker projectFixer)
		{
			if (CopyLocalToggle.Enabled)
			{
				var fixCopyLocal = new FixCopyLocal();
				projectFixer.OnVisitMetadata += fixCopyLocal.OnVisitMetadata;
				projectFixer.OnVisitProjectItem += fixCopyLocal.OnVisitProjectItem;
			}
		}
	}
}

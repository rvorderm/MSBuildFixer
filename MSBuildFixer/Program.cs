﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Mail;
using System.Text.RegularExpressions;
using FeatureToggle.Core;
using Microsoft.Build.Construction;
using MSBuildFixer.FeatureToggles;
using MSBuildFixer.Fixes;
using MSBuildFixer.SampleFeatureToggles;
using MSBuildFixerTests;
using static System.Configuration.ConfigurationManager;

namespace MSBuildFixer
{
	public class Program
	{
		static void Main(string[] args)
		{
			var fullSolutionPath = AppSettings["SolutionPath"];

			var solutionDirectory = Path.GetDirectoryName(fullSolutionPath);
			var solutionFilename = Path.GetFileName(fullSolutionPath);
			if (solutionDirectory == null || !Directory.Exists(solutionDirectory))
			{
				Console.WriteLine($"SolutionPath null or directory did not exist: {fullSolutionPath}");
				return;
			}

			if (!File.Exists(fullSolutionPath)) return;


			Console.WriteLine($"Opening {fullSolutionPath}");

			Environment.CurrentDirectory = solutionDirectory;
			var projectFixer = new SolutionWalker();
			AttachFixes(projectFixer);
			projectFixer.VisitSolution(solutionDirectory, solutionFilename);

			Console.WriteLine($"Finished");
		}

		private static void AttachFixes(SolutionWalker walker)
		{
			Attach(CopyLocalToggle.Instance, AttachCopyLocal, walker);
			Attach(CopyToOutputDirectoryToggle.Instance, AttachCopyToOutputDirectory, walker);
			Attach(HintPathToggle.Instance, AttachHintPath, walker);
			Attach(OutputPathToggle.Instance, AttachOutputPath, walker);
			Attach(RunPostBuildEventToggle.Instance, AttachRunPostBuildEvent, walker);
			Attach(FixXCopyToggle.Instance, AttachXCopy, walker);
			AttachScriptBuilder();


		}

		private static void Attach(IFeatureToggle copyLocalToggle, Action<SolutionWalker> attachCopyLocal, SolutionWalker walker)
		{
			if (copyLocalToggle.FeatureEnabled) attachCopyLocal(walker);
		}

		private static void AttachCopyLocal(SolutionWalker walker)
		{
			var fixCopyLocal = new FixCopyLocal();
			walker.OnVisitMetadata += fixCopyLocal.OnVisitMetadata;
			walker.OnVisitProjectItem += fixCopyLocal.OnVisitProjectItem;
		}

		private static void AttachCopyToOutputDirectory(SolutionWalker walker)
		{
			var fixCopyToOutputDirectory = new FixCopyToOutputDirectory();
			walker.OnVisitMetadata += fixCopyToOutputDirectory.OnVisitMetadata;
		}

		private static void AttachHintPath(SolutionWalker walker)
		{
			var fullSolutionPath = AppSettings["SolutionPath"];
			var libraryFolder = AppSettings["LibraryFolder"];
			var solutionDirectory = Path.GetDirectoryName(fullSolutionPath);

			var fixHintPath = new FixHintPath(solutionDirectory, libraryFolder);
			walker.OnVisitMetadata += fixHintPath.OnVisitMetadata;
		}

		private static void AttachOutputPath(SolutionWalker walker)
		{
			var fixOutputPath = new FixOutputPath();
			walker.OnVisitProperty += fixOutputPath.OnVisitProperty;
		}

		private static void AttachRunPostBuildEvent(SolutionWalker walker)
		{
			var fixRunPostBuildEvent = new FixRunPostBuildEvent();
			walker.OnVisitProperty += fixRunPostBuildEvent.OnVisitProperty;
		}

		private static void AttachXCopy(SolutionWalker walker)
		{
			var fileName = AppSettings["SummarizeXCopyToggle_FileName"];
			var fixXCopy = new FixXCopy(fileName);
			walker.OnVisitProperty += fixXCopy.OnVisitProperty;
			walker.OnOpenSolution += fixXCopy.OnOpenSolution;
			walker.OnAfterVisitSolution += fixXCopy.OnAfterVisitSolution;
		}

		private static void AttachScriptBuilder()
		{
			var directoryName = Path.GetDirectoryName(AppSettings["SolutionPath"]);
			var target = AppSettings["BuildCopyScripts_Target"];
			var destinations = AppSettings["BuildCopyScripts_Destinations"].Split(';');
			var scriptBuilder = new ScriptBuilder(directoryName, target, destinations);
			scriptBuilder.BuildScripts();
		}
	}
}

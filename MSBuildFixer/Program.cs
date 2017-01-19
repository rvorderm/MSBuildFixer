﻿using FeatureToggle.Core;
using MSBuildFixer.Configuration;
using MSBuildFixer.FeatureToggles;
using MSBuildFixer.Fixes;
using MSBuildFixer.SampleFeatureToggles;
using System;
using System.IO;
using static System.Configuration.ConfigurationManager;

namespace MSBuildFixer
{
	public class Program
	{
		static void Main(string[] args)
		{
			SolutionConfiguration solutionConfiguration = (dynamic)GetSection("solutionConfiguration");

			foreach (var fullSolutionPath in solutionConfiguration.Solutions)
			{
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
				AttachFixes(projectFixer, fullSolutionPath);
				projectFixer.VisitSolution(solutionDirectory, solutionFilename);
			}

			Console.WriteLine($"Finished");
		}

		private static void AttachFixes(SolutionWalker walker, string solutionPath)
		{
			Attach<MergeBinFolders>(MergeBinFoldersToggle.Instance, walker);
			Attach<FixCopyLocal>(CopyLocalToggle.Instance, walker);
			Attach<FixColocateAssemblyInfo>(ColocateAssemblyInfoToggle.Instance, walker);
			Attach<FixCopyToOutputDirectory>(CopyToOutputDirectoryToggle.Instance, walker);
			Attach<FixHintPath>(HintPathToggle.Instance, walker);
			Attach<FixOutputPath>(OutputPathToggle.Instance, walker);
			Attach<FixRunPostBuildEvent>(RunPostBuildEventToggle.Instance, walker);
			Attach<FixProjectRefences>(ProjectReferencesToggle.Instance, walker);
			Attach<FixReferenceVersion>(ReferenceVersionToggle.Instance, walker);
			Attach<FixXCopy>(FixXCopyToggle.Instance, walker);
			//Attach<FixTargetFramework>(FixTargetFrameworkToggle.Instance, walker);
			//AttachScriptBuilder();
			//new ListUntrackedProjectFiles(solutionPath).AttachTo(walker);
//			Attach<ListProjectsWithReferences>(!string.IsNullOrEmpty(ListProjectsWithReferences.ReferenceRegex), walker);
			new FixAPICore().AttachTo(walker);
//			new FixCodeSigning().AttachTo(walker);
		}

		private static void Attach<T>(IFeatureToggle copyLocalToggle, SolutionWalker walker)
			where T : IFix, new()
		{
			Attach<T>(copyLocalToggle.FeatureEnabled, walker);
		}

		private static void Attach<T>(bool shouldAttach, SolutionWalker walker)
			where T : IFix, new()
		{
			if (shouldAttach) new T().AttachTo(walker);
		}

		private static void AttachScriptBuilder()
		{
			var scriptBuilder = new ScriptBuilder();
			if (BuildCopyScriptsToggle.Enabled)
			{
				scriptBuilder.BuildScripts();
			}
		}
	}
}

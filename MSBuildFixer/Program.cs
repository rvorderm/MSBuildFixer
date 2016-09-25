using FeatureToggle.Core;
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
			Attach<MergeBinFolders>(MergeBinFoldersToggle.Instance, walker);
			Attach<FixCopyLocal>(CopyLocalToggle.Instance, walker);
			Attach<FixCopyToOutputDirectory>(CopyToOutputDirectoryToggle.Instance, walker);
			Attach<FixHintPath>(HintPathToggle.Instance, walker);
			Attach<FixOutputPath>(OutputPathToggle.Instance, walker);
			Attach<FixRunPostBuildEvent>(RunPostBuildEventToggle.Instance, walker);
			Attach<FixProjectRefences>(ProjectReferencesToggle.Instance, walker);
			Attach<FixReferenceVersion>(ReferenceVersionToggle.Instance, walker);
			Attach<FixXCopy>(FixXCopyToggle.Instance, walker);
			Attach<FixTargetFramework>(FixTargetFrameworkToggle.Instance, walker);
			AttachScriptBuilder();


		}

		private static void Attach<T>(IFeatureToggle copyLocalToggle, SolutionWalker walker)
			where T : IFix, new()
		{
			if (copyLocalToggle.FeatureEnabled) new T().AttachTo(walker);
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

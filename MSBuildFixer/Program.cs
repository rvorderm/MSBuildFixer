using FeatureToggle.Core;
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

			foreach (string fullSolutionPath in solutionConfiguration.Solutions)
			{
				SolutionWalker projectFixer = SolutionWalker.CreateWalker(fullSolutionPath);
				projectFixer.VisitSolution();
			}

			Console.WriteLine($"Finished");
		}
	}
}

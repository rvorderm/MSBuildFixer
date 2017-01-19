using MSBuildFixer.Configuration;
using System;

namespace MSBuildFixer
{
	public class Program
	{
		static void Main(string[] args)
		{
			foreach (string fullSolutionPath in SolutionConfiguration.Instance.Solutions)
			{
				SolutionWalker projectFixer = SolutionWalker.CreateWalker(fullSolutionPath);
				projectFixer.VisitSolution();
			}

			Console.WriteLine($"Finished");
		}
	}
}

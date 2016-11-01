using Microsoft.Build.Construction;
using MSBuildFixer.Configuration;
using System.Linq;

namespace MSBuildFixer.Fixes
{
	public class UnifySolutions
	{
		public void GenerateSolutionFile()
		{
			var unified = new SolutionFile();
			Solution unifiedSolution = workspace.OpenSolutionAsync(UnifySolutionsConfiguration.Instance.FileName).Result;

			foreach (var solutionPath in SolutionConfiguration.Instance.Solutions)
			{
				var file = SolutionFile.Parse(solutionPath);
				file.
				MSBuildWorkspace workspaceToMerge = MSBuildWorkspace.Create();
				Solution solution = workspaceToMerge.OpenSolutionAsync(solutionPath).Result;
				foreach (Project solutionProject in solution.Projects)
				{
					if (!unifiedSolution.ProjectIds.Contains(solutionProject.Id))
					{
						unifiedSolution = unifiedSolution.AddProject(solutionProject.Id, solutionProject.Name, solutionProject.AssemblyName, solutionProject.Language);
					}
				}
			}
			workspace.TryApplyChanges(unifiedSolution);
			workspace.CloseSolution();
		}
	}
}

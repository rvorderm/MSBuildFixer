using Microsoft.Build.Construction;
using MSBuildFixer.Configuration;
using MSBuildFixer.Helpers;
using Serilog;
using Serilog.Context;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;

namespace MSBuildFixer.Reports
{
	public class ListCircularDependencies : IFix
	{
		private readonly Dictionary<string, List<string>> cycles = new Dictionary<string, List<string>>();
		private readonly Dictionary<string, HashSet<string>> edgeSets = new Dictionary<string, HashSet<string>>();
		private ProjectReferenceCounter sourceFinder;
		public void AttachTo(SolutionWalker walker)
		{
			sourceFinder = new ProjectReferenceCounter();
			sourceFinder.AttachTo(walker);
			walker.OnVisitProjectItem_ProjectReference += Walker_OnVisitProjectItem_ProjectReference;
			walker.OnAfterVisitSolution += Walker_OnAfterVisitSolution;
		}

		private void Walker_OnAfterVisitSolution(SolutionFile solutionFile)
		{
			using (LogContext.PushProperty(ReportsConfiguration.PropertyName, "Cycle Analysis"))
			{
				Log.Information("Beginning search for circular references.");

//				foreach (string source in sourceFinder.GetProjects((x,y)=>y==0))
//				{
//					var cycle = new List<string>();
//					DFS(source, cycle);
//				}
				Log.Information("{count} cycles were found", cycles.Count);
				foreach (KeyValuePair<string, List<string>> cycle in cycles)
				{
					Log.Information("Cycle detected for project {project}:",
						Path.GetFileNameWithoutExtension(cycle.Key));
					foreach (string path in cycle.Value)
					{
						Log.Information("-> {path}", Path.GetFileNameWithoutExtension(path));
					}
				}
			}
		}

		private void DFS(string source, List<string> cycle)
		{
			if (cycles.ContainsKey(source)) return;
			HashSet<string> dependencies;
			if (!edgeSets.TryGetValue(source, out dependencies))
				return;

			cycle.Add(source);

			int start = cycle.IndexOf(source);
			if (start != cycle.Count - 1)
			{
				cycles[source] = new List<string>(cycle.Skip(start));
				return;
			}

			foreach (string dependency in dependencies)
				DFS(dependency, cycle);
			cycle.RemoveAt(cycle.Count-1);
		}

		private void Walker_OnVisitProjectItem_ProjectReference(ProjectItemElement projectItemElement)
		{
			ProjectRootElement projectRootElement = projectItemElement.ContainingProject;
			string path = PathHelpers.GetAbsolutePath(Path.Combine(projectRootElement.DirectoryPath, projectItemElement.Include));

			HashSet<string> dependencies;
			if (!edgeSets.TryGetValue(projectRootElement.FullPath, out dependencies))
				edgeSets[projectRootElement.FullPath] = dependencies = new HashSet<string>();
			dependencies.Add(path);
		}
	}
}
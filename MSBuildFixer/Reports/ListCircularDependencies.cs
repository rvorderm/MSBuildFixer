using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Construction;
using MSBuildFixer.Configuration;
using MSBuildFixer.Helpers;
using Serilog;
using Serilog.Context;

namespace MSBuildFixer.Reports
{
	public class ListCircularDependencies : IFix
	{
		private readonly Dictionary<string, HashSet<string>> edgeSets = new Dictionary<string, HashSet<string>>();
		private readonly HashSet<string> sources = new HashSet<string>();

		public void AttachTo(SolutionWalker walker)
		{
			walker.OnVisitProjectRootItem += Walker_OnVisitProjectRootItem;
			walker.OnVisitProjectItem_ProjectReference += Walker_OnVisitProjectItem_ProjectReference;
			walker.OnAfterVisitSolution += Walker_OnAfterVisitSolution;
		}

		private void Walker_OnVisitProjectRootItem(ProjectRootElement rootElement)
		{
			sources.Add(rootElement.FullPath);
		}

		private void Walker_OnAfterVisitSolution(SolutionFile solutionFile)
		{
			using (LogContext.PushProperty(ReportsConfiguration.PropertyName, "Cycle Analysis"))
			{
				Log.Information("Beginning search for circular references.");

				foreach (string source in sources)
				{
					var cycle = new List<string>();
					if (DFS(source, cycle))
						Log.Information("Cycle detected for project {project}: {cycle}", source,
							string.Join($"{Environment.NewLine}-> ", cycle));
				}
			}
		}

		private bool DFS(string source, ICollection<string> cycle)
		{
			HashSet<string> dependencies;
			if (!edgeSets.TryGetValue(source, out dependencies))
				return false;

			if (cycle.Contains(source))
			{
				cycle.Add(source);
				return true;
			}

			cycle.Add(source);
			return dependencies.Any(dependency => DFS(dependency, cycle));
		}

		private void Walker_OnVisitProjectItem_ProjectReference(ProjectItemElement projectItemElement)
		{
			ProjectRootElement projectRootElement = projectItemElement.ContainingProject;
			string path = PathHelpers.GetAbsolutePath(Path.Combine(projectRootElement.DirectoryPath, projectItemElement.Include));

			HashSet<string> dependencies;
			if (!edgeSets.TryGetValue(projectRootElement.FullPath, out dependencies))
				edgeSets[projectRootElement.FullPath] = dependencies = new HashSet<string>();
			dependencies.Add(path);

			//This is how we figure out what isn't referenced by anything for a DFS search later
			if (sources.Contains(path)) sources.Remove(path);
		}
	}
}
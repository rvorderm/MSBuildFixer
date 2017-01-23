using Microsoft.Build.Construction;
using MSBuildFixer.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace MSBuildFixer.Reports
{
	public class ListProjectsWithReferences : IFix
	{
		public ListProjectsWithReferences()
		{
			_regex = new Regex(ReferenceRegex);
		}

		public void AttachTo(SolutionWalker walker)
		{
			walker.OnVisitProjectItem_Reference += Walker_OnVisitProjectItem;
			walker.OnVisitProjectItem_ProjectReference += Walker_OnVisitProjectItem;
			walker.OnAfterVisitSolution += Walker_OnAfterVisitSolution;
		}

		private void Walker_OnAfterVisitSolution(SolutionFile solutionFile)
		{
			string path = Path.Combine(Environment.CurrentDirectory, "ProjectsWithReference.txt");
			List<string> list = allFiles.OrderBy(x=>x).ToList();
			list = list.Select(x=>x.Replace(Environment.CurrentDirectory, string.Empty)).ToList();
			File.WriteAllLines(path, list);
		}

		private void Walker_OnVisitProjectItem(ProjectItemElement projectItemElement)
		{
			Match match = _regex.Match(projectItemElement.Include);
			if (match.Success) allFiles.Add(projectItemElement.ContainingProject.FullPath);
		}



		public static string ReferenceRegex { get; set; } = ReportsConfiguration.Instance.ReferenceRegex;

		private readonly HashSet<string> allFiles = new HashSet<string>();
		private readonly Regex _regex;
	}
}

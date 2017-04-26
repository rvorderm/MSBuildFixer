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
	public class ProjectReferenceCounter : IFix
	{
		private readonly Dictionary<string, Holder> counts = new Dictionary<string, Holder>();

		public void AttachTo(SolutionWalker walker)
		{
			walker.OnVisitProjects += Walker_OnVisitProjects;
			walker.OnVisitProjectItem_ProjectReference += Walker_OnVisitProjectItem_ProjectReference;
			walker.OnVisitProjectItem_Reference += Walker_OnVisitProjectItem_Reference;
			walker.OnAfterVisitSolution += Walker_OnAfterVisitSolution;
		}

		private void Walker_OnVisitProjects(IReadOnlyList<ProjectInSolution> projects)
		{
			foreach (ProjectInSolution project in projects)
			{
				counts[project.AbsolutePath] = new Holder()
				{
					Type = project.ProjectType
				};
			}
		}

		private void Walker_OnVisitProjectItem_ProjectReference(ProjectItemElement projectItemElement)
		{
			string projectDir = projectItemElement.ContainingProject.DirectoryPath;
			string path = PathHelpers.GetAbsolutePath(Path.Combine(projectDir, projectItemElement.Include));
			if (counts.ContainsKey(path))
			{
				Holder holder = counts[path];
				holder.Count++;
				holder.References.Add(projectItemElement.ContainingProject.FullPath);
			}
		}

		private void Walker_OnVisitProjectItem_Reference(ProjectItemElement projectItemElement)
		{
			string projectDir = projectItemElement.ContainingProject.DirectoryPath;
			ProjectMetadataElement projectMetadataElement = ProjectItemElementHelpers.GetHintPath(projectItemElement);
			if (projectMetadataElement == null) return;
			if (File.Exists(projectMetadataElement.Value))
			{
				if (counts.ContainsKey(projectMetadataElement.Value))
				{
					Holder holder = counts[projectMetadataElement.Value];
					holder.Count++;
					holder.References.Add(projectItemElement.ContainingProject.FullPath);
				}
			}
			else
			{
				string path = PathHelpers.GetAbsolutePath(Path.Combine(projectDir, projectItemElement.Include));
				if (File.Exists(path))
				{
					Holder holder = counts[projectMetadataElement.Value];
					holder.Count++;
					holder.References.Add(projectItemElement.ContainingProject.FullPath);
				}
			}
		}

		public IEnumerable<string> GetProjects(Func<string, int, bool> selector)
		{
			return counts.Where(x => selector(x.Key, x.Value.Count)).Select(x => x.Key).ToList();
		}



		private void Walker_OnAfterVisitSolution(SolutionFile solutionFile)
		{
			using (LogContext.PushProperty(ReportsConfiguration.PropertyName, "Project Counts"))
			{
				Log.Information("Reference Counts for each project:");

				foreach (KeyValuePair<string, Holder> count in counts)
				{
					if(count.Value.Count==0)
						Log.Information("Project {project} with type {type} was referenced {times} times", Path.GetFileNameWithoutExtension(count.Key), count.Value.Type, count.Value.Count);

				}
				foreach (KeyValuePair<string, Holder> count in counts)
				{
					if (count.Value.Count != 0)
					{
						Log.Information("Project {project} was referenced {times} times", Path.GetFileName(count.Key), count.Value.Count);
						foreach (string reference in count.Value.References)
						{
							Log.Debug("Project {project} was referenced by reference {reference}", Path.GetFileNameWithoutExtension(count.Key), Path.GetFileNameWithoutExtension(reference));
						}
					}
				}
			}
		}

		private class Holder
		{
			public int Count { get; set; }
			public List<string> References { get; set; } = new List<string>();
			public SolutionProjectType Type { get; set; }
		}
	}
}
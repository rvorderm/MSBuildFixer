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
		private readonly Dictionary<string, Holder> Projects = new Dictionary<string, Holder>();
		private readonly Dictionary<string, Holder> Assemblies = new Dictionary<string, Holder>();

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
				Projects[project.AbsolutePath] = new Holder(project.ProjectName, project.ProjectType);
			}
		}

		private void Walker_OnVisitProjectItem_ProjectReference(ProjectItemElement projectItemElement)
		{
			string sourceDir = projectItemElement.ContainingProject.DirectoryPath;
			string sourcePath = projectItemElement.ContainingProject.FullPath;
			string destinationPath = PathHelpers.GetAbsolutePath(Path.Combine(sourceDir, projectItemElement.Include));

			UpdateHolders(sourcePath, destinationPath);
		}

		private void UpdateHolders(string sourcePath, string destinationPath)
		{
			Holder source;
			if (Projects.TryGetValue(sourcePath, out source))
			{
				source.AddOutgoing(destinationPath);
			}

			Holder destination;
			if (Projects.TryGetValue(destinationPath, out destination))
			{
				destination.AddIncoming(sourcePath);
			}
		}

		private void Walker_OnVisitProjectItem_Reference(ProjectItemElement projectItemElement)
		{
			string sourceDir = projectItemElement.ContainingProject.DirectoryPath;
			string sourcePath = projectItemElement.ContainingProject.FullPath;
			string assemblyPath = null;
			
			ProjectMetadataElement projectMetadataElement = ProjectItemElementHelpers.GetHintPath(projectItemElement);
			if (projectMetadataElement == null) return;
			if (File.Exists(projectMetadataElement.Value))
			{
				assemblyPath = projectMetadataElement.Value;
			}
			else
			{
				string path = PathHelpers.GetAbsolutePath(Path.Combine(sourceDir, projectItemElement.Include));
				if (File.Exists(path))
				{
					assemblyPath = path;
				}
			}

			if (string.IsNullOrEmpty(assemblyPath)) return;
			Holder source;
			if (Projects.TryGetValue(sourcePath, out source))
			{
				source.AddAssembly(sourcePath);
			}
			
			Holder assembly;
			if (Assemblies.TryGetValue(assemblyPath, out assembly))
			{
				assembly.AddIncoming(sourcePath);
			}
			else
			{
				var holder = new Holder(Path.GetFileName(assemblyPath));
				holder.AddIncoming(sourcePath);
				Assemblies.Add(assemblyPath, holder);
			}
		}

		private void Walker_OnAfterVisitSolution(SolutionFile solutionFile)
		{
			using (LogContext.PushProperty(ReportsConfiguration.PropertyName, "Project Counts"))
			{
				Log.Information("Reference Counts for each project:");

				foreach (KeyValuePair<string, Holder> count in Projects.OrderByDescending(x => x.Value.IncomingCount))
				{
					string projectName = Path.GetFileNameWithoutExtension(count.Key);
					Log.Information("Project {projectName} with type {type} was referenced {times} times",
						projectName, count.Value.Type, count.Value.IncomingCount);

					foreach (string outgoingReference in count.Value.OutgoingReferences)
					{
						string outgoingReferenceName = Path.GetFileNameWithoutExtension(outgoingReference);
						Log.Debug("Project {projectName} referenced the following project {reference}", projectName, outgoingReferenceName);
					}

					foreach (string incomingReference in count.Value.IncomingReferences)
					{
						string incomingReferenceName = Path.GetFileNameWithoutExtension(incomingReference);
						Log.Debug("Project {projectName} was referenced by the following project {reference}", projectName, incomingReferenceName);
					}
				}


				foreach (KeyValuePair<string, Holder> count in Assemblies.OrderByDescending(x => x.Value.IncomingCount))
				{
					string assemblyName = Path.GetFileNameWithoutExtension(count.Key);
					Log.Information("Assembly {assembly} was referenced {times} times", assemblyName, count.Value.IncomingCount);
					foreach (string incomingReference in count.Value.IncomingReferences)
					{
						string incomingReferenceName = Path.GetFileNameWithoutExtension(incomingReference);
						Log.Debug("Assembly {assemblyName} was referenced by the following project {reference}", assemblyName, incomingReferenceName);
					}
				}
			}
		}

		private class Holder
		{
			public string Name { get; }
			public SolutionProjectType Type { get; }
			
			public int OutgoingCount => _outgoingReferences.Count;
			private HashSet<string> _outgoingReferences = new HashSet<string>();
			public IEnumerable<string> OutgoingReferences => _outgoingReferences;
			
			public int IncomingCount => _IncomingReferences.Count;
			private HashSet<string> _IncomingReferences = new HashSet<string>();
			public IEnumerable<string> IncomingReferences => _IncomingReferences;
			
			public int AssemblyCount => _AssemblyReferences.Count;
			private HashSet<string> _AssemblyReferences = new HashSet<string>();
			public IEnumerable<string> AssemblyReferences => _AssemblyReferences;

			public Holder(string name)
			{
				Name = name;
			}

			public Holder(string name, SolutionProjectType type)
			: this(name)
			{
				Type = type;
			}

            public bool AddOutgoing(string reference)
            {
	            return _outgoingReferences.Add(reference);
            }

            public bool AddIncoming(string reference)
            {
	            return _IncomingReferences.Add(reference);
            }

            public bool AddAssembly(string reference)
            {
	            return _AssemblyReferences.Add(reference);
            }
		}
	}
}
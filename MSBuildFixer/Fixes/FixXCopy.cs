using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using MSBuildFixer.FeatureToggles;

namespace MSBuildFixer.Fixes
{
	public class FixXCopy
	{
		private string[] _excludeReferencesFor;
		private readonly Dictionary<ProjectRootElement, HashSet<string>> _references = new Dictionary<ProjectRootElement, HashSet<string>>();
		private readonly Dictionary<ProjectRootElement, HashSet<string>> _outputPaths = new Dictionary<ProjectRootElement, HashSet<string>>();
		private readonly Dictionary<ProjectRootElement, HashSet<string>> _xcopies = new Dictionary<ProjectRootElement, HashSet<string>>();
		private readonly Dictionary<ProjectRootElement, string> _assemblyNames = new Dictionary<ProjectRootElement, string>();
		private string _solutionFilePath;
		private string _fileName;

		public FixXCopy(string fileName = null, string excludeReferencesFor = null)
		{
			_fileName = fileName;
			if (!string.IsNullOrEmpty(excludeReferencesFor))
			{
				_excludeReferencesFor = excludeReferencesFor.Split(';');
			}
		}
		/// <summary>
		/// This event will handle parsing the pre/post build event. This will strip and remove
		/// all the xcopy commands, and collect their destinations.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void OnVisitProperty(object sender, EventArgs e)
		{
			var property = sender as ProjectPropertyElement;
			if (property == null) return;
			if (property.Name.Equals("AssemblyName"))
			{
				_assemblyNames[property.ContainingProject] = property.Value;
			}

			if (property.Name.Equals("PostBuildEvent"))
			{
				ProcessBuildEvent(property);
			}

			if (property.Name.Equals("OutputPath"))
			{
				var paths = GetHashSet(property.ContainingProject, _outputPaths);
				paths.Add(property.Value);
			}
		}

		/// <summary>
		/// The event will collect all the references for given project. The idea is that any reference
		/// with a HintPath is something that is not installed to the GAC, and therefor needs to be
		/// copied to all the destinations this assembly is being sent to.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void OnVisitMetadata(object sender, EventArgs e)
		{
			var metadata = sender as ProjectMetadataElement;
			if (metadata == null) return;
			if (!metadata.Name.Equals("HintPath")) return;

			var references = GetHashSet(metadata.ContainingProject, _references);
			references.Add(metadata.Value);
		}

		/// <summary>
		/// This event should be called after the walker has traversed the entire
		/// solution file. This one will generate and write out the xcopy script.
		/// This is the point when the scripts will be generated and written out.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void OnAfterVisitSolution(object sender, EventArgs e)
		{
			if (SummarizeXCopyToggle.Enabled)
			{
				if (!string.IsNullOrEmpty(_solutionFilePath))
				{
					File.WriteAllText(Path.Combine(Path.GetDirectoryName(_solutionFilePath), _fileName), CollateAllXCopies());
				}
			}
		}

		/// <summary>
		/// Need to capture the solution file path
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void OnOpenSolution(object sender, EventArgs e)
		{
			_solutionFilePath = sender as string;
		}

		public string CollateAllXCopies()
		{
			var stringBuilder = new StringBuilder();
			foreach (var xcopy in _xcopies)
			{
				var assemblyName = _assemblyNames[xcopy.Key];
				stringBuilder.AppendLine($":: {assemblyName}");
				foreach (var copy in xcopy.Value)
				{
					stringBuilder.AppendLine(copy);
				}
			}
			return stringBuilder.ToString();
		}

		public void ProcessBuildEvent(ProjectPropertyElement property)
		{
			var lines = property.Value.Split(new[] {Environment.NewLine, "\n"}, StringSplitOptions.None);
			var xcopies = lines.Where(x => x.Contains("xcopy"));
			lines = lines.Except(xcopies).ToArray();
			property.Value = string.Join(Environment.NewLine, lines);
			var hashSet = GetHashSet(property.ContainingProject, _xcopies);
			foreach (var xcopy in xcopies)
			{
				hashSet.Add(xcopy);
			}
		}

		public static string GetDestination(string xcopy)
		{
			var copySegments = xcopy.Split(new[] { " " }, StringSplitOptions.None);
			var segments = copySegments.Where(x => !(x.StartsWith("/") || x.Contains("xcopy")));
			var firstOrDefault = segments.Skip(1).FirstOrDefault();
			return firstOrDefault;
		}

		public IEnumerable<string> GetReferences(ProjectRootElement rootElement)
		{
			return GetHashSet(rootElement, _references);
		}

		public IEnumerable<string> GetXCopies(ProjectRootElement rootElement)
		{
			return GetHashSet(rootElement, _xcopies);
		}

		public string GetAssembly(ProjectRootElement rootElement)
		{
			string assemblyName;
			_assemblyNames.TryGetValue(rootElement, out assemblyName);
			return assemblyName;
		}

		public IEnumerable<string> GetOutputPaths(ProjectRootElement rootElement)
		{
			return GetHashSet(rootElement, _outputPaths);
		}

		private HashSet<string> GetHashSet(ProjectRootElement projectRootElement, IDictionary<ProjectRootElement, HashSet<string>> dictionary)
		{
			HashSet<string> hashSet;
			if (dictionary.TryGetValue(projectRootElement, out hashSet)) return hashSet;
			hashSet = new HashSet<string>();
			dictionary[projectRootElement] = hashSet;
			return hashSet;
		}
	}
}

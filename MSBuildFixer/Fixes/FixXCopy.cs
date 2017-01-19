using Microsoft.Build.Construction;
using MSBuildFixer.FeatureToggles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static System.Configuration.ConfigurationManager;

namespace MSBuildFixer.Fixes
{
	public class FixXCopy : IFix
	{
		private readonly Dictionary<ProjectRootElement, HashSet<string>> _xcopies = new Dictionary<ProjectRootElement, HashSet<string>>();
		private readonly Dictionary<ProjectRootElement, string> _assemblyNames = new Dictionary<ProjectRootElement, string>();
		public string SolutionFilePath { get; private set; }
		private readonly string _fileName;

		public FixXCopy()
		{
			_fileName = AppSettings["SummarizeXCopyToggle_FileName"];
		}

		/// <summary>
		/// This event will handle parsing the pre/post build event. This will strip and remove
		/// all the xcopy commands, and collect their destinations.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void OnVisitProperty(ProjectPropertyElement property)
		{
			if (property.Name.Equals("AssemblyName"))
			{
				_assemblyNames[property.ContainingProject] = property.Value;
			}

			if (property.Name.Equals("PostBuildEvent"))
			{
				ProcessBuildEvent(property);
			}
		}

		/// <summary>
		/// This event should be called after the walker has traversed the entire
		/// solution file. This one will generate and write out the xcopy script.
		/// This is the point when the scripts will be generated and written out.
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void OnAfterVisitSolution(SolutionFile solutionFile)
		{
			if (FixXCopyToggle.Enabled)
			{
				if (!string.IsNullOrEmpty(SolutionFilePath))
				{
					File.WriteAllText(Path.Combine(Path.GetDirectoryName(SolutionFilePath), _fileName), CollateAllXCopies());
				}
			}
		}

		/// <summary>
		/// Need to capture the solution file path
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		public void OnOpenSolution(string solutionPath)
		{
			SolutionFilePath = solutionPath;
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
			property.Value = String.Join(Environment.NewLine, lines);
			var hashSet = GetHashSet(property.ContainingProject, _xcopies);
			foreach (var xcopy in xcopies)
			{
				hashSet.Add(xcopy);
			}
		}

		public HashSet<string> GetXCopies(ProjectRootElement projectRootElement)
		{
			return GetHashSet(projectRootElement, _xcopies);
		}

		public string GetAssembly(ProjectRootElement projectRootElement)
		{
			string result;
			_assemblyNames.TryGetValue(projectRootElement, out result);
			return result;
		}
		
		private HashSet<string> GetHashSet(ProjectRootElement projectRootElement, IDictionary<ProjectRootElement, HashSet<string>> dictionary)
		{
			HashSet<string> hashSet;
			if (dictionary.TryGetValue(projectRootElement, out hashSet)) return hashSet;
			hashSet = new HashSet<string>();
			dictionary[projectRootElement] = hashSet;
			return hashSet;
		}

		public void AttachTo(SolutionWalker walker)
		{
			walker.OnVisitProperty += OnVisitProperty;
			walker.OnOpenSolution += OnOpenSolution;
			walker.OnAfterVisitSolution += OnAfterVisitSolution;
		}
	}
}

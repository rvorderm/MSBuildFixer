using Microsoft.Build.Construction;
using MSBuildFixer.SampleFeatureToggles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static System.Configuration.ConfigurationManager;

namespace MSBuildFixer.Fixes
{
	public class FixReferenceVersion : IFix
	{
		public void AttachTo(SolutionWalker walker)
		{
			walker.OnOpenSolution += Walker_OnOpenSolution;
			walker.OnVisitProjectItem += OnVisitProjectItem;
		}

		private void Walker_OnOpenSolution(string solutionPath)
		{
			_solutionDirectory = Path.GetDirectoryName(solutionPath);
		}

		private string _solutionDirectory;

		public void OnVisitProjectItem(ProjectItemElement projectItemElement)
		{
			if (!projectItemElement.ItemType.Equals("Reference")) return;
			ICollection<ProjectMetadataElement> metadataCollection = projectItemElement.Metadata;
			ProjectMetadataElement hintPath = metadataCollection.FirstOrDefault(x=>x.Name.Equals("HintPath"));
			if (hintPath == null) return;

			string fileName = GetFileName(hintPath);
			if (!File.Exists(fileName)) return;
			FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(fileName);
			string referenceVersion = GetVersion(projectItemElement.Include);
			if (string.IsNullOrEmpty(referenceVersion)) return;
			if ( referenceVersion.Equals(versionInfo.FileVersion)) return;
			projectItemElement.Include = projectItemElement.Include.Replace(referenceVersion, versionInfo.FileVersion);
		}

		private string GetFileName(ProjectMetadataElement hintPath)
		{
			if (hintPath.Value.Contains("$(SolutionDir)"))
			{
				return hintPath.Value.Replace("$(SolutionDir)", _solutionDirectory);
			}
			if(Path.IsPathRooted(hintPath.Value)) return hintPath.Value;

			string directoryName = Path.GetDirectoryName(hintPath.ContainingProject.FullPath);
			return directoryName == null ? hintPath.Value : Path.Combine(directoryName, hintPath.Value);
		}

		private static string GetVersion(string include)
		{
			if (string.IsNullOrEmpty(include)) return null;
			int indexOfVersion = include.IndexOf("Version=", StringComparison.InvariantCulture);
			if (indexOfVersion < 0) return null;
			int indexOfComma = include.IndexOf(",", indexOfVersion, StringComparison.InvariantCulture);
			if (indexOfComma < 0) return null;

			string substring = include.Substring(indexOfVersion + 8, indexOfComma-indexOfVersion-8);
			return substring;
		}
	}
}

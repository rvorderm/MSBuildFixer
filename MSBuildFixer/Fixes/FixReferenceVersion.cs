using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Build.Construction;
using MSBuildFixer.SampleFeatureToggles;

namespace MSBuildFixer.Fixes
{
	public class FixReferenceVersion
	{
		private readonly string _solutionDirectory;

		public FixReferenceVersion(string solutionDirectory)
		{
			_solutionDirectory = solutionDirectory;
		}

		public void OnVisitProjectItem(object sender, EventArgs eventArgs)
		{
			var projectItemElement = sender as ProjectItemElement;
			if (projectItemElement == null) return;
			if (!projectItemElement.ItemType.Equals("Reference")) return;
			var metadataCollection = projectItemElement.Metadata;
			var hintPath = metadataCollection.FirstOrDefault(x=>x.Name.Equals("HintPath"));
			if (hintPath == null) return;
			if (!ReferenceVersionToggle.Enabled) return;

			var fileName = GetFileName(hintPath);
			if (!File.Exists(fileName)) return;
			var versionInfo = FileVersionInfo.GetVersionInfo(fileName);
			var referenceVersion = GetVersion(projectItemElement.Include);
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

			var directoryName = Path.GetDirectoryName(hintPath.ContainingProject.FullPath);
			if (directoryName == null) return hintPath.Value;
			return Path.Combine(directoryName, hintPath.Value);
		}

		private string GetVersion(string include)
		{
			if (string.IsNullOrEmpty(include)) return null;
			var indexOfVersion = include.IndexOf("Version=", StringComparison.InvariantCulture);
			if (indexOfVersion < 0) return null;
			var indexOfComma = include.IndexOf(",", indexOfVersion, StringComparison.InvariantCulture);
			if (indexOfComma < 0) return null;

			var substring = include.Substring(indexOfVersion + 8, indexOfComma-indexOfVersion-8);
			return substring;
		}
	}
}

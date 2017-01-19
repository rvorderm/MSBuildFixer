using Microsoft.Build.Construction;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using MSBuildFixer.Helpers;

namespace MSBuildFixer.Fixes
{
	public class FixUpdateReferenceVersionToFileVersion : IFix
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

			string fileName = ProjectItemElementHelpers.GetFileName(_solutionDirectory, hintPath.ContainingProject.FullPath, hintPath.Value);
			if (!File.Exists(fileName)) return;
			FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(fileName);
			string referenceVersion = ProjectItemElementHelpers.GetVersion(projectItemElement.Include);
			if (string.IsNullOrEmpty(referenceVersion)) return;
			if ( referenceVersion.Equals(versionInfo.FileVersion)) return;
			projectItemElement.Include = projectItemElement.Include.Replace(referenceVersion, versionInfo.FileVersion);
		}
	}
}

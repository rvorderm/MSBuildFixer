using Microsoft.Build.Construction;
using MSBuildFixer.Configuration;
using MSBuildFixer.Helpers;
using MSBuildFixer.SampleFeatureToggles;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace MSBuildFixer.Fixes
{
	public class FixReferenceVersion : IFix
	{
		private string _solutionDirectory;
		private Dictionary<ProjectRootElement, PackageConfigHelper> configHelpers = new Dictionary<ProjectRootElement, PackageConfigHelper>();

		public void AttachTo(SolutionWalker walker)
		{
			walker.OnOpenSolution += Walker_OnOpenSolution;
			walker.OnVisitProjectItem_Reference += OnVisitReference;
			walker.OnVisitProjectRootItem += Walker_OnVisitProjectRootItem;
			walker.OnAfterVisitSolution += Walker_OnAfterVisitSolution;
		}

		private void Walker_OnAfterVisitSolution(SolutionFile solutionFile)
		{
			foreach (PackageConfigHelper packageConfigHelper in configHelpers.Values)
			{
				packageConfigHelper.SavePackageFile();
			}
		}

		private void Walker_OnVisitProjectRootItem(ProjectRootElement rootElement)
		{
			string packagePath = ProjectRootElementHelpers.GetNugetPackagePath(rootElement);
			configHelpers[rootElement] = new PackageConfigHelper(packagePath);
		}

		private void Walker_OnOpenSolution(string solutionPath)
		{
			_solutionDirectory = Path.GetDirectoryName(solutionPath);
		}

		public void OnVisitReference(ProjectItemElement projectItemElement)
		{
			switch (ReferenceVersionToggle.Instance.Type)
			{
				case RefereneVersionType.HintPath:
					SetVersionToFileVersion(projectItemElement);
					break;
				case RefereneVersionType.Config:
					UpgradePackageAndInclude(projectItemElement);
					break;
			}
		}

		private void UpgradePackageAndInclude(ProjectItemElement projectItemElement)
		{
			string assemblyName = ProjectItemElementHelpers.GetAssemblyName(projectItemElement.Include);
			Reference reference = ReferencesConfiguration.Instance.TryGetReference(assemblyName);
			if (reference == null) return;
			string version = ProjectItemElementHelpers.GetIncludeVersion(projectItemElement.Include);
			if (string.IsNullOrEmpty(version)) return;
			projectItemElement.Include = projectItemElement.Include.Replace(version, reference.IncludeVersion);
			
			ProjectMetadataElement hintPath = ProjectItemElementHelpers.GetHintPath(projectItemElement);
			string hintPathVersion = ProjectItemElementHelpers.GetHintPathVersion(projectItemElement);
			hintPath.Value = hintPath.Value.Replace(hintPathVersion, reference.HintPathVersion);

//			Package package = PackagesConfiguration.Instance.TryGetPackage(assemblyName);
//			if (package == null) return;
//			configHelpers[projectItemElement.ContainingProject].Update(package.PackageName, package.Version);
		}

		private void SetVersionToFileVersion(ProjectItemElement projectItemElement)
		{
			ICollection<ProjectMetadataElement> metadataCollection = projectItemElement.Metadata;
			ProjectMetadataElement hintPath = metadataCollection.FirstOrDefault(x => x.Name.Equals("HintPath"));
			if (hintPath == null) return;

			string fileName = ProjectItemElementHelpers.GetFileName(_solutionDirectory, hintPath.ContainingProject.FullPath,
				hintPath.Value);
			if (!File.Exists(fileName)) return;
			FileVersionInfo versionInfo = FileVersionInfo.GetVersionInfo(fileName);
			string referenceVersion = ProjectItemElementHelpers.GetIncludeVersion(projectItemElement.Include);
			if (string.IsNullOrEmpty(referenceVersion)) return;
			if (referenceVersion.Equals(versionInfo.FileVersion)) return;
			projectItemElement.Include = projectItemElement.Include.Replace(referenceVersion, versionInfo.FileVersion);
		}
	}
}

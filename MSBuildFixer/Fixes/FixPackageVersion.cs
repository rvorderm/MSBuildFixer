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
	public class FixPackageVersion : IFix
	{
		public void AttachTo(SolutionWalker walker)
		{
		walker.OnVisitProjectRootItem += Walker_OnVisitProjectRootItem; ;
		}

		private void Walker_OnVisitProjectRootItem(ProjectRootElement rootElement)
		{
			string directoryName = Path.GetDirectoryName(rootElement.FullPath);
			string packagePath = Path.Combine(directoryName, "packages.config");

			var packageConfigHelper = new PackageConfigHelper(packagePath);
			foreach (Package package in PackagesConfiguration.Instance.Packages)
			{
				packageConfigHelper.Update(package.PackageName, package.Version);
			}
			packageConfigHelper.SavePackageFile();
		}

	}
}

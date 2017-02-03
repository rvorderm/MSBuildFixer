using System.Collections.Generic;
using Microsoft.Build.Construction;
using MSBuildFixer.Configuration;
using MSBuildFixer.Helpers;

namespace MSBuildFixer.Fixes
{
	public class FixPackages : IFix
	{
		public List<IPackageConfigHelper> PackageBuilders = new List<IPackageConfigHelper>();
		public void AttachTo(SolutionWalker walker)
		{
			walker.OnVisitProjectRootItem += Walker_OnVisitProjectRootItem;
			walker.OnSave += Walker_OnSave;
		}

		public void Walker_OnSave()
		{
			foreach (IPackageConfigHelper packageConfigHelper in PackageBuilders)
			{
				packageConfigHelper.SavePackageFile(packageConfigHelper.GetPackageDocument());
			}
		}

		private void Walker_OnVisitProjectRootItem(ProjectRootElement rootElement)
		{
			string packagePath = ProjectRootElementHelpers.GetNugetPackagePath(rootElement);
			var packageConfigHelper = new PackageConfigHelper(packagePath);
			foreach (Package package in PackagesConfiguration.Instance.Packages)
			{
				packageConfigHelper.Update(package.PackageName, package.Version, PackagesConfiguration.Instance.TargetFramework);
			}
			PackageBuilders.Add(packageConfigHelper);
		}
	}
}

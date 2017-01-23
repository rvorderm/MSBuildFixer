using Microsoft.Build.Construction;
using MSBuildFixer.Configuration;
using MSBuildFixer.Helpers;

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
			string packagePath = ProjectRootElementHelpers.GetNugetPackagePath(rootElement);
			var packageConfigHelper = new PackageConfigHelper(packagePath);
			foreach (Package package in PackagesConfiguration.Instance.Packages)
			{
				packageConfigHelper.Update(package.PackageName, package.Version);
			}
			packageConfigHelper.SavePackageFile();
		}

	}
}

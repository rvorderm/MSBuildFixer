using System;
using System.Xml;
using FakeItEasy;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSBuildFixer.Fixes;
using MSBuildFixer.Helpers;

namespace MSBuildFixerTests.Fixes
{
	[TestClass]
	public class FixPackagesTests
	{
		[TestMethod]
		public void SaveCallsPackageBuilderSave()
		{
			var fixPackages = new FixPackages();
			var packageConfigHelper = A.Fake<IPackageConfigHelper>();
			fixPackages.PackageBuilders.Add(packageConfigHelper);
			A.CallTo(() => packageConfigHelper.GetPackageDocument()).Returns(null);
			fixPackages.Walker_OnSave();
			A.CallTo(() => packageConfigHelper.SavePackageFile(null)).MustHaveHappened();
		}
	}
}

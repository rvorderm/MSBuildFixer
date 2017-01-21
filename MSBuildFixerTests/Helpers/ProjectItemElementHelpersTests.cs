using Microsoft.Build.Construction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSBuildFixer.Helpers;
using MSBuildFixerTests.Fixes;
using System.Linq;

namespace MSBuildFixerTests.Helpers
{
	[TestClass]
	public class ProjectItemElementHelpersTests
	{
		[TestMethod]
		public void GetAssemblyFromWeakNameString()
		{
			string assemblyName = ProjectItemElementHelpers.GetAssemblyName("FakeItEasy");
			Assert.AreEqual("FakeItEasy", assemblyName);
		}

		[TestMethod]
		public void GetAssemblyFromStrongNameString()
		{
			string assemblyName = ProjectItemElementHelpers.GetAssemblyName("FakeItEasy, Version=2.3.0.0, Culture=neutral, PublicKeyToken=eff28e2146d5fd2c, processorArchitecture=MSIL");
			Assert.AreEqual("FakeItEasy", assemblyName);
		}

		[TestMethod]
		public void GetIncludeVersionFromWeakNameString()
		{
			string assemblyName = ProjectItemElementHelpers.GetIncludeVersion("FakeItEasy");
			Assert.AreEqual(null, assemblyName);
		}

		[TestMethod]
		public void GetIncludeVersionFromStrongNameString()
		{
			string assemblyName = ProjectItemElementHelpers.GetIncludeVersion("FakeItEasy, Version=2.3.0.0, Culture=neutral, PublicKeyToken=eff28e2146d5fd2c, processorArchitecture=MSIL");
			Assert.AreEqual("2.3.0.0", assemblyName);
		}

		[TestMethod]
		public void GetHintPathVersion()
		{
			ProjectRootElement projectRootElement = TestSetup.GetTestProject();
			ProjectItemElement projectItemElement = projectRootElement.Items.FirstOrDefault();
			string version = ProjectItemElementHelpers.GetHintPathVersion(projectItemElement);
			Assert.AreEqual("2.3.3", version);
		}

		[TestMethod]
		public void GetHintPathVersionNotNuget()
		{
			ProjectRootElement projectRootElement = TestSetup.GetTestProject();
			ProjectItemElement projectItemElement = projectRootElement.Items.Skip(3).FirstOrDefault();
			string version = ProjectItemElementHelpers.GetHintPathVersion(projectItemElement);
			Assert.IsNull(version);
		}

		[TestMethod]
		public void GetHintPathVersionDotInName()
		{
			ProjectRootElement projectRootElement = TestSetup.GetTestProject();
			ProjectItemElement projectItemElement = projectRootElement.Items.Skip(2).FirstOrDefault();
			string version = ProjectItemElementHelpers.GetHintPathVersion(projectItemElement);
			Assert.AreEqual("3.5.1", version);
		}
	}
}

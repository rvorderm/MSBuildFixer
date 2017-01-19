using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSBuildFixer.Helpers;

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
		public void GetVersionFromWeakNameString()
		{
			string assemblyName = ProjectItemElementHelpers.GetVersion("FakeItEasy");
			Assert.AreEqual(null, assemblyName);
		}

		[TestMethod]
		public void GetVersionFromStrongNameString()
		{
			string assemblyName = ProjectItemElementHelpers.GetVersion("FakeItEasy, Version=2.3.0.0, Culture=neutral, PublicKeyToken=eff28e2146d5fd2c, processorArchitecture=MSIL");
			Assert.AreEqual("2.3.0.0", assemblyName);
		}
	}
}

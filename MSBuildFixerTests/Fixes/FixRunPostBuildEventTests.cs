using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSBuildFixer.Fixes;

namespace MSBuildFixerTests.Fixes
{
	[TestClass]
	public class FixRunPostBuildEventTests
	{
		[TestClass]
		public class OnVisitPropertyTests
		{
			[TestMethod]
			public void SetsValue()
			{
				TestSetup.SetOutputPathToggleTo(true);

				var projectRootElement = TestSetup.GetTestProject();

				Assert.Fail();
			}

			[TestMethod]
			public void ToggleBlocks()
			{
				TestSetup.SetOutputPathToggleTo(false);

				var projectRootElement = TestSetup.GetTestProject();

				Assert.Fail();
			}

			[TestMethod]
			public void NullInput()
			{
				FixRunPostBuildEvent.OnVisitProperty(null, new EventArgs());
			}

			[TestMethod]
			public void NotOutputPath()
			{
				TestSetup.SetOutputPathToggleTo(true);

				var projectRootElement = TestSetup.GetTestProject();

				Assert.Fail();
			}
		}
	}
}

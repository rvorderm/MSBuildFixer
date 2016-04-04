using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSBuildFixer.Fixes;
using MSBuildFixer.SampleFeatureToggles;

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
				TestSetup.SetToggleTo(RunPostBuildEventToggle.Instance, true);

				var projectRootElement = TestSetup.GetTestProject();

				var elements = projectRootElement.Properties.Where(x => x.Name.Equals("RunPostBuildEvent")).ToList();
				Assert.IsTrue(elements.Any());

				var element = elements[0];
				var fixRunPostBuildEvent = new FixRunPostBuildEvent();
				fixRunPostBuildEvent.OnVisitProperty(element, new EventArgs());
				Assert.AreEqual(@"OnOutputUpdated", element.Value);
			}

			[TestMethod]
			public void ToggleBlocks()
			{
				TestSetup.SetToggleTo(RunPostBuildEventToggle.Instance, false);

				var projectRootElement = TestSetup.GetTestProject();

				var elements = projectRootElement.Properties.Where(x => x.Name.Equals("RunPostBuildEvent")).ToList();
				Assert.IsTrue(elements.Any());

				var element = elements[0];
				var fixRunPostBuildEvent = new FixRunPostBuildEvent();
				fixRunPostBuildEvent.OnVisitProperty(element, new EventArgs());
				Assert.AreEqual(@"Always", element.Value);
			}

			[TestMethod]
			public void NullInput()
			{
				var fixRunPostBuildEvent = new FixRunPostBuildEvent();
				fixRunPostBuildEvent.OnVisitProperty(null, new EventArgs());
			}

			[TestMethod]
			public void WrongName()
			{
				TestSetup.SetToggleTo(RunPostBuildEventToggle.Instance, true);

				var projectRootElement = TestSetup.GetTestProject();

				var elements = projectRootElement.Properties.Where(x => x.Name.Equals("RunPostBuildEvent")).ToList();
				Assert.IsTrue(elements.Any());

				var element = elements[0];
				element.Name = "anythingElse";
				var fixRunPostBuildEvent = new FixRunPostBuildEvent();
				fixRunPostBuildEvent.OnVisitProperty(element, new EventArgs());
				Assert.AreEqual(@"Always", element.Value);
			}
		}
	}
}

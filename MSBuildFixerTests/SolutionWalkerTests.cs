using System.Linq;
using FakeItEasy;
using Microsoft.Build.Construction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSBuildFixer;
using MSBuildFixerTests.Fixes;

namespace MSBuildFixerTests
{
	[TestClass]
	public class SolutionWalkerTests
	{
		[TestClass]
		public class VisitPropertyTests
		{
			[TestMethod]
			public void CallsEventHandler()
			{
				var eventHandlerCalled = false;
				var walker = A.Fake<SolutionWalker>();
				var projectRootElement = TestSetup.GetTestProject();
				var projectPropertyElement = projectRootElement.Properties.First();
				walker.OnVisitProperty += (sender, args) => { eventHandlerCalled = true; };
				walker.VisitProperty(projectPropertyElement);
				Assert.IsTrue(eventHandlerCalled);
			}
		}
	}
}
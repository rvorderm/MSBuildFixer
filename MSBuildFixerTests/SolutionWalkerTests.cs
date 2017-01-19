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
				var element = projectRootElement.Properties.First();
				walker.OnVisitProperty += (sender) => { eventHandlerCalled = true; };
				walker.VisitProperty(element);
				Assert.IsTrue(eventHandlerCalled);
			}
		}

		[TestClass]
		public class VisitMetadataTests
		{
			[TestMethod]
			public void CallsEventHandler()
			{
				var eventHandlerCalled = false;
				var walker = A.Fake<SolutionWalker>();
				var projectRootElement = TestSetup.GetTestProject();
				var element = projectRootElement.AllChildren.OfType<ProjectMetadataElement>().First();
				walker.OnVisitMetadata += sender => { eventHandlerCalled = true; };
				walker.VisitMetadata(element);
				Assert.IsTrue(eventHandlerCalled);
			}
		}

		[TestClass]
		public class VisitProjectItemTests
		{
			[TestMethod]
			public void CallsEventHandler()
			{
				var eventHandlerCalled = false;
				var walker = A.Fake<SolutionWalker>();
				var projectRootElement = TestSetup.GetTestProject();
				var element = projectRootElement.Items.First();
				walker.OnVisitMetadata += sender => { eventHandlerCalled = true; };
				walker.VisitProjectItem(element);
				Assert.IsTrue(eventHandlerCalled);
			}
		}

		[TestClass]
		public class VisitMetadataCollectionTests
		{
			[TestMethod]
			public void CallsEventHandler()
			{
				var eventHandlerCalled = false;
				var walker = A.Fake<SolutionWalker>();
				var projectRootElement = TestSetup.GetTestProject();
				var elements = projectRootElement.AllChildren.OfType<ProjectMetadataElement>().ToList();
				walker.OnVisitMetadataCollection += sender => { eventHandlerCalled = true; };
				walker.VisitMetadataCollection(elements);
				Assert.IsTrue(eventHandlerCalled);
			}
		}
	}
}
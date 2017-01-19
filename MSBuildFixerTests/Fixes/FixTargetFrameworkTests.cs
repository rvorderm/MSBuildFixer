using FakeItEasy;
using Microsoft.Build.Construction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSBuildFixer.Fixes;
using MSBuildFixer.SampleFeatureToggles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MSBuildFixerTests.Fixes
{
	[TestClass]
	public class FixTargetFrameworkTests
	{
		[TestClass]
		public class OnVisitMetadataTests
		{
			[TestMethod]
			public void SetsValue()
			{
				TestSetup.SetToggleTo(FixTargetFrameworkToggle.Instance, true);

				ProjectRootElement projectRootElement = TestSetup.GetTestProject();

				List<ProjectPropertyElement> elements = projectRootElement.Properties.Where(x=>x.Name.Equals("TargetFrameworkVersion")).ToList();
				Assert.IsTrue(elements.Any());

				var fixTargetFramework = new FixTargetFramework("v4.6.2");
				ProjectPropertyElement element = elements.First();
				Assert.AreNotEqual("v4.6.2", element.Value);
				fixTargetFramework.OnVisitProperty(element);
				Assert.AreEqual("v4.6.2", element.Value);
			}

			[TestMethod]
			public void ToggleBlocks()
			{
				TestSetup.SetToggleTo(FixTargetFrameworkToggle.Instance, false);

				ProjectRootElement projectRootElement = TestSetup.GetTestProject();

				List<ProjectPropertyElement> elements = projectRootElement.Properties.Where(x => x.Name.Equals("TargetFrameworkVersion")).ToList();
				Assert.IsTrue(elements.Any());

				var fixTargetFramework = new FixTargetFramework("v4.6.2");
				ProjectPropertyElement element = elements.First();
				Assert.AreNotEqual("v4.6.2", element.Value);
				fixTargetFramework.OnVisitProperty(element);
				Assert.AreNotEqual("v4.6.2", element.Value);
			}

			[TestMethod]
			public void NullInput()
			{
				var fixTargetFramework = new FixTargetFramework(string.Empty);
				fixTargetFramework.OnVisitProperty(null);
			}

			[TestMethod]
			public void WrongName()
			{
				TestSetup.SetToggleTo(FixTargetFrameworkToggle.Instance, false);

				ProjectRootElement projectRootElement = TestSetup.GetTestProject();

				List<ProjectPropertyElement> elements = projectRootElement.Properties.Where(x => x.Name.Equals("TargetFrameworkVersion")).ToList();
				Assert.IsTrue(elements.Any());

				var fixTargetFramework = new FixTargetFramework("v4.6.2");
				ProjectPropertyElement element = elements.First();
				Assert.AreNotEqual("v4.6.2", element.Value);
				fixTargetFramework.OnVisitProperty(element);
				Assert.AreNotEqual("v4.6.2", element.Value);
			}
		}
	}
}

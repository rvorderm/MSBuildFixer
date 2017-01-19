using System;
using System.IO;
using System.Linq;
using System.Xml;
using FakeItEasy;
using FeatureToggle.Core;
using Microsoft.Build.Construction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSBuildFixer;
using MSBuildFixer.Fixes;
using MSBuildFixer.SampleFeatureToggles;
using MSBuildFixerTests.Properties;

namespace MSBuildFixerTests.Fixes
{
	[TestClass]
	public class FixOutputPathTests
	{
		[TestClass]
		public class OnVisitPropertyTests
		{
			[TestMethod]
			public void SetsValue()
			{
				TestSetup.SetToggleTo(OutputPathToggle.Instance, true);

				var projectRootElement = TestSetup.GetTestProject();

				var elements = projectRootElement.Properties.Where(x=>x.Name.Equals("OutputPath")).ToList();
				Assert.IsTrue(elements.Any());

				var element = elements[0];
				var fixOutputPath = new FixOutputPath();
				fixOutputPath.OnVisitProperty(element);
				Assert.AreEqual(Path.Combine("$(SolutionDir)", "bin", "$(Configuration)"), element.Value);
			}

			[TestMethod]
			public void ToggleBlocks()
			{
				TestSetup.SetToggleTo(OutputPathToggle.Instance, false);

				var projectRootElement = TestSetup.GetTestProject();

				var elements = projectRootElement.Properties.Where(x=>x.Name.Equals("OutputPath")).ToList();

				Assert.IsTrue(elements.Any());

				var element = elements[0];
				var fixOutputPath = new FixOutputPath();
				fixOutputPath.OnVisitProperty(element);
				Assert.AreEqual(@"bin\Debug", element.Value);
			}

			[TestMethod]
			public void NullInput()
			{
				var fixOutputPath = new FixOutputPath();
				fixOutputPath.OnVisitProperty(null);
			}

			[TestMethod]
			public void NotOutputPath()
			{
				TestSetup.SetToggleTo(OutputPathToggle.Instance, true);

				var projectRootElement = TestSetup.GetTestProject();

				var elements = projectRootElement.Properties.Where(x => x.Name.Equals("OutputPath")).ToList();
				Assert.IsTrue(elements.Any());

				var element = elements[0];
				element.Name = "meow";
				var fixOutputPath = new FixOutputPath();
				fixOutputPath.OnVisitProperty(element);
				Assert.AreEqual(@"bin\Debug", element.Value);
			}
		}
	}
}

using System;
using System.IO;
using System.Linq;
using System.Xml;
using FakeItEasy;
using FakeItEasy.ExtensionSyntax.Full;
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

				var outputPaths = projectRootElement.Properties.Where(x=>x.Name.Equals("OutputPath")).ToList();
				Assert.IsTrue(outputPaths.Any());

				var outputPath = outputPaths[0];
				FixOutputPath.OnVisitProperty(outputPath, new EventArgs());
				Assert.AreEqual(Path.Combine("$(SolutionDir)", "bin", "$(Configuration)"), outputPath.Value);
			}

			[TestMethod]
			public void ToggleBlocks()
			{
				TestSetup.SetToggleTo(OutputPathToggle.Instance, false);

				var projectRootElement = TestSetup.GetTestProject();

				var outputPaths = projectRootElement.Properties.Where(x=>x.Name.Equals("OutputPath")).ToList();
				Assert.IsTrue(outputPaths.Any());

				var outputPath = outputPaths[0];
				FixOutputPath.OnVisitProperty(outputPath, new EventArgs());
				Assert.AreEqual(@"bin\Debug", outputPath.Value);
			}

			[TestMethod]
			public void NullInput()
			{
				FixOutputPath.OnVisitProperty(null, new EventArgs());
			}

			[TestMethod]
			public void NotOutputPath()
			{
				TestSetup.SetToggleTo(OutputPathToggle.Instance, true);

				var projectRootElement = TestSetup.GetTestProject();

				var outputPaths = projectRootElement.Properties.Where(x => x.Name.Equals("OutputPath")).ToList();
				Assert.IsTrue(outputPaths.Any());

				var outputPath = outputPaths[0];
				outputPath.Name = "meow";
				FixOutputPath.OnVisitProperty(outputPath, new EventArgs());
				Assert.AreEqual(@"bin\Debug", outputPath.Value);
			}
		}
	}
}

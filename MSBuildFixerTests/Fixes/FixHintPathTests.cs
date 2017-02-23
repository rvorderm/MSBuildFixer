using FakeItEasy;
using Microsoft.Build.Construction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSBuildFixer;
using MSBuildFixer.Fixes;
using MSBuildFixer.Helpers;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace MSBuildFixerTests.Fixes
{
	[TestClass]
	public class FixHintPathTests
	{
		private static ILookup<string, string> CreateBasicLookup(string name, string value)
		{
			var lookup = A.Fake<ILookup<string, string>>();
			AddToLookup(lookup, name, value);
			return lookup;
		}

		private static void AddToLookup(ILookup<string, string> libLookup, string name, string value)
		{
			A.CallTo(() => libLookup.Contains(name)).Returns(true);
			A.CallTo(() => libLookup[name]).Returns(new List<string>() { value });
		}

		[TestMethod]
		public void LibraryPathBecomesLookup()
		{
			var fix = new FixHintPath();
			fix.LibraryPath = Path.Combine(Path.GetDirectoryName(TestSetup.SolutionPath), "packages");
			Assert.IsNotNull(fix.Library);
			Assert.IsTrue(fix.Library.Any());
		}

		[TestMethod]
		public void SolutionPathBecomesLookup()
		{
			var fix = new FixHintPath();
			fix.SolutionPath = Path.Combine(Path.GetDirectoryName(TestSetup.SolutionPath), "packages");
			Assert.IsNotNull(fix.SolutionLookup);
			Assert.IsTrue(fix.SolutionLookup.Any());
		}

		[TestMethod]
		public void FixBrokenPath()
		{
			SolutionWalker solutionWalker = TestSetup.BuildWalker<FixHintPath>(x =>
			{
				x.Library = CreateBasicLookup("FeatureToggle.Core.dll", "hintPath");
				x.SolutionLookup = CreateBasicLookup("", "");
			});
			IEnumerable<ProjectRootElement> projectRootElements = solutionWalker.VisitSolution(false);
			ProjectRootElement rootElement = projectRootElements.First();
			ProjectItemElement item = rootElement.Items.First(x=>x.Include.StartsWith("FeatureToggle.Core, Version=3.3.0.0"));
			ProjectMetadataElement hintPath = ProjectItemElementHelpers.GetHintPath(item);
			Assert.AreEqual("hintPath", hintPath.Value);
		}

		[TestMethod]
		public void AddMissingPath()
		{
			var walker = new SolutionWalker(TestSetup.SolutionPath);
			ProjectRootElement badProject = walker.VisitSolution(false).First();
			ProjectItemElement projectItemElement = badProject.Items.First(x=>x.Include.StartsWith("FeatureToggle,"));
			ProjectMetadataElement missingHintPath = ProjectItemElementHelpers.GetHintPath(projectItemElement);
			Assert.IsNull(missingHintPath);

			SolutionWalker solutionWalker = TestSetup.BuildWalker<FixHintPath>(x =>
			{
				x.Library = CreateBasicLookup("FeatureToggle.dll", "hintPath");
				x.SolutionLookup = CreateBasicLookup("", "");
			});
			IEnumerable<ProjectRootElement> projectRootElements = solutionWalker.VisitSolution(false);
			ProjectRootElement rootElement = projectRootElements.First();
			ProjectItemElement item = rootElement.Items.First(x => x.Include.StartsWith("FeatureToggle, Version=3.3.0.0"));
			ProjectMetadataElement hintPath = ProjectItemElementHelpers.GetHintPath(item);
			Assert.AreEqual("hintPath", hintPath.Value);
		}

		[TestMethod]
		public void FindInSolutionFolder()
		{
			SolutionWalker solutionWalker = TestSetup.BuildWalker<FixHintPath>(x =>
			{
				x.Library = CreateBasicLookup(string.Empty, string.Empty);
				x.SolutionLookup = CreateBasicLookup("FeatureToggle.Core.dll", "hintPath");
			});
			IEnumerable<ProjectRootElement> projectRootElements = solutionWalker.VisitSolution(false);
			ProjectRootElement rootElement = projectRootElements.First();
			ProjectItemElement item = rootElement.Items.First(x => x.Include.StartsWith("FeatureToggle.Core, Version=3.3.0.0"));
			ProjectMetadataElement hintPath = ProjectItemElementHelpers.GetHintPath(item);
			Assert.AreEqual("hintPath", hintPath.Value);
		}

		[TestMethod]
		public void MatchFileVersion()
		{
			Assert.Fail();
		}

		[TestMethod]
		public void UseRelative()
		{
			Assert.Fail();
		}
	}
}

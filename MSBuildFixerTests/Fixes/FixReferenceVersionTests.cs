using Microsoft.Build.Construction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSBuildFixer;
using MSBuildFixer.Configuration;
using MSBuildFixer.Fixes;
using MSBuildFixer.Helpers;
using MSBuildFixer.SampleFeatureToggles;
using System.Collections.Generic;
using System.Linq;

namespace MSBuildFixerTests.Fixes
{
    [TestClass]
	public class FixReferenceVersionTests
	{
		[TestClass]
		public class OnVisitMetadataTests
		{
			[TestMethod]
			public void ShouldUseHintPath()
			{
				FixesConfiguration.Instance.ReferenceVersionType = ReferenceVersionType.HintPath;
				var walker = new SolutionWalker(TestSetup.SolutionPath);
				ProjectRootElement badProject = walker.VisitSolution(false).First();
				ProjectItemElement badElement = badProject.Items.FirstOrDefault(x=>x.Include.StartsWith("FakeItEasy, Version=2.3.0"));
				Assert.IsNotNull(badElement);
				new FixReferenceVersion().AttachTo(walker);
				ProjectRootElement fixedProject = walker.VisitSolution(false).First();
				ProjectItemElement missingElement = fixedProject.Items.FirstOrDefault(x => x.Include.StartsWith("FakeItEasy, Version=2.3.0"));
				Assert.IsNull(missingElement);
				ProjectItemElement fixedElement = fixedProject.Items.FirstOrDefault(x => x.Include.StartsWith("FakeItEasy, Version=2.3.3"));
				Assert.IsNotNull(fixedElement);
			}

            [TestMethod]
			public void ShouldUseConfig()
			{
				FixesConfiguration.Instance.ReferenceVersionType = ReferenceVersionType.Config;
                ReferencesConfiguration.Instance = new ReferencesConfiguration()
                {
                    References = new List<Reference>()
                    {
                        new Reference()
                        {
                            AssemblyName = "FakeItEasy",
                            HintPathVersion = "3.0",
                            IncludeVersion = "3.0",
                            PackageVersion = "3.0"
                        }
                    }
                };
                var walker = new SolutionWalker(TestSetup.SolutionPath);
				ProjectRootElement badProject = walker.VisitSolution(false).First();
				ProjectItemElement badElement = badProject.Items.FirstOrDefault(x=>x.Include.StartsWith("FakeItEasy, Version=2.3.0"));
				Assert.IsNotNull(badElement);
				new FixReferenceVersion().AttachTo(walker);
				ProjectRootElement fixedProject = walker.VisitSolution(false).First();
				ProjectItemElement missingElement = fixedProject.Items.FirstOrDefault(x => x.Include.StartsWith("FakeItEasy, Version=2.3.0"));
				Assert.IsNull(missingElement);
				ProjectItemElement fixedElement = fixedProject.Items.FirstOrDefault(x => x.Include.StartsWith("FakeItEasy, Version=3.0"));
				Assert.IsNotNull(fixedElement);
                Assert.AreEqual("3.0", ProjectItemElementHelpers.GetHintPathVersion(fixedElement));
			}
		}
	}
}

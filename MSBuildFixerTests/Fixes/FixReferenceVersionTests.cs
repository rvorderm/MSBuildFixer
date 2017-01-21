using Microsoft.Build.Construction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSBuildFixer;
using MSBuildFixer.Fixes;
using System.Linq;
using MSBuildFixer.SampleFeatureToggles;

namespace MSBuildFixerTests.Fixes
{
	[TestClass]
	public class FixReferenceVersionTests
	{
		[TestClass]
		public class OnVisitMetadataTests
		{
			[TestMethod]
			public void FixesVersionNumber()
			{
				ReferenceVersionToggle.Instance.Type = RefereneVersionType.HintPath;
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
		}
	}
}

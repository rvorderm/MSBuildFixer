using Microsoft.Build.Construction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSBuildFixer.Fixes;
using MSBuildFixer.SampleFeatureToggles;
using System.IO;

namespace MSBuildFixerTests.Fixes
{
	[TestClass]
	public class FixOutputPathTests
	{
		[TestClass]
		public class OnVisitPropertyTests
		{
			[TestMethod]
			public void SetsValueNotRelative()
			{
				TestSetup.SetToggleTo(OutputPathToggle.Instance, true);
				TestSetup.SetToggleTo(UseRelativePathing.Instance, false);

				ProjectPropertyElement element = TestSetup.GetProperty("OutputPath");
				var fixOutputPath = new FixOutputPath();
				fixOutputPath.OnVisitProperty(element);
				Assert.AreEqual(Path.Combine("$(SolutionDir)", "bin", "$(Configuration)"), element.Value);
			}

			[TestMethod]
			public void SetsValueRelative()
			{
				TestSetup.SetToggleTo(OutputPathToggle.Instance, true);
				TestSetup.SetToggleTo(UseRelativePathing.Instance, true);

				ProjectPropertyElement element = TestSetup.GetProperty("OutputPath");
				element.ContainingProject.FullPath = @"C:\Repo\project\project.csproj";

				var fixOutputPath = new FixOutputPath();
				fixOutputPath.OnOpenSolution(@"C:\Repo\solution.sln");
				fixOutputPath.OnVisitProperty(element);
				Assert.AreEqual(Path.Combine("..", "bin", "$(Configuration)"), element.Value);
			}

			[TestMethod]
			public void NotOutputPath()
			{
				TestSetup.SetToggleTo(OutputPathToggle.Instance, true);

				ProjectPropertyElement element = TestSetup.GetProperty("OutputPath");

				element.Name = "meow";
				var fixOutputPath = new FixOutputPath();
				fixOutputPath.OnVisitProperty(element);
				Assert.AreEqual(@"bin\Debug", element.Value);
			}
		}
	}
}

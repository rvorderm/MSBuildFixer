using FakeItEasy;
using Microsoft.Build.Construction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSBuildFixer.Fixes;
using MSBuildFixer.SampleFeatureToggles;
using System;
using System.Collections.Generic;
using System.Linq;
using MSBuildFixer;
using MSBuildFixer.Configuration;

namespace MSBuildFixerTests.Fixes
{
	[TestClass]
	public class FixTargetFrameworkTests
	{
		[TestMethod]
		public void UpdatesTargetFramework()
		{
			FixesConfiguration.Instance = new FixesConfiguration
			{
				Properties = new List<Property>
				{
					new Property()
					{
						Name = "TargetFrameworkVersion",
						Value = "v4.6"
					}
				}
			};

			SolutionWalker solutionWalker = TestSetup.BuildWalker<FixProperties>();
			IEnumerable<ProjectRootElement> projectRootElements = solutionWalker.VisitSolution(false);
			IEnumerable<ProjectPropertyElement> badProperties = projectRootElements.SelectMany(x => x.Properties)
				.Where(x => x.Name.Equals("TargetFrameworkVersion"))
				.Where(x => !x.Value.Equals("v4.6"));
			foreach (ProjectPropertyElement property in badProperties)
			{
				Assert.Fail($"{property.Value} found in {property.ContainingProject.FullPath}");
			}
		}

		
	}
}

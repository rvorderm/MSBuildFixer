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
			AssertPropertyValues(projectRootElements, "TargetFrameworkVersion", "v4.6");
		}

		[TestMethod]
		public void UpdatesPostbuild()
		{
			FixesConfiguration.Instance = new FixesConfiguration
			{
				Properties = new List<Property>
				{
					new Property()
					{
						Name = "RunPostBuildEvent",
						Value = "OnOutputUpdated"
					}
				}
			};

			SolutionWalker solutionWalker = TestSetup.BuildWalker<FixProperties>();
			IEnumerable<ProjectRootElement> projectRootElements = solutionWalker.VisitSolution(false);
			AssertPropertyValues(projectRootElements, "TargetFrameworkVersion", "v4.6");
		}

		private static void AssertPropertyValues(IEnumerable<ProjectRootElement> projectRootElements, string propertyName, string propertyValue)
		{
			IEnumerable<ProjectPropertyElement> badProperties = projectRootElements.SelectMany(x => x.Properties)
				.Where(x => x.Name.Equals(propertyName))
				.Where(x => !x.Value.Equals(propertyValue));
			foreach (ProjectPropertyElement property in badProperties)
			{
				Assert.Fail($"{property.Value} found in {property.ContainingProject.FullPath}");
			}
		}
	}
}

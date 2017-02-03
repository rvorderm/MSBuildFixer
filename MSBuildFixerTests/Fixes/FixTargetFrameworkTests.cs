using FakeItEasy;
using Microsoft.Build.Construction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSBuildFixer.Fixes;
using MSBuildFixer.SampleFeatureToggles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using MSBuildFixer;
using MSBuildFixer.Configuration;
using MSBuildFixer.Helpers;

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
					new Property
					{
						Name = "TargetFrameworkVersion",
						Value = "v4.6"
					}
				}
			};

			SolutionWalker solutionWalker = TestSetup.BuildWalker<FixProperties>();
			IEnumerable<ProjectRootElement> projectRootElements = solutionWalker.VisitSolution(false);
			TestSetup.AssertPropertyValues(projectRootElements, "TargetFrameworkVersion", "v4.6");
		}

		[TestMethod]
		public void UpdatesPackageFiles()
		{
			PackagesConfiguration.Instance = new PackagesConfiguration()
			{
				Packages= new List<Package>
				{
					new Package
					{
						PackageName = ".*",
						Version = "v4.6"
					}
				},
				TargetFramework = "net46"
			};

			var fix = new FixPackages();
			SolutionWalker solutionWalker = TestSetup.BuildWalker(fix);
			solutionWalker.VisitSolution(false);
			foreach (PackageConfigHelper builder in fix.PackageBuilders)
			{
				XmlDocument packageDocument = builder.GetPackageDocument();
				XmlNodeList xmlNodeList = packageDocument.SelectNodes("//package[@targetFramework='net40']");
				Assert.IsTrue(xmlNodeList.Count == 0);
			}
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
			TestSetup.AssertPropertyValues(projectRootElements, "TargetFrameworkVersion", "v4.6");
		}
	}
}

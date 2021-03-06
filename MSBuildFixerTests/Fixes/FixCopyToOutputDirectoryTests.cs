﻿using System;
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
	public class FixCopyToOutputDirectoryTests
	{
		[TestClass]
		public class OnVisitMetadataTests
		{
			[TestMethod]
			public void SetsValue()
			{
				TestSetup.SetToggleTo(CopyToOutputDirectoryToggle.Instance, true);

				var projectRootElement = TestSetup.GetTestProject();

				var elements = projectRootElement.Items.Where(x=>x.ItemType.Equals("None")).ToList();
				Assert.IsTrue(elements.Any());

				var element = elements.SelectMany(x => x.Metadata).First(y => y.Name.Equals("CopyToOutputDirectory"));
				var fixCopyToOutputDirectory = new FixCopyToOutputDirectory();
				fixCopyToOutputDirectory.OnVisitMetadata(element);
				Assert.AreEqual("PreserveNewest", element.Value);
			}

			[TestMethod]
			public void ToggleBlocks()
			{
				TestSetup.SetToggleTo(CopyToOutputDirectoryToggle.Instance, false);

				var projectRootElement = TestSetup.GetTestProject();

				var elements = projectRootElement.Items.Where(x => x.ItemType.Equals("None")).ToList();
				Assert.IsTrue(elements.Any());

				var element = elements.SelectMany(x => x.Metadata).First(y => y.Name.Equals("CopyToOutputDirectory"));
				var fixCopyToOutputDirectory = new FixCopyToOutputDirectory();
				fixCopyToOutputDirectory.OnVisitMetadata(element);
				Assert.AreNotEqual("PreserveNewest", element.Value);
			}

			[TestMethod]
			public void WrongName()
			{
				TestSetup.SetToggleTo(CopyToOutputDirectoryToggle.Instance, true);

				var projectRootElement = TestSetup.GetTestProject();

				var elements = projectRootElement.Items.Where(x => x.ItemType.Equals("None")).ToList();
				Assert.IsTrue(elements.Any());

				var element = elements.SelectMany(x => x.Metadata).First(y => y.Name.Equals("CopyToOutputDirectory"));
				element.Name = "NotRight";
				var fixCopyToOutputDirectory = new FixCopyToOutputDirectory();
				fixCopyToOutputDirectory.OnVisitMetadata(element);
				Assert.AreNotEqual("PreserveNewest", element.Value);
			}
		}
	}
}

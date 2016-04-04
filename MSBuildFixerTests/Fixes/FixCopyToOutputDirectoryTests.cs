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
	public class FixCopyToOutputDirectoryTests
	{
		[TestClass]
		public class OnVisitProjectMetadataTests
		{
			[TestMethod]
			public void SetsValue()
			{
				TestSetup.SetToggleTo(CopyToOutputDirectoryToggle.Instance, true);

				var projectRootElement = TestSetup.GetTestProject();

				var elements = projectRootElement.Items.Where(x=>x.ItemType.Equals("None")).ToList();
				Assert.IsTrue(elements.Any());

				var element = elements.SelectMany(x => x.Metadata).First(y => y.Name.Equals("CopyToOutputDirectory"));
				FixCopyToOutputDirectory.OnVisitProjectMetadata(element, EventArgs.Empty);
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
				FixCopyToOutputDirectory.OnVisitProjectMetadata(element, EventArgs.Empty);
				Assert.AreNotEqual("PreserveNewest", element.Value);
			}

			[TestMethod]
			public void NullInput()
			{
				FixCopyToOutputDirectory.OnVisitProjectMetadata(null, EventArgs.Empty);
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
				FixCopyToOutputDirectory.OnVisitProjectMetadata(element, EventArgs.Empty);
				Assert.AreNotEqual("PreserveNewest", element.Value);
			}
		}
	}
}

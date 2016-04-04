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
	public class FixCopyLocalTests
	{
		[TestClass]
		public class OnVisitMetadataTests
		{
			[TestMethod]
			public void SetsValue()
			{
				TestSetup.SetToggleTo(CopyLocalToggle.Instance, true);

				var projectRootElement = TestSetup.GetTestProject();

				var metadataElements = projectRootElement.AllChildren.OfType<ProjectMetadataElement>().Where(x=>x.Name.Equals("Private")).ToList();
				Assert.IsTrue(metadataElements.Any());

				var element = metadataElements[0];
				var fixCopyLocal = new FixCopyLocal();
				fixCopyLocal.OnVisitMetadata(element, EventArgs.Empty);
				Assert.AreEqual(false.ToString(), element.Value);
			}

			[TestMethod]
			public void ToggleBlocks()
			{
				TestSetup.SetToggleTo(CopyLocalToggle.Instance, false);

				var projectRootElement = TestSetup.GetTestProject();

				var metadataElements = projectRootElement.AllChildren.OfType<ProjectMetadataElement>().Where(x => x.Name.Equals("Private")).ToList();
				Assert.IsTrue(metadataElements.Any());

				var element = metadataElements[0];
				var fixCopyLocal = new FixCopyLocal();
				fixCopyLocal.OnVisitMetadata(element, EventArgs.Empty);
				Assert.AreEqual(true.ToString(), element.Value);
			}

			[TestMethod]
			public void NullInput()
			{
				var fixCopyLocal = new FixCopyLocal();
				fixCopyLocal.OnVisitMetadata(null, EventArgs.Empty);
			}

			[TestMethod]
			public void Notelement()
			{
				TestSetup.SetToggleTo(CopyLocalToggle.Instance, true);

				var projectRootElement = TestSetup.GetTestProject();

				var metadataElements = projectRootElement.AllChildren.OfType<ProjectMetadataElement>().Where(x => x.Name.Equals("Private")).ToList();
				Assert.IsTrue(metadataElements.Any());

				var element = metadataElements[0];
				element.Name = "meow";
				var fixCopyLocal = new FixCopyLocal();
				fixCopyLocal.OnVisitMetadata(element, EventArgs.Empty);
				Assert.AreEqual(true.ToString(), element.Value);
			}
		}

		[TestClass]
		public class OnVisitProjectItemTests
		{
			[TestMethod]
			public void AddsMetadata()
			{
				TestSetup.SetToggleTo(CopyLocalToggle.Instance, true);

				var projectRootElement = TestSetup.GetTestProject();

				var element = projectRootElement.Items.First(x => x.ItemType.Equals("Reference"));
				Assert.IsFalse(element.Metadata.Any(x=>x.Name.Equals("Private")));

				var fixCopyLocal = new FixCopyLocal();
				fixCopyLocal.OnVisitProjectItem(element, EventArgs.Empty);
				Assert.IsTrue(element.Metadata.Any(x => x.Name.Equals("Private")));
				Assert.AreEqual(false.ToString(), element.Metadata.First(x => x.Name.Equals("Private")).Value);
			}

			[TestMethod]
			public void ToggleBlocks()
			{
				TestSetup.SetToggleTo(CopyLocalToggle.Instance, false);

				var projectRootElement = TestSetup.GetTestProject();

				var element = projectRootElement.Items.First(x => x.ItemType.Equals("Reference"));
				Assert.IsFalse(element.Metadata.Any(x => x.Name.Equals("Private")));

				var fixCopyLocal = new FixCopyLocal();
				fixCopyLocal.OnVisitProjectItem(element, EventArgs.Empty);
				Assert.IsFalse(element.Metadata.Any(x => x.Name.Equals("Private")));
			}

			[TestMethod]
			public void NullInput()
			{
				var fixCopyLocal = new FixCopyLocal();
				fixCopyLocal.OnVisitMetadata(null, EventArgs.Empty);
			}

			[TestMethod]
			public void IsGacAssembly()
			{
				TestSetup.SetToggleTo(CopyLocalToggle.Instance, true);

				var projectRootElement = TestSetup.GetTestProject();

				var element = projectRootElement.Items.First(x=>x.Include.Equals("Microsoft.VisualBasic"));
				Assert.IsFalse(element.Metadata.Any(x => x.Name.Equals("Private")));

				var fixCopyLocal = new FixCopyLocal();
				fixCopyLocal.OnVisitProjectItem(element, EventArgs.Empty);
				Assert.IsFalse(element.Metadata.Any(x => x.Name.Equals("Private")));
			}

			[TestMethod]
			public void HasMetadata()
			{
				TestSetup.SetToggleTo(CopyLocalToggle.Instance, true);

				var projectRootElement = TestSetup.GetTestProject();

				var element = projectRootElement.Items.First(x => x.Include.Equals("IdeaBlade.Util, Version=3.6.4.1, Culture=neutral, PublicKeyToken=287b5094865421c0, processorArchitecture=MSIL"));
				var count = element.Metadata.Count;

				var fixCopyLocal = new FixCopyLocal();
				fixCopyLocal.OnVisitProjectItem(element, EventArgs.Empty);
				Assert.AreEqual(count, element.Metadata.Count);
			}

			[TestMethod]
			public void NotRefence()
			{
				TestSetup.SetToggleTo(CopyLocalToggle.Instance, true);

				var projectRootElement = TestSetup.GetTestProject();

				var element = projectRootElement.Items.First(x => x.ItemType.Equals("Reference"));
				element.ItemType = "NotReference";
				Assert.IsFalse(element.Metadata.Any(x => x.Name.Equals("Private")));

				var fixCopyLocal = new FixCopyLocal();
				fixCopyLocal.OnVisitProjectItem(element, EventArgs.Empty);
				Assert.IsFalse(element.Metadata.Any(x => x.Name.Equals("Private")));
			}

			[TestMethod]
			public void NotProjectItem()
			{
				TestSetup.SetToggleTo(CopyLocalToggle.Instance, true);

				var projectRootElement = TestSetup.GetTestProject();

				var element = projectRootElement.Items.First(x => x.ItemType.Equals("Reference"));
				var count = element.Metadata.Count;

				var fixCopyLocal = new FixCopyLocal();
				fixCopyLocal.OnVisitProjectItem(element.Metadata.First(), EventArgs.Empty);
				Assert.AreEqual(count, element.Metadata.Count);
			}
		}
	}
}

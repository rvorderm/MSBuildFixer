using FakeItEasy;
using Microsoft.Build.Construction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSBuildFixer.Fixes;
using MSBuildFixer.SampleFeatureToggles;
using System;
using System.Linq;

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
				fixCopyLocal.OnVisitMetadata(element);
				Assert.AreEqual(false.ToString(), element.Value);
			}

			[TestMethod]
			public void NullInput()
			{
				var fixCopyLocal = new FixCopyLocal();
				fixCopyLocal.OnVisitMetadata(null);
			}

			[TestMethod]
			public void Notelement()
			{
				TestSetup.SetToggleTo(CopyLocalToggle.Instance, true);

				var projectRootElement = TestSetup.GetTestProject();

				var metadataElements = projectRootElement.AllChildren.OfType<ProjectMetadataElement>().Where(x => x.Name.Equals("Private") && x.Value.Equals(true.ToString())).ToList();
				Assert.IsTrue(metadataElements.Any());

				var element = metadataElements[0];
				element.Name = "meow";
				var fixCopyLocal = new FixCopyLocal();
				fixCopyLocal.OnVisitMetadata(element);
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

				var element = projectRootElement.Items.First(x => x.ItemType.Equals("Reference") && !x.Metadata.Any(y=>y.Name.Equals("Private")));
				Assert.IsFalse(element.Metadata.Any(x=>x.Name.Equals("Private")));

				var fixCopyLocal = new FixCopyLocal();
				fixCopyLocal.OnVisitProjectItem(element);
				Assert.IsTrue(element.Metadata.Any(x => x.Name.Equals("Private")));
				Assert.AreEqual(false.ToString(), element.Metadata.First(x => x.Name.Equals("Private")).Value);
			}

			[TestMethod]
			public void NullInput()
			{
				var fixCopyLocal = new FixCopyLocal();
				fixCopyLocal.OnVisitMetadata(null);
			}

			[TestMethod]
			public void IsGacAssembly()
			{
				TestSetup.SetToggleTo(CopyLocalToggle.Instance, true);

				var projectRootElement = TestSetup.GetTestProject();

				var element = projectRootElement.Items.First(x=>x.Include.Equals("Microsoft.VisualBasic"));
				Assert.IsFalse(element.Metadata.Any(x => x.Name.Equals("Private")));

				var fixCopyLocal = new FixCopyLocal();
				fixCopyLocal.OnVisitProjectItem(element);
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
				fixCopyLocal.OnVisitProjectItem(element);
				Assert.AreEqual(count, element.Metadata.Count);
			}

			[TestMethod]
			public void NotRefence()
			{
				TestSetup.SetToggleTo(CopyLocalToggle.Instance, true);

				var projectRootElement = TestSetup.GetTestProject();

				var element = projectRootElement.Items.First(x => x.ItemType.Equals("Reference") && !x.Metadata.Any(y=>y.Name.Equals("Private")));
				element.ItemType = "NotReference";
				Assert.IsFalse(element.Metadata.Any(x => x.Name.Equals("Private")));

				var fixCopyLocal = new FixCopyLocal();
				fixCopyLocal.OnVisitProjectItem(element);
				Assert.IsFalse(element.Metadata.Any(x => x.Name.Equals("Private")));
			}
		}
	}
}

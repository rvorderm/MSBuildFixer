using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Construction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSBuildFixer.Fixes;
using MSBuildFixer.SampleFeatureToggles;

namespace MSBuildFixerTests.Fixes
{
	public class FixHintPathTests
	{
		[TestClass]
		public class CtorTests
		{
			[TestMethod]
			[ExpectedException(typeof(ArgumentException))]
			public void SolutionPathNull()
			{
				new FixHintPath(null, "world");
			}

			[TestMethod]
			[ExpectedException(typeof(ArgumentException))]
			public void LibraryDirNull()
			{
				new FixHintPath("hello", null);
			}

			[TestMethod]
			public void ValidPath()
			{
				var path = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetTempPath()));
				var folder = Path.GetFileName(Path.GetDirectoryName(Path.GetTempPath()));
				new FixHintPath(path, folder);
			}

			[TestMethod]
			[ExpectedException(typeof(ArgumentException))]
			public void InvalidPath()
			{
				var path = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetTempPath()));
				var folder = "NotValid";
				new FixHintPath(path, folder);
			}
		}

		[TestClass]
		public class OnVisitMetadataTests
		{
			[TestMethod]
			[ExpectedException(typeof(ArgumentNullException))]
			public void SetsValue()
			{
				TestSetup.SetToggleTo(HintPathToggle.Instance, true);

				var projectRootElement = TestSetup.GetTestProject();

				var metadataElements = projectRootElement.AllChildren.OfType<ProjectMetadataElement>().Where(x => x.Name.Equals("HintPath")).ToList();
				Assert.IsTrue(metadataElements.Any());

				var fileStream = File.Create(Path.Combine(Path.GetTempPath(), "IdeaBlade.Persistence.dll"));
				fileStream.Close();

				var path = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetTempPath()));
				var folder = Path.GetFileName(Path.GetDirectoryName(Path.GetTempPath()));

				var element = metadataElements[0];
				var fixHintPath = new FixHintPath(path, folder);
				fixHintPath.OnVisitMetadata(element, EventArgs.Empty);
				Assert.AreEqual(Path.Combine(Path.GetTempPath(), "IdeaBlade.Persistence.dll"), element.Value);
			}

			[TestMethod]
			public void ToggleBlocks()
			{
				TestSetup.SetToggleTo(HintPathToggle.Instance, false);

				var projectRootElement = TestSetup.GetTestProject();

				var metadataElements = projectRootElement.AllChildren.OfType<ProjectMetadataElement>().Where(x => x.Name.Equals("HintPath")).ToList();
				Assert.IsTrue(metadataElements.Any());

				var path = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetTempPath()));
				var folder = Path.GetFileName(Path.GetDirectoryName(Path.GetTempPath()));

				var element = metadataElements[0];
				var fixHintPath = new FixHintPath(path, folder);
				fixHintPath.OnVisitMetadata(element, EventArgs.Empty);
				Assert.AreEqual("..\\.lib\\Ideablade\\IdeaBlade.Persistence.dll", element.Value);
			}

			[TestMethod]
			public void NullInput()
			{
				var path = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetTempPath()));
				var folder = Path.GetFileName(Path.GetDirectoryName(Path.GetTempPath()));

				var fixHintPath = new FixHintPath(path, folder);
				fixHintPath.OnVisitMetadata(null, EventArgs.Empty);
			}

			[TestMethod]
			public void WrongType()
			{
				TestSetup.SetToggleTo(HintPathToggle.Instance, false);

				var projectRootElement = TestSetup.GetTestProject();

				var item = projectRootElement.AllChildren.OfType<ProjectItemElement>().First();

				var path = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetTempPath()));
				var folder = Path.GetFileName(Path.GetDirectoryName(Path.GetTempPath()));

				var fixHintPath = new FixHintPath(path, folder);
				fixHintPath.OnVisitMetadata(item, EventArgs.Empty);
			}

			[TestMethod]
			public void Notelement()
			{
				TestSetup.SetToggleTo(HintPathToggle.Instance, true);

				var projectRootElement = TestSetup.GetTestProject();

				var metadataElements = projectRootElement.AllChildren.OfType<ProjectMetadataElement>().Where(x => x.Name.Equals("HintPath")).ToList();
				Assert.IsTrue(metadataElements.Any());

				var path = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetTempPath()));
				var folder = Path.GetFileName(Path.GetDirectoryName(Path.GetTempPath()));

				var element = metadataElements[0];
				element.Name = "Meow";
				var fixHintPath = new FixHintPath(path, folder);
				fixHintPath.OnVisitMetadata(element, EventArgs.Empty);
				Assert.AreEqual("..\\.lib\\Ideablade\\IdeaBlade.Persistence.dll", element.Value);
			}
		}
	}
}

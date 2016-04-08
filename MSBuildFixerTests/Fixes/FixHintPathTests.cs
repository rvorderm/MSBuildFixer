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
		public class OnVisitProjectItemTests
		{
			[TestMethod]
			public void WrongType()
			{
				var projectRootElement = TestSetup.GetTestProject();
				var projectItemElement = projectRootElement.Properties.First();
				var path = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetTempPath()));
				var folder = Path.GetFileName(Path.GetDirectoryName(Path.GetTempPath()));

				var fixHintPath = new FixHintPath(path, folder);
				fixHintPath.OnVisitProjectItem(projectItemElement, EventArgs.Empty);
			}

			[TestMethod]
			public void ToggleDiabled()
			{
				TestSetup.SetToggleTo(HintPathToggle.Instance, false);
				var projectRootElement = TestSetup.GetTestProject();
				var projectItemElement = projectRootElement.Items
					.First(x => x.Include.Contains("IdeaBlade.Persistence.Rdb") && !x.Metadata.Any(m => m.Name.Equals("HintPath")));
				var path = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetTempPath()));
				var folder = Path.GetFileName(Path.GetDirectoryName(Path.GetTempPath()));

				var fixHintPath = new FixHintPath(path, folder);
				fixHintPath.OnVisitProjectItem(projectItemElement, EventArgs.Empty);
				Assert.AreEqual(1, projectItemElement.Metadata.Count);
			}

			[TestMethod]
			public void EmptyCollection()
			{
				TestSetup.SetToggleTo(HintPathToggle.Instance, true);
				var projectRootElement = TestSetup.GetTestProject();
				var projectItemElement = projectRootElement.Items
					.First(x => x.Include.Contains("IdeaBlade.Persistence.Rdb") && !x.Metadata.Any(m => m.Name.Equals("HintPath")));
				projectItemElement.RemoveChild(projectItemElement.Metadata.First());
				Assert.AreEqual(0, projectItemElement.Metadata.Count);
				var path = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetTempPath()));
				var folder = Path.GetFileName(Path.GetDirectoryName(Path.GetTempPath()));

				var fixHintPath = new FixHintPath(path, folder);
				fixHintPath.OnVisitProjectItem(projectItemElement, EventArgs.Empty);
				Assert.AreEqual(0, projectItemElement.Metadata.Count);
			}

			[TestMethod]
			public void ParentNotReference()
			{
				TestSetup.SetToggleTo(HintPathToggle.Instance, true);
				var projectRootElement = TestSetup.GetTestProject();
				var projectItemElement = projectRootElement.Items
					.First(x => x.Include.Contains("IdeaBlade.Persistence.Rdb") && !x.Metadata.Any(m => m.Name.Equals("HintPath")));
				var path = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetTempPath()));
				var folder = Path.GetFileName(Path.GetDirectoryName(Path.GetTempPath()));

				var fixHintPath = new FixHintPath(path, folder);
				projectItemElement.ItemType = "NotRef";
				fixHintPath.OnVisitProjectItem(projectItemElement, EventArgs.Empty);
				Assert.AreEqual(1, projectItemElement.Metadata.Count);
			}

			[TestMethod]
			public void ParentWrongType()
			{
				TestSetup.SetToggleTo(HintPathToggle.Instance, true);
				var projectRootElement = TestSetup.GetTestProject();
				var projectItemElement = projectRootElement.Properties.First();
				
				
				var path = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetTempPath()));
				var folder = Path.GetFileName(Path.GetDirectoryName(Path.GetTempPath()));

				var fixHintPath = new FixHintPath(path, folder);
				fixHintPath.OnVisitProjectItem(projectItemElement, EventArgs.Empty);
			}

			[TestMethod]
			public void ShouldNotInsert()
			{
				TestSetup.SetToggleTo(HintPathToggle.Instance, true);
				var projectRootElement = TestSetup.GetTestProject();
				var projectItemElement = projectRootElement.Items
					.First(x => x.Include.Contains("IdeaBlade") && x.Metadata.Any(m => m.Name.Equals("HintPath")));

				var path = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetTempPath()));
				var folder = Path.GetFileName(Path.GetDirectoryName(Path.GetTempPath()));

				var filePath = Path.Combine(Path.GetTempPath(), "IdeaBlade.Persistence.Rdb.dll");
				if (!File.Exists(filePath)) File.Create(filePath).Close();

				var fixHintPath = new FixHintPath(path, folder);
				fixHintPath.OnVisitProjectItem(projectItemElement, EventArgs.Empty);
				Assert.AreEqual(2, projectItemElement.Metadata.Count);
				var hintPath = projectItemElement.Metadata.Skip(1).First();
				Assert.AreEqual("HintPath", hintPath.Name);
				Assert.AreEqual(@"..\.lib\Ideablade\IdeaBlade.Persistence.dll", hintPath.Value);
			}

			[TestMethod]
			public void ShouldInsert()
			{
				TestSetup.SetToggleTo(HintPathToggle.Instance, true);
				var projectRootElement = TestSetup.GetTestProject();
				var projectItemElement = projectRootElement.Items
					.First(x => x.Include.Contains("IdeaBlade.Persistence.Rdb") && !x.Metadata.Any(m => m.Name.Equals("HintPath")));

				var path = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetTempPath()));
				var folder = Path.GetFileName(Path.GetDirectoryName(Path.GetTempPath()));

				var filePath = Path.Combine(Path.GetTempPath(), "IdeaBlade.Persistence.Rdb.dll");
				if(!File.Exists(filePath)) File.Create(filePath).Close();

				var fixHintPath = new FixHintPath(path, folder);
				fixHintPath.OnVisitProjectItem(projectItemElement, EventArgs.Empty);
				Assert.AreEqual(2, projectItemElement.Metadata.Count);
				var hintPath = projectItemElement.Metadata.Skip(1).First();
				Assert.AreEqual("HintPath", hintPath.Name);
				Assert.AreEqual("$(SolutionDir)\\Temp\\IdeaBlade.Persistence.Rdb.dll", hintPath.Value);
			}
		}

		[TestClass]
		public class ShouldInsertTests
		{
			[TestMethod]
			public void IsMicroftAssembly()
			{
				var projectItemElement = TestSetup.GetTestProject().Items.First();
				projectItemElement.Include = "Microsoft.VisualBasic";

				var path = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetTempPath()));
				var folder = Path.GetFileName(Path.GetDirectoryName(Path.GetTempPath()));
				var fixHintPath = new FixHintPath(path, folder);

				Assert.IsFalse(fixHintPath.ShouldInsert(projectItemElement));
			}

			[TestMethod]
			public void IsSystemAssembly()
			{
				var projectItemElement = TestSetup.GetTestProject().Items.First();
				projectItemElement.Include = "System";

				var path = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetTempPath()));
				var folder = Path.GetFileName(Path.GetDirectoryName(Path.GetTempPath()));
				var fixHintPath = new FixHintPath(path, folder);

				Assert.IsFalse(fixHintPath.ShouldInsert(projectItemElement));
			}

			[TestMethod]
			public void HasHintPath()
			{
				var projectItemElement = TestSetup.GetTestProject().Items.First();

				var path = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetTempPath()));
				var folder = Path.GetFileName(Path.GetDirectoryName(Path.GetTempPath()));
				var fixHintPath = new FixHintPath(path, folder);

				Assert.IsFalse(fixHintPath.ShouldInsert(projectItemElement));
			}

			[TestMethod]
			public void ShouldInsert()
			{
				var projectItemElement = TestSetup.GetTestProject().Items.First();
				var projectMetadataElement = projectItemElement.Metadata.First(x=>x.Name.Equals("HintPath"));
				projectItemElement.RemoveChild(projectMetadataElement);

				var path = Path.GetDirectoryName(Path.GetDirectoryName(Path.GetTempPath()));
				var folder = Path.GetFileName(Path.GetDirectoryName(Path.GetTempPath()));
				var fixHintPath = new FixHintPath(path, folder);

				Assert.IsTrue(fixHintPath.ShouldInsert(projectItemElement));
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
				Assert.AreEqual(@"$(SolutionDir)\Temp\IdeaBlade.Persistence.dll", element.Value);
				//Assert.AreEqual(Path.Combine(Path.GetTempPath(), "IdeaBlade.Persistence.dll"), element.Value);
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

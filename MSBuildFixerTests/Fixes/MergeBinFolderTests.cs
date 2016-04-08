using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSBuildFixer.Fixes;

namespace MSBuildFixerTests.Fixes
{
	[TestClass]
	public class MergeBinFolderTests
	{
		public string _testFolder = Path.Combine(Path.GetTempPath(), "MSBuildFixerTests");

		[TestClass]
		public class MergeProjectIntoRootTests : MergeBinFolderTests
		{
			[TestMethod]
			public void MergeOneFolder()
			{
				var root = Path.Combine(_testFolder, "MergeIntoOneFolder");
				var project = Path.Combine(root, "Project");
				var destination = Path.Combine(root, "bin");

				if(Directory.Exists(project)) Directory.Delete(project, true);
				if(Directory.Exists(destination)) Directory.Delete(destination, true);

				Directory.CreateDirectory(Path.Combine(project, "bin"));

				var randomFileName = Path.GetRandomFileName();
				File.Create(Path.Combine(project, "bin", randomFileName)).Close();

				MergeBinFolders.MergeProjectIntoRoot(project, root);

				Assert.IsTrue(File.Exists(Path.Combine(destination, randomFileName)));
			}

			[TestMethod]
			public void MergeNestedFolder()
			{
				var root = Path.Combine(_testFolder, "MergeNestedFolder");
				var project = Path.Combine(root, "Project");
				var destination = Path.Combine(root, "bin");

				if(Directory.Exists(project)) Directory.Delete(project, true);
				if(Directory.Exists(destination)) Directory.Delete(destination, true);

				Directory.CreateDirectory(Path.Combine(project, "bin", "nested"));

				var randomFileName = Path.GetRandomFileName();
				File.Create(Path.Combine(project, "bin", "nested", randomFileName)).Close();

				MergeBinFolders.MergeProjectIntoRoot(project, root);

				Assert.IsTrue(File.Exists(Path.Combine(destination, "nested", randomFileName)));
			}

			[TestMethod]
			public void SourceIsDestination()
			{
				var testFolder = _testFolder;
				Directory.CreateDirectory(Path.Combine(_testFolder, "bin"));
				MergeBinFolders.MergeProjectIntoRoot(testFolder, testFolder);
			}

			[TestMethod]
			public void SourceNull()
			{
				MergeBinFolders.MergeProjectIntoRoot(null, _testFolder);
			}

			[TestMethod]
			public void DestinationNull()
			{
				MergeBinFolders.MergeProjectIntoRoot(_testFolder, null);
			}
		}
	}
}

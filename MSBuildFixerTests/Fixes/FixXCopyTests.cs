using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Construction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSBuildFixer.Fixes;
using MSBuildFixerTests.Properties;

namespace MSBuildFixerTests.Fixes
{
	[TestClass]
	public class FixXCopyTests
	{
		[TestClass]
		public class GetDestinationTests
		{
			[TestMethod]
			public void InvalidText()
			{
				string destination = FixXCopy.GetDestination(@"xcopy /y");
				Assert.IsNull(destination);
			}


			[TestMethod]
			public void Standard()
			{
				string destination = FixXCopy.GetDestination(@"xcopy /y $(TargetDir)$(TargetName).* $(SolutionDir)\.out\*.*");
				Assert.AreEqual(@"$(SolutionDir)\.out\*.*", destination);
			}
		}

		[TestClass]
		public class OnVisitMetadataTests
		{
			[TestMethod]
			public void Null()
			{
				var fixXCopy = new FixXCopy();
				fixXCopy.OnVisitMetadata(null, EventArgs.Empty);
			}

			[TestMethod]
			public void WrongType()
			{
				var projectRootElement = TestSetup.GetTestProject();
				var projectItemElement = projectRootElement.Items.First();
				var fixXCopy = new FixXCopy();
				fixXCopy.OnVisitMetadata(projectItemElement, EventArgs.Empty);
				Assert.IsFalse(fixXCopy.GetReferences(projectRootElement).Any());
			}

			[TestMethod]
			public void WrongName()
			{
				var projectRootElement = TestSetup.GetTestProject();
				var metadataElement = projectRootElement.AllChildren.OfType<ProjectMetadataElement>().First();
				var fixXCopy = new FixXCopy();
				fixXCopy.OnVisitMetadata(metadataElement, EventArgs.Empty);
				Assert.IsFalse(fixXCopy.GetReferences(projectRootElement).Any());
			}

			[TestMethod]
			public void HintPath()
			{
				var projectRootElement = TestSetup.GetTestProject();
				var metadataElement = projectRootElement.AllChildren.OfType<ProjectMetadataElement>().First(x=>x.Name.Equals("HintPath"));
				var fixXCopy = new FixXCopy();
				fixXCopy.OnVisitMetadata(metadataElement, EventArgs.Empty);
				Assert.IsTrue(fixXCopy.GetReferences(projectRootElement).Any());
			}
		}

		[TestClass]
		public class ProcessBuildEventTests
		{
			[TestMethod]
			public void RemovesXCopies()
			{
				var projectRootElement = TestSetup.GetTestProject();
				var element = projectRootElement.AllChildren.OfType<ProjectPropertyElement>().First(x => x.Name.Equals("PostBuildEvent"));
				var fixXCopy = new FixXCopy();

				Assert.IsTrue(element.Value.Contains("xcopy"));
				fixXCopy.ProcessBuildEvent(element);
				Assert.IsFalse(element.Value.Contains("xcopy"));
			}

			[TestMethod]
			public void AddsXCopiesToSet()
			{
				var projectRootElement = TestSetup.GetTestProject();
				var element = projectRootElement.AllChildren.OfType<ProjectPropertyElement>().First(x => x.Name.Equals("PostBuildEvent"));
				var fixXCopy = new FixXCopy();
				var count = element.Value.Select((c, i) => element.Value.Substring(i)).Count(sub => sub.StartsWith("xcopy"));
				Assert.IsTrue(element.Value.Contains("xcopy"));
				var xCopies = fixXCopy.GetXCopies(projectRootElement);
				Assert.IsFalse(xCopies.Any());
				fixXCopy.ProcessBuildEvent(element);
				xCopies = fixXCopy.GetXCopies(projectRootElement);
				Assert.AreEqual(count, xCopies.Count());
			}
		}

		[TestClass]
		public class OnVisitPropertyTests
		{
			[TestMethod]
			public void Null()
			{
				var fixXCopy = new FixXCopy();
				fixXCopy.OnVisitProperty(null, EventArgs.Empty);
			}

			[TestMethod]
			public void WrongType()
			{
				var projectRootElement = TestSetup.GetTestProject();
				var projectItemElement = projectRootElement.Items.First();
				var fixXCopy = new FixXCopy();
				fixXCopy.OnVisitProperty(projectItemElement, EventArgs.Empty);
				Assert.IsFalse(fixXCopy.GetXCopies(projectRootElement).Any());
				Assert.IsNull(fixXCopy.GetAssembly(projectRootElement));
				Assert.IsFalse(fixXCopy.GetOutputPaths(projectRootElement).Any());
			}

			[TestMethod]
			public void WrongName()
			{
				var projectRootElement = TestSetup.GetTestProject();
				var element = projectRootElement.AllChildren.OfType<ProjectPropertyElement>().First();
				element.Name = "Invalid";
				var fixXCopy = new FixXCopy();
				fixXCopy.OnVisitProperty(element, EventArgs.Empty);
				Assert.IsFalse(fixXCopy.GetXCopies(projectRootElement).Any());
				Assert.IsNull(fixXCopy.GetAssembly(projectRootElement));
				Assert.IsFalse(fixXCopy.GetOutputPaths(projectRootElement).Any());
			}

			[TestMethod]
			public void AssemblyName()
			{
				var projectRootElement = TestSetup.GetTestProject();
				var element = projectRootElement.AllChildren.OfType<ProjectPropertyElement>().First(x=>x.Name.Equals("AssemblyName"));
				var fixXCopy = new FixXCopy();
				fixXCopy.OnVisitProperty(element, EventArgs.Empty);
				Assert.IsFalse(fixXCopy.GetXCopies(projectRootElement).Any());
				Assert.IsNotNull(fixXCopy.GetAssembly(projectRootElement));
				Assert.IsFalse(fixXCopy.GetOutputPaths(projectRootElement).Any());
			}

			[TestMethod]
			public void OutputPath()
			{
				var projectRootElement = TestSetup.GetTestProject();
				var element = projectRootElement.AllChildren.OfType<ProjectPropertyElement>().First(x => x.Name.Equals("OutputPath"));
				var fixXCopy = new FixXCopy();
				fixXCopy.OnVisitProperty(element, EventArgs.Empty);
				Assert.IsFalse(fixXCopy.GetXCopies(projectRootElement).Any());
				Assert.IsNull(fixXCopy.GetAssembly(projectRootElement));
				Assert.IsTrue(fixXCopy.GetOutputPaths(projectRootElement).Any());
			}

			[TestMethod]
			public void PostBuildEvent()
			{
				var projectRootElement = TestSetup.GetTestProject();
				var element = projectRootElement.AllChildren.OfType<ProjectPropertyElement>().First(x => x.Name.Equals("PostBuildEvent"));
				var fixXCopy = new FixXCopy();
				fixXCopy.OnVisitProperty(element, EventArgs.Empty);
				Assert.IsTrue(fixXCopy.GetXCopies(projectRootElement).Any());
				Assert.IsNull(fixXCopy.GetAssembly(projectRootElement));
				Assert.IsFalse(fixXCopy.GetOutputPaths(projectRootElement).Any());
			}
		}

		[TestClass]
		public class CollateAllXCopiesTests
		{
			[TestMethod]
			public void GeneratesStringFromTestData()
			{
				var projectRootElement = TestSetup.GetTestProject();
				var fixXCopy = new FixXCopy();
				var assemblyNames = projectRootElement.Properties.Where(x => x.Name.Equals("AssemblyName"));
				foreach (var assemblyName in assemblyNames)
				{
					fixXCopy.OnVisitProperty(assemblyName, EventArgs.Empty);
				}

				var postBuilds = projectRootElement.Properties.Where(x => x.Name.Equals("PostBuildEvent"));
				foreach (var postBuild in postBuilds)
				{
					fixXCopy.OnVisitProperty(postBuild, EventArgs.Empty);
				}

				var collateAllXCopies = fixXCopy.CollateAllXCopies();
				Assert.AreEqual(Resources.CollatedXCopy, collateAllXCopies);
			}
		}
	}
}

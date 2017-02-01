using Microsoft.Build.Construction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSBuildFixer.Fixes;
using MSBuildFixerTests.Properties;
using System.Linq;

namespace MSBuildFixerTests.Fixes
{
	[TestClass]
	public class FixXCopyTests
	{
		[TestClass]
		public class OnOpenSolutionTests
		{
			[TestMethod]
			public void SendString()
			{
				var fixXCopy = new FixXCopy();
				fixXCopy.OnOpenSolution("fileName");
				Assert.AreEqual("fileName", fixXCopy.SolutionFilePath);
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
			public void WrongName()
			{
				var projectRootElement = TestSetup.GetTestProject();
				var element = projectRootElement.AllChildren.OfType<ProjectPropertyElement>().First();
				element.Name = "Invalid";
				var fixXCopy = new FixXCopy();
				fixXCopy.OnVisitProperty(element);
				Assert.IsFalse(fixXCopy.GetXCopies(projectRootElement).Any());
				Assert.IsNull(fixXCopy.GetAssembly(projectRootElement));
			}

			[TestMethod]
			public void AssemblyName()
			{
				var projectRootElement = TestSetup.GetTestProject();
				var element = projectRootElement.AllChildren.OfType<ProjectPropertyElement>().First(x=>x.Name.Equals("AssemblyName"));
				var fixXCopy = new FixXCopy();
				fixXCopy.OnVisitProperty(element);
				Assert.IsFalse(fixXCopy.GetXCopies(projectRootElement).Any());
				Assert.IsNotNull(fixXCopy.GetAssembly(projectRootElement));
			}

			[TestMethod]
			public void OutputPath()
			{
				var projectRootElement = TestSetup.GetTestProject();
				var element = projectRootElement.AllChildren.OfType<ProjectPropertyElement>().First(x => x.Name.Equals("OutputPath"));
				var fixXCopy = new FixXCopy();
				fixXCopy.OnVisitProperty(element);
				Assert.IsFalse(fixXCopy.GetXCopies(projectRootElement).Any());
				Assert.IsNull(fixXCopy.GetAssembly(projectRootElement));
			}

			[TestMethod]
			public void PostBuildEvent()
			{
				var projectRootElement = TestSetup.GetTestProject();
				var element = projectRootElement.AllChildren.OfType<ProjectPropertyElement>().First(x => x.Name.Equals("PostBuildEvent"));
				var fixXCopy = new FixXCopy();
				fixXCopy.OnVisitProperty(element);
				Assert.IsTrue(fixXCopy.GetXCopies(projectRootElement).Any());
				Assert.IsNull(fixXCopy.GetAssembly(projectRootElement));
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
					fixXCopy.OnVisitProperty(assemblyName);
				}

				var postBuilds = projectRootElement.Properties.Where(x => x.Name.Equals("PostBuildEvent"));
				foreach (var postBuild in postBuilds)
				{
					fixXCopy.OnVisitProperty(postBuild);
				}

				var collateAllXCopies = fixXCopy.CollateAllXCopies();
				Assert.AreEqual(Resources.CollatedXCopy, collateAllXCopies);
			}
		}
	}
}

using Microsoft.Build.Construction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSBuildFixer;
using MSBuildFixer.Fixes;
using MSBuildFixer.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace MSBuildFixerTests.Fixes
{
	[TestClass]
	public class FixCopyLocalTests
	{

		[TestMethod]
		public void NoCopyLocal()
		{
			var solutionWalker = new SolutionWalker(TestSetup.SolutionPath);
			new FixCopyLocal().AttachTo(solutionWalker);
			IEnumerable<ProjectRootElement> projectRootElements = solutionWalker.VisitSolution(false);
			foreach (ProjectRootElement projectRootElement in projectRootElements)
			{
				foreach (ProjectItemElement projectItemElement in projectRootElement.Items)
				{
					if(FixCopyLocal.IsGacAssembly(projectItemElement)) continue;
					if (!projectItemElement.ItemType.Contains("Reference")) continue;
					Assert.AreEqual(false.ToString(), ProjectItemElementHelpers.GetPrivate(projectItemElement).Value);
				}
			}
		}

		[TestMethod]
		public void FirstOnly()
		{
			var solutionWalker = new SolutionWalker(TestSetup.SolutionPath);
			var fixCopyLocal = new FixCopyLocal {CopyStyle = CopyStyle.FirstOnly};
			fixCopyLocal.AttachTo(solutionWalker);
			
			IEnumerable<ProjectRootElement> projectRootElements = solutionWalker.VisitSolution(false);
			IEnumerable<ProjectItemElement> projectItemElements = projectRootElements.SelectMany(x=>x.Items);
			var includes = new HashSet<string>();
			foreach (ProjectItemElement itemElement in projectItemElements)
			{

				if (FixCopyLocal.IsGacAssembly(itemElement)) continue;
				if (!itemElement.ItemType.Contains("Reference")) continue;

				bool visited = includes.Contains(itemElement.Include);
				includes.Add(itemElement.Include);
				string expected = !visited ?  true.ToString() : false.ToString();

				ProjectMetadataElement projectMetadataElement = ProjectItemElementHelpers.GetPrivate(itemElement);
				if (projectMetadataElement == null) continue;
				Assert.AreEqual(expected, projectMetadataElement.Value, $"{itemElement.ContainingProject.FullPath}, {itemElement.Include}");
			}
		}

		[TestMethod]
		public void LastOnly()
		{
			var solutionWalker = new SolutionWalker(TestSetup.SolutionPath);
			var fixCopyLocal = new FixCopyLocal { CopyStyle = CopyStyle.LastOnly };
			fixCopyLocal.AttachTo(solutionWalker);

			IEnumerable<ProjectRootElement> projectRootElements = solutionWalker.VisitSolution(false);
			IEnumerable<ProjectItemElement> projectItemElements = projectRootElements.SelectMany(x => x.Items).Reverse();
			var includes = new HashSet<string>();
			foreach (ProjectItemElement itemElement in projectItemElements)
			{

				if (FixCopyLocal.IsGacAssembly(itemElement)) continue;
				if (!itemElement.ItemType.Contains("Reference")) continue;

				bool visited = includes.Contains(itemElement.Include);
				includes.Add(itemElement.Include);
				string expected = !visited ? true.ToString() : false.ToString();

				ProjectMetadataElement projectMetadataElement = ProjectItemElementHelpers.GetPrivate(itemElement);
				if (projectMetadataElement == null) continue;
				Assert.AreEqual(expected, projectMetadataElement.Value, $"{itemElement.ContainingProject.FullPath}, {itemElement.Include}");
			}
		}
	}
}

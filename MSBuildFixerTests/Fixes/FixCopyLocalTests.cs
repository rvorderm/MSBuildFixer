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
			SolutionWalker solutionWalker = TestSetup.BuildWalker<FixCopyLocal>();
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
		public void FirstOccurence()
		{
			SolutionWalker solutionWalker = TestSetup.BuildWalker<FixCopyLocal>(x=>x.CopyStyle = CopyStyle.FirstOccurence);
			
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
		public void LastOccurence()
		{
			SolutionWalker solutionWalker = TestSetup.BuildWalker<FixCopyLocal>(x => x.CopyStyle = CopyStyle.LastOccurence);

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

		[TestMethod]
		public void MoveAllToFirstProject()
		{
			SolutionWalker solutionWalker = TestSetup.BuildWalker<FixCopyLocal>(x => x.CopyStyle = CopyStyle.MoveAllToFirstProject);

			IEnumerable<ProjectRootElement> projectRootElements = solutionWalker.VisitSolution(false);
			ProjectRootElement firstProject = projectRootElements.First();
			var includes = new HashSet<string>();

			foreach (ProjectItemElement projectItemElement in firstProject.Items)
			{
				if (FixCopyLocal.IsGacAssembly(projectItemElement)) continue;
				if (!projectItemElement.ItemType.Equals("Reference")) continue;
				ProjectMetadataElement projectMetadataElement = ProjectItemElementHelpers.GetPrivate(projectItemElement);
				Assert.AreEqual(true.ToString(), projectMetadataElement.Value);
				includes.Add(ProjectItemElementHelpers.GetAssemblyName(projectItemElement));
			}

			foreach (ProjectRootElement projectRootElement in projectRootElements.Skip(1))
			{
				foreach (ProjectItemElement projectItemElement in projectRootElement.Items)
				{
					if (FixCopyLocal.IsGacAssembly(projectItemElement)) continue;
					if (!projectItemElement.ItemType.Equals("Reference")) continue;
					ProjectMetadataElement projectMetadataElement = ProjectItemElementHelpers.GetPrivate(projectItemElement);
					Assert.AreEqual(false.ToString(), projectMetadataElement.Value);
					Assert.IsTrue(includes.Contains(ProjectItemElementHelpers.GetAssemblyName(projectItemElement)), $"Wasn't moved to first project: {projectItemElement.ContainingProject.Properties.FirstOrDefault(x=>x.Name.Equals("AssemblyName")).Value} - {projectItemElement.Include}");
				}
			}
		}
	}
}

using Microsoft.Build.Construction;
using MSBuildFixer.Configuration;
using MSBuildFixer.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace MSBuildFixer.Fixes
{
	public class FixReplaceProjectReferences : IFix
	{
		public void AttachTo(SolutionWalker walker)
		{
			walker.OnVisitProjectItem_ProjectReference += Walker_OnVisitProjectItem_ProjectReference;
			walker.OnVisitProjects += Walker_OnVisitProjects;
		}

		public Dictionary<string, ProjectInSolution> Replacements { get; set; } = new Dictionary<string, ProjectInSolution>();

		private void Walker_OnVisitProjects(IReadOnlyList<ProjectInSolution> projects)
		{
			foreach (ProjectReferenceReplacement projectReferenceReplacement in FixesConfiguration.Instance.ProjectReferenceReplacements)
			{
				ProjectInSolution replacement = projects.FirstOrDefault(x=>x.ProjectName.Equals(projectReferenceReplacement.Replacement));
				if (replacement == null) continue;
				Replacements[projectReferenceReplacement.Name] = replacement;
			}
		}

		private void Walker_OnVisitProjectItem_ProjectReference(ProjectItemElement projectItemElement)
		{
			string projectName = ProjectRootElementHelpers.GetAssemblyName(projectItemElement.ContainingProject);
			string assemblyName = ProjectItemElementHelpers.GetAssemblyName(projectItemElement);
			ProjectInSolution replacement;
			if (!Replacements.TryGetValue(assemblyName, out replacement)) return;
			projectItemElement.Parent.RemoveChild(projectItemElement);

			//Don't add a reference if it's already there
			if (
				projectItemElement.ContainingProject.Items.Any(
					x =>
						x.ItemType.Equals("ProjectReference") &&
						ProjectItemElementHelpers.GetAssemblyName(x).Equals(replacement.ProjectName))) return;
		    if (projectName.Equals(replacement.ProjectName)) return;
			ProjectRootElementHelpers.AddProjectReference(projectItemElement.ContainingProject, replacement);
		}
	}
}

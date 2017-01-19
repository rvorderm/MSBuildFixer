using Microsoft.Build.Construction;

namespace MSBuildFixer.Fixes
{
	public class FixCodeSigning : IFix
	{
		public void AttachTo(SolutionWalker walker)
		{
			walker.OnVisitProperty += Walker_OnVisitProperty
				;
		}

		private void Walker_OnVisitProperty(ProjectPropertyElement projectPropertyElement)
		{
			if (!projectPropertyElement.Name.Equals("SignAssembly")) return;
			projectPropertyElement.Value = false.ToString();
		}
	}
}

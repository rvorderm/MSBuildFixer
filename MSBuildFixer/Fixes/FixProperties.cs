using Microsoft.Build.Construction;
using MSBuildFixer.Configuration;
using MSBuildFixer.SampleFeatureToggles;
using static System.Configuration.ConfigurationManager;

namespace MSBuildFixer.Fixes
{
	public class FixProperties : IFix
	{
		public void OnVisitProperty(ProjectPropertyElement projectPropertyElement)
		{
			string value;
			bool hasValue = FixesConfiguration.Instance.TryGetProperty(projectPropertyElement.Name, out value);
			if (!hasValue) return;
			projectPropertyElement.Value = value;
		}

		public void AttachTo(SolutionWalker walker)
		{
			walker.OnVisitProperty += OnVisitProperty;
		}
	}
}

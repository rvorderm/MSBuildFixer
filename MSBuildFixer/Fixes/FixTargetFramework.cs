using Microsoft.Build.Construction;
using MSBuildFixer.SampleFeatureToggles;
using System;
using static System.Configuration.ConfigurationManager;

namespace MSBuildFixer.Fixes
{
	public class FixTargetFramework : IFix
	{
		private readonly string _targetFrameworkVersion;

		public FixTargetFramework(string targetFrameworkVersion)
		{
			_targetFrameworkVersion = targetFrameworkVersion;
		}

		public FixTargetFramework() : this(AppSettings["TargetFrameworkVersion"])
		{
		}

		public void OnVisitProperty(object sender, EventArgs e)
		{
			var projectPropertyElement = sender as ProjectPropertyElement;
			if (projectPropertyElement == null) return;
			if (!FixTargetFrameworkToggle.Enabled) return;
			if (!projectPropertyElement.Name.Equals("TargetFrameworkVersion")) return;

			projectPropertyElement.Value = _targetFrameworkVersion;
		}

		public void AttachTo(SolutionWalker walker)
		{
			walker.OnVisitProperty += OnVisitProperty;
		}
	}
}

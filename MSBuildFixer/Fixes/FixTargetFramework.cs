﻿using Microsoft.Build.Construction;
using MSBuildFixer.SampleFeatureToggles;
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

		public void OnVisitProperty(ProjectPropertyElement projectPropertyElement)
		{
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
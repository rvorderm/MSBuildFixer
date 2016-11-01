using FeatureToggle.Toggles;
using System;

namespace MSBuildFixer.SampleFeatureToggles
{
	public class TargetFrameworkToggle : SimpleFeatureToggle
	{
		private static readonly Lazy<TargetFrameworkToggle> Lazy =
		new Lazy<TargetFrameworkToggle>(() => new TargetFrameworkToggle());

		public static TargetFrameworkToggle Instance => Lazy.Value;

		public static bool Enabled => Instance.FeatureEnabled;
	}
}
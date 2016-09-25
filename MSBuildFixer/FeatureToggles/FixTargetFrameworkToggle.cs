using FeatureToggle.Toggles;
using System;

namespace MSBuildFixer.SampleFeatureToggles
{
	public class FixTargetFrameworkToggle : SimpleFeatureToggle
	{
		private static readonly Lazy<FixTargetFrameworkToggle> Lazy =
		new Lazy<FixTargetFrameworkToggle>(() => new FixTargetFrameworkToggle());

		public static FixTargetFrameworkToggle Instance => Lazy.Value;

		public static bool Enabled => Instance.FeatureEnabled;
	}
}
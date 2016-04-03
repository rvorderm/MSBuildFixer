using System;
using FeatureToggle.Core;
using FeatureToggle.Toggles;

namespace MSBuildFixer.SampleFeatureToggles
{
	public class RunPostBuildEventToggle : SimpleFeatureToggle
	{
		private static readonly Lazy<RunPostBuildEventToggle> Lazy =
		new Lazy<RunPostBuildEventToggle>(() => new RunPostBuildEventToggle());

		public static RunPostBuildEventToggle Instance => Lazy.Value;

		public static bool Enabled => Instance.FeatureEnabled;
	}
}
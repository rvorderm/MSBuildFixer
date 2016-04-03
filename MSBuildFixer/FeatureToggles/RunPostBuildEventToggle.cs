using System;
using FeatureToggle.Core;
using FeatureToggle.Toggles;

namespace MSBuildFixer.SampleFeatureToggles
{
	public class RunPostBuildEventToggle : SimpleFeatureToggle
	{
		private static readonly Lazy<IFeatureToggle> Lazy =
		new Lazy<IFeatureToggle>(() => 
			new DefaultToDisabledOnErrorDecorator(
				new RunPostBuildEventToggle()));

		public static IFeatureToggle Instance => Lazy.Value;

		public static bool Enabled => Instance.FeatureEnabled;
	}
}
using System;
using System.Dynamic;
using FeatureToggle.Toggles;

namespace MSBuildFixer.SampleFeatureToggles
{
	public class HintPathToggle : SimpleFeatureToggle
	{
		private static readonly Lazy<HintPathToggle> Lazy =
		new Lazy<HintPathToggle>(() => new HintPathToggle());

		public static HintPathToggle Instance => Lazy.Value;

		public static bool Enabled => Instance.FeatureEnabled;
	}
}
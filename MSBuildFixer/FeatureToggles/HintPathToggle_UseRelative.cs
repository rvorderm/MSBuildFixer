using System;
using System.Dynamic;
using FeatureToggle.Toggles;

namespace MSBuildFixer.SampleFeatureToggles
{
	public class HintPathToggle_UseRelative : SimpleFeatureToggle
	{
		private static readonly Lazy<HintPathToggle_UseRelative> Lazy =
		new Lazy<HintPathToggle_UseRelative>(() => new HintPathToggle_UseRelative());

		public static HintPathToggle_UseRelative Instance => Lazy.Value;

		public static bool Enabled => Instance.FeatureEnabled;
	}
}
using System;
using System.Dynamic;
using FeatureToggle.Toggles;

namespace MSBuildFixer.SampleFeatureToggles
{
	public class HintPathToggle : SimpleFeatureToggle
	{
		private static readonly Lazy<HintPathToggle> Lazy =
		new Lazy<HintPathToggle>(() => new HintPathToggle());

		private static bool _enabled = Instance.FeatureEnabled;

		public static HintPathToggle Instance => Lazy.Value;

		public static bool Enabled
		{
			get { return _enabled; }
			set { _enabled = value; }
		}
	}
}
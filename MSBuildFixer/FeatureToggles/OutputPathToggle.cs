using System;
using System.Dynamic;
using FeatureToggle.Toggles;

namespace MSBuildFixer.SampleFeatureToggles
{
	public class OutputPathToggle : SimpleFeatureToggle
	{
		private static readonly Lazy<OutputPathToggle> Lazy =
		new Lazy<OutputPathToggle>(() => new OutputPathToggle());

		public static OutputPathToggle Instance => Lazy.Value;

		public static bool Enabled => Instance.FeatureEnabled;
	}
}
using System;
using System.Dynamic;
using FeatureToggle.Toggles;

namespace MSBuildFixer.SampleFeatureToggles
{
	public class UseRelativePathing : SimpleFeatureToggle
	{
		private static readonly Lazy<UseRelativePathing> Lazy =
		new Lazy<UseRelativePathing>(() => new UseRelativePathing());

		public static UseRelativePathing Instance => Lazy.Value;

		public static bool Enabled => Instance.FeatureEnabled;
	}
}
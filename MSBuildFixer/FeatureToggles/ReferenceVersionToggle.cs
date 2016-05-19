using System;
using FeatureToggle.Core;
using FeatureToggle.Toggles;

namespace MSBuildFixer.SampleFeatureToggles
{
	public class ReferenceVersionToggle : SimpleFeatureToggle
	{
		private static readonly Lazy<ReferenceVersionToggle> Lazy =
		new Lazy<ReferenceVersionToggle>(() => new ReferenceVersionToggle());

		public static ReferenceVersionToggle Instance => Lazy.Value;

		public static bool Enabled => Instance.FeatureEnabled;
	}
}
using System;
using System.Dynamic;
using FeatureToggle.Toggles;

namespace MSBuildFixer.SampleFeatureToggles
{
	public class CopyLocalToggle : SimpleFeatureToggle
	{
		private static readonly Lazy<CopyLocalToggle> Lazy =
		new Lazy<CopyLocalToggle>(() => new CopyLocalToggle());

		public static CopyLocalToggle Instance => Lazy.Value;

		public static bool Enabled => Instance.FeatureEnabled;
	}
}
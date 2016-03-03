using System;
using System.Dynamic;
using FeatureToggle.Toggles;

namespace MSBuildFixer.SampleFeatureToggles
{
	public class CopyToOutputDirectoryToggle : SimpleFeatureToggle
	{
		private static readonly Lazy<CopyToOutputDirectoryToggle> Lazy =
		new Lazy<CopyToOutputDirectoryToggle>(() => new CopyToOutputDirectoryToggle());

		public static CopyToOutputDirectoryToggle Instance => Lazy.Value;

		public static bool Enabled => Instance.FeatureEnabled;
	}
}
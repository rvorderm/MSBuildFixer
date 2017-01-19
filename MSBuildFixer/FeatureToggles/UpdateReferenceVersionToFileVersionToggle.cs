using FeatureToggle.Toggles;
using System;

namespace MSBuildFixer.SampleFeatureToggles
{
	public class UpdateReferenceVersionToFileVersionToggle : SimpleFeatureToggle
	{
		private static readonly Lazy<UpdateReferenceVersionToFileVersionToggle> Lazy =
		new Lazy<UpdateReferenceVersionToFileVersionToggle>(() => new UpdateReferenceVersionToFileVersionToggle());

		public static UpdateReferenceVersionToFileVersionToggle Instance => Lazy.Value;

		public static bool Enabled => Instance.FeatureEnabled;
	}
}
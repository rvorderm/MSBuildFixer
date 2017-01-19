using FeatureToggle.Toggles;
using System;

namespace MSBuildFixer.SampleFeatureToggles
{
	public class ColocateAssemblyInfoToggle : SimpleFeatureToggle
	{
		private static readonly Lazy<ColocateAssemblyInfoToggle> Lazy =
		new Lazy<ColocateAssemblyInfoToggle>(() => new ColocateAssemblyInfoToggle());

		public static ColocateAssemblyInfoToggle Instance => Lazy.Value;

		public static bool Enabled => Instance.FeatureEnabled;
	}
}
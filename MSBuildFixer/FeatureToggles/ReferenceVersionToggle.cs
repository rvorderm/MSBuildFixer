using System;
using static System.Configuration.ConfigurationManager;

namespace MSBuildFixer.SampleFeatureToggles
{
	public class ReferenceVersionToggle
	{
		private static readonly Lazy<ReferenceVersionToggle> Lazy =
		new Lazy<ReferenceVersionToggle>(Create);

		public static ReferenceVersionToggle Instance => Lazy.Value;

		public static ReferenceVersionToggle Create()
		{
			string appSetting = AppSettings["ReferenceVersionType"];
			RefereneVersionType result;
			var toggle = new ReferenceVersionToggle
			{
				Type = Enum.TryParse(appSetting, true, out result) ? result : RefereneVersionType.None
			};
			return toggle;
		}

		public RefereneVersionType Type { get; set; }

		public bool Enabled => Type != RefereneVersionType.None;

		
	}

	public enum RefereneVersionType
	{
		None,
		HintPath,
		Config
	}
}
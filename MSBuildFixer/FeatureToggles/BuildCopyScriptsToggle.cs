using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeatureToggle.Toggles;

namespace MSBuildFixer.FeatureToggles
{
	public class BuildCopyScriptsToggle : SimpleFeatureToggle
	{
		private static readonly Lazy<BuildCopyScriptsToggle> Lazy =
		new Lazy<BuildCopyScriptsToggle>(() => new BuildCopyScriptsToggle());

		public static BuildCopyScriptsToggle Instance => Lazy.Value;

		public static bool Enabled => Instance.FeatureEnabled;
	}
}

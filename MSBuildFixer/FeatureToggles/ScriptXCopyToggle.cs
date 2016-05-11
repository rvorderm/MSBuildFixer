using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeatureToggle.Toggles;

namespace MSBuildFixer.FeatureToggles
{
	public class ScriptXCopyToggle : SimpleFeatureToggle
	{
		private static readonly Lazy<ScriptXCopyToggle> Lazy =
		new Lazy<ScriptXCopyToggle>(() => new ScriptXCopyToggle());

		public static ScriptXCopyToggle Instance => Lazy.Value;

		public static bool Enabled => Instance.FeatureEnabled;
	}
}

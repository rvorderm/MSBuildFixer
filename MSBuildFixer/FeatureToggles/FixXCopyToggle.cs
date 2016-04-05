using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeatureToggle.Toggles;

namespace MSBuildFixer.FeatureToggles
{
	public class FixXCopyToggle : SimpleFeatureToggle
	{
		private static readonly Lazy<FixXCopyToggle> Lazy =
		new Lazy<FixXCopyToggle>(() => new FixXCopyToggle());

		public static FixXCopyToggle Instance => Lazy.Value;

		public static bool Enabled => Instance.FeatureEnabled;
	}
}

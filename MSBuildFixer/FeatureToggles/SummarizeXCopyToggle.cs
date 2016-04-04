using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeatureToggle.Toggles;

namespace MSBuildFixer.FeatureToggles
{
	public class SummarizeXCopyToggle : SimpleFeatureToggle
	{
		private static readonly Lazy<SummarizeXCopyToggle> Lazy =
		new Lazy<SummarizeXCopyToggle>(() => new SummarizeXCopyToggle());

		public static SummarizeXCopyToggle Instance => Lazy.Value;

		public static bool Enabled => Instance.FeatureEnabled;
	}
}

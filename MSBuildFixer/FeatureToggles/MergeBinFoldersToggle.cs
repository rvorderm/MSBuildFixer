using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeatureToggle.Toggles;

namespace MSBuildFixer.FeatureToggles
{
	class MergeBinFoldersToggle : SimpleFeatureToggle
	{
		private static readonly Lazy<MergeBinFoldersToggle> Lazy =
		   new Lazy<MergeBinFoldersToggle>(() => new MergeBinFoldersToggle());

		public static MergeBinFoldersToggle Instance => Lazy.Value;

		public static bool Enabled => Instance.FeatureEnabled;
	}
}

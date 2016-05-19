using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FeatureToggle.Toggles;

namespace MSBuildFixer.FeatureToggles
{
	public class ProjectReferencesToggle : SimpleFeatureToggle
	{
		private static readonly Lazy<ProjectReferencesToggle> Lazy =
		new Lazy<ProjectReferencesToggle>(() => new ProjectReferencesToggle());

		public static ProjectReferencesToggle Instance => Lazy.Value;

		public static bool Enabled => Instance.FeatureEnabled;
	}
}

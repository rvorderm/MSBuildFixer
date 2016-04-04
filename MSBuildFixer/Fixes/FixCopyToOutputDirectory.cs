using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Construction;
using MSBuildFixer.SampleFeatureToggles;

namespace MSBuildFixer.Fixes
{
	public class FixCopyToOutputDirectory
	{
		public static void OnVisitProjectMetadata(object sender, EventArgs eventArgs)
		{
			var metadataElement = sender as ProjectMetadataElement;
			if (metadataElement == null) return;
			if (!CopyToOutputDirectoryToggle.Enabled) return;
			if (!metadataElement.Name.Equals("CopyToOutputDirectory")) return;

			metadataElement.Value = "PreserveNewest";
		}
	}
}

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
	public class FixCopyLocal
	{
		public static void OnVisitMetadata(object sender, EventArgs eventArgs)
		{
			var projectMetadataElement = sender as ProjectMetadataElement;
			if (projectMetadataElement == null) return;
			if (!CopyLocalToggle.Enabled) return;
			if (!projectMetadataElement.Name.Equals("Private")) return;
			
			projectMetadataElement.Value = false.ToString();
		}

		public static void OnVisitProjectItem(object sender, EventArgs eventArgs)
		{
			var projectItemElement = sender as ProjectItemElement;
			if (projectItemElement == null) return;
			if (!CopyLocalToggle.Enabled) return;
			if (!projectItemElement.ItemType.Equals("Reference")) return;
			if (IsGacAssembly(projectItemElement)) return;
			if (HasPrivateMetadata(projectItemElement)) return;
			
			projectItemElement.AddMetadata("Private", false.ToString());
		}

		public static bool HasPrivateMetadata(ProjectItemElement projectItemElement)
		{
			return projectItemElement.Metadata.Any(x => x.Name.Equals("Private"));
		}

		public static bool IsGacAssembly(ProjectItemElement projectItemElement)
		{
			return projectItemElement.Include.StartsWith("System")
			       || projectItemElement.Include.StartsWith("Microsoft");
		}
	}
}

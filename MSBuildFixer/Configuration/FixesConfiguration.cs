using System.Collections.Generic;
using System.Configuration;

namespace MSBuildFixer.Configuration
{
	public class FixesConfiguration
	{
		public static FixesConfiguration Instance = (dynamic)ConfigurationManager.GetSection("FixesConfiguration");
		public IEnumerable<ProjectReferenceReplacement> ProjectReferenceReplacements { get; set; }
	}

	public class ProjectReferenceReplacement
	{
		public string Name { get; set; }
		public string Replacement { get; set; }
	}
}

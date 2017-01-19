using System.Collections.Generic;
using System.Configuration;

namespace MSBuildFixer.Configuration
{
	public class ReferencesConfiguration
	{
		public IEnumerable<Reference> References { get; set; }
		public static ReferencesConfiguration Instance = (dynamic)ConfigurationManager.GetSection("ReferencesConfiguration");
	}

	public class Reference
	{
		public string AssemblyName { get; set; }
		public string DesiredVersion { get; set; }
	}
}

using System.Collections.Generic;
using System.Configuration;

namespace MSBuildFixer.Configuration
{
	public class ReportsConfiguration
	{
		public string ReferenceRegex { get; set; }
		public bool ListUntrackedProjects { get; set; }
		private IEnumerable<TransitiveCheck> TransitivesChecks { get; set; }
		public static ReportsConfiguration Instance = (dynamic)ConfigurationManager.GetSection("ReportsConfiguration");
	}

	public class TransitiveCheck
	{
		public string Source { get; set; }
		public string Dependency { get; set; }
	}
}

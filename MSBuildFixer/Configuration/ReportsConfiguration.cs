using System.Configuration;

namespace MSBuildFixer.Configuration
{
	public class ReportsConfiguration
	{
		public string ReferenceRegex { get; set; }
		public bool ListUntrackedProjects { get; set; }
		public static ReportsConfiguration Instance = (dynamic)ConfigurationManager.GetSection("ReportsConfiguration");
	}
}

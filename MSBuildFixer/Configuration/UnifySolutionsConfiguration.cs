using System.Collections.Generic;
using System.Configuration;

namespace MSBuildFixer.Configuration
{

	public class UnifySolutionsConfiguration
	{
		public string FileName { get; set; }
		public bool Enabled { get; set; }
		public IEnumerable<ExcludedProjects> ExcludedProjects { get; set; }

		public static UnifySolutionsConfiguration Instance { get; set; } =
			(dynamic) ConfigurationManager.GetSection("unifySolutionsConfiguration");
	}

	public class ExcludedProjects
	{
		public string SolutionPath { get; set; }
		public string ProjectName { get; set; }
	}
}

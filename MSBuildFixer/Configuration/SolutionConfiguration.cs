using System.Collections.Generic;
using System.Configuration;

namespace MSBuildFixer.Configuration
{
	public class SolutionConfiguration
	{
		public IEnumerable<string> Solutions { get; set; }
		public static SolutionConfiguration Instance = (dynamic)ConfigurationManager.GetSection("SolutionConfiguration");
	}
}

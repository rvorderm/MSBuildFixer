using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace MSBuildFixer.Configuration
{
	public class ExclusionConfiguration
	{
		public IEnumerable<string> Projects { get; set; }


		private static ExclusionConfiguration _exclustionConfiguration;


		public static bool IsExcludedProject(string absolutePath)
		{
			string fileName = Path.GetFileName(absolutePath);
			if(string.IsNullOrEmpty(fileName)) throw new ArgumentException($"{nameof(absolutePath)} must be a valid file name");
			_exclustionConfiguration = (dynamic) ConfigurationManager.GetSection("exclusionConfiguration");
			return _exclustionConfiguration?.Projects?.Any(exclusion => fileName.Equals(exclusion)) ?? false;
		}
	}
}

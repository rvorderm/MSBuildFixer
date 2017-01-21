using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace MSBuildFixer.Configuration
{
	public class ReferencesConfiguration
	{
		public IEnumerable<Reference> References { get; set; }
		public static ReferencesConfiguration Instance = (dynamic)ConfigurationManager.GetSection("ReferencesConfiguration");

		private Dictionary<string, Reference> _references { get; set; }

		public Reference TryGetReference(string assemblyName)
		{
			if (_references == null)
			{
				_references = References.ToDictionary(x => x.AssemblyName);
			}
			Reference result;
			_references.TryGetValue(assemblyName, out result);
			return result;
		}
	}

	public class Reference
	{
		public string AssemblyName { get; set; }
		public string HintPathVersion { get; set; }
		public string IncludeVersion { get; set; }
		public string PackageVersion { get; set; }
	}
}

using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace MSBuildFixer.Configuration
{
	public class PackagesConfiguration
	{
		public IEnumerable<Package> Packages { get; set; }
		public static PackagesConfiguration Instance = (dynamic)ConfigurationManager.GetSection("PackagesConfiguration");

		private Dictionary<string, Package> _packages { get; set; }

		public Package TryGetPackage(string assemblyName)
		{
			if (_packages == null)
			{
				_packages = Packages.ToDictionary(x => x.PackageName);
			}
			Package result;
			_packages.TryGetValue(assemblyName, out result);
			return result;
		}
	}

	public class Package
	{
		public string PackageName { get; set; }
		public string Version { get; set; }
	}
}

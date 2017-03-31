using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace MSBuildFixer.Configuration
{
	public class FixesConfiguration
	{
		public static FixesConfiguration Instance = (dynamic)ConfigurationManager.GetSection("FixesConfiguration");
		public IEnumerable<ProjectReferenceReplacement> ProjectReferenceReplacements { get; set; }
		public bool FixProjectFileEncodings { get; set; }

		public IEnumerable<Property> Properties
		{
			get { return _properties; }
			set
			{
				_properties = value;
				_propertiesDictionary = value.ToDictionary(x => x.Name, x => x.Value);
			}
		}

		private Dictionary<string, string> _propertiesDictionary;
		private IEnumerable<Property> _properties;

		public bool TryGetProperty(string name, out string value)
		{
			if(_propertiesDictionary == null || _propertiesDictionary.Any()) _properties.ToDictionary(x => x.Name, x => x.Value);
			return _propertiesDictionary.TryGetValue(name, out value);
		}
	}

	public class ProjectReferenceReplacement
	{
		public string Name { get; set; }
		public string Replacement { get; set; }
	}

	public class Property
	{
		public string Name { get; set; }
		public string Value { get; set; }
	}
}

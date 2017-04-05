using MSBuildFixer.Fixes;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace MSBuildFixer.Configuration
{
	public class FixesConfiguration
	{
		public static FixesConfiguration Instance = (dynamic)ConfigurationManager.GetSection("FixesConfiguration");
		public IEnumerable<ProjectReferenceReplacement> ProjectReferenceReplacements { get; set; }
		public bool ProjectFileEncodings { get; set; }
		public bool ProjectReferences { get; set; }
		public ReferenceVersionType ReferenceVersionType { get; set; } = ReferenceVersionType.None;
		public bool ReferenceVersion => ReferenceVersionType != ReferenceVersionType.None;

		public IEnumerable<Property> Properties
		{
			get { return _properties; }
			set
			{
				_properties = value;
				_propertiesDictionary = value.ToDictionary(x => x.Name, x => x.Value);
			}
		}

		public CopyStyle CopyStyle { get; set; } = CopyStyle.DoNothing;

		private Dictionary<string, string> _propertiesDictionary;
		private IEnumerable<Property> _properties;

		public bool TryGetProperty(string name, out string value)
		{
			if(_propertiesDictionary == null || Properties.Any()) _propertiesDictionary = _properties.ToDictionary(x => x.Name, x => x.Value);
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

	public enum ReferenceVersionType
	{
		None,
		HintPath,
		Config,
	}
}

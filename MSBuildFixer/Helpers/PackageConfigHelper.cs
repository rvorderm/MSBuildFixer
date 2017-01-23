using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace MSBuildFixer.Helpers
{
	public class PackageConfigHelper
	{
		private readonly string _packageFilePath;
		private XmlDocument _xmlDocument { get; set; }
		private List<Action> Actions { get; set; } = new List<Action>();

		public PackageConfigHelper(string packageFilePath)
		{
			_packageFilePath = packageFilePath;
		}

		public PackageConfigHelper AddOrUpdate(string assemblyName, string version)
		{
			Actions.Add(()=>AddOrUpdate_Action(assemblyName, version));
			return this;
		}

		public PackageConfigHelper Update(string assemblyName, string version)
		{
			Actions.Add(()=>Update_Action(assemblyName, version));
			return this;
		}

		public void SavePackageFile()
		{
			if (!Actions.Any()) return;
			CreateAndLoadFile();
			foreach (Action action in Actions)
			{
				action.Invoke();
			}
			_xmlDocument.Save(_packageFilePath);
		}

		private XmlElement GetElement(string id)
		{
			XmlNodeList xmlNodeList = _xmlDocument.SelectNodes("//package");
			return xmlNodeList?.Cast<XmlElement>().FirstOrDefault(elem => elem.Attributes["id"].Value.Equals(id));
		}

		private XmlElement AddPackage(string id, string version)
		{
			XmlElement child = _xmlDocument.CreateElement("package");
			addAttribute(child, "id", id);
			addAttribute(child, "version", version);
			addAttribute(child, "targetFramework", "net461");
			return child;
		}

		private void addAttribute(XmlNode elem, string name, string version)
		{
			XmlAttribute xmlAttribute = _xmlDocument.CreateAttribute(name);
			xmlAttribute.Value = version;
			elem.Attributes?.Append(xmlAttribute);
		}

		private void CreateAndLoadFile()
		{
			if (!File.Exists(_packageFilePath))
			{
				File.WriteAllText(_packageFilePath, "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<packages>\r\n</packages>");
			}
			if (_xmlDocument == null)
			{
				_xmlDocument = new XmlDocument();
				_xmlDocument.Load(_packageFilePath);
			}
		}

		private void AddOrUpdate_Action(string assemblyName, string version)
		{
			XmlElement xmlElement = GetElement(assemblyName);
			if (xmlElement == null)
			{
				xmlElement = (XmlElement)_xmlDocument.SelectSingleNode("//packages");
				XmlElement child = AddPackage(assemblyName, version);
				xmlElement?.AppendChild(child);
			}
			else
			{
				xmlElement.Attributes["version"].Value = version;
			}
		}

		private void Update_Action(string assemblyName, string version)
		{
			XmlElement xmlElement = GetElement(assemblyName);
			if (xmlElement != null)
			{
				xmlElement.Attributes["version"].Value = version;
			}
		}
	}
}

using System.IO;
using System.Linq;
using System.Xml;

namespace MSBuildFixer.Helpers
{
	public class PackageConfigHelper
	{
		private readonly string _packageFilePath;
		private readonly XmlDocument _xmlDocument = new XmlDocument();

		public PackageConfigHelper(string packageFilePath)
		{
			_packageFilePath = packageFilePath;
			if (!File.Exists(packageFilePath))
			{
				File.WriteAllText(packageFilePath, "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<packages>\r\n</packages>");
			}
			_xmlDocument.Load(packageFilePath);
		}

		public PackageConfigHelper WithPackage(string assemblyName, string version)
		{
			XmlElement xmlElement = GetElement(_xmlDocument, assemblyName);
			if (xmlElement == null)
			{
				xmlElement = (XmlElement)_xmlDocument.SelectSingleNode("//packages");
				XmlElement child = AddPackage(_xmlDocument, assemblyName, version);
				xmlElement?.AppendChild(child);
			}
			else
			{
				xmlElement.Attributes["version"].Value = version;
			}
			return this;
		}

		private static XmlElement GetElement(XmlNode xmlDocument, string id)
		{
			XmlNodeList xmlNodeList = xmlDocument.SelectNodes("//package");
			return xmlNodeList?.Cast<XmlElement>().FirstOrDefault(elem => elem.Attributes["id"].Value.Equals(id));
		}

		private static XmlElement AddPackage(XmlDocument xmlDocument, string id, string version)
		{
			XmlElement child = xmlDocument.CreateElement("package");
			addAttribute(xmlDocument, child, "id", id);
			addAttribute(xmlDocument, child, "version", version);
			addAttribute(xmlDocument, child, "targetFramework", "net461");
			return child;
		}

		private static void addAttribute(XmlDocument xmlDocument, XmlNode elem, string name, string version)
		{
			XmlAttribute xmlAttribute = xmlDocument.CreateAttribute(name);
			xmlAttribute.Value = version;
			elem.Attributes?.Append(xmlAttribute);
		}

		public void SavePackageFile()
		{
			_xmlDocument.Save(_packageFilePath);
		}
	}
}

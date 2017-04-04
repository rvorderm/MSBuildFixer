using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;

namespace MSBuildFixer.Helpers
{
	public interface IPackageConfigHelper
	{
		PackageConfigHelper AddPackage(string assemblyName, string version, string targetFramework = null);
		PackageConfigHelper Update(string regex, string version, string targetFramework = null);
		void SavePackageFile(XmlDocument xmlDocument);
		XmlDocument GetPackageDocument();
	}

	public class PackageConfigHelper : IPackageConfigHelper
	{
		private readonly string _packageFilePath;

		public PackageConfigHelper(string packageFilePath)
		{
			_packageFilePath = packageFilePath;
		}

		private List<Action<XmlDocument>> Actions { get; } = new List<Action<XmlDocument>>();

		public PackageConfigHelper AddPackage(string assemblyName, string version, string targetFramework = null)
		{
			Actions.Add(xmlDocument => AddPackage_Action(xmlDocument, assemblyName, version, targetFramework));
			return this;
		}

		public PackageConfigHelper Update(string regex, string version, string targetFramework = null)
		{
			Actions.Add(xmlDocument => Update_Action(xmlDocument, new Regex(regex), version, targetFramework));
			return this;
		}

		public void SavePackageFile(XmlDocument xmlDocument)
		{
			if (xmlDocument == null) return;
			if (xmlDocument.InnerText.Equals(File.ReadAllText(_packageFilePath))) return;
			xmlDocument.Save(_packageFilePath);
		}

		public XmlDocument GetPackageDocument()
		{
			if (!Actions.Any()) return null;
			XmlDocument xmlDocument = GetXmlDocument();
			foreach (Action<XmlDocument> action in Actions)
				action.Invoke(xmlDocument);
			return xmlDocument;
		}

		private static IEnumerable<XmlElement> GetElements(XmlNode xmlDocument, Regex regex)
		{
			XmlNodeList xmlNodeList = xmlDocument.SelectNodes("//package");
			return xmlNodeList?.Cast<XmlElement>().Where(elem => regex.IsMatch(elem.Attributes["id"].Value));
		}

		private static void AddPackage_Action(XmlDocument xmlDocument, string id, string version, string targetFramework)
		{
			XmlElement child = xmlDocument.CreateElement("package");
			AddAttribute(child, "id", id);
			AddAttribute(child, "version", version);
			AddAttribute(child, "targetFramework", targetFramework);
			var xmlElement = (XmlElement) xmlDocument.SelectSingleNode("//packages");
			xmlElement?.AppendChild(child);
		}

		private static void AddAttribute(XmlNode elem, string name, string version)
		{
			XmlAttribute xmlAttribute = elem.OwnerDocument?.CreateAttribute(name);
			if (xmlAttribute == null) return;
			xmlAttribute.Value = version;
			elem.Attributes?.Append(xmlAttribute);
		}

		private XmlDocument GetXmlDocument()
		{
			var xmlDocument = new XmlDocument();
			if (!File.Exists(_packageFilePath))
				xmlDocument.LoadXml("<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<packages>\r\n</packages>");
			else
				xmlDocument.Load(_packageFilePath);
			return xmlDocument;
		}

		private static void Update_Action(XmlNode xmlDocument, Regex regex, string version, string targetFramework)
		{
			IEnumerable<XmlElement> xmlElements = GetElements(xmlDocument, regex);
			foreach (XmlElement xmlElement in xmlElements)
				UpdatePackageAttributes(version, targetFramework, xmlElement);
		}

		private static void UpdatePackageAttributes(string version, string targetFramework, XmlElement xmlElement)
		{
			if (!string.IsNullOrEmpty(version)) xmlElement.Attributes["version"].Value = version;
			if (!string.IsNullOrEmpty(targetFramework)) xmlElement.Attributes["targetFramework"].Value = targetFramework;
		}
	}
}
using Microsoft.Build.Construction;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace MSBuildFixer.Fixes
{
	class FixAPICore : IFix
	{
		private readonly HashSet<string> _projectPaths = new HashSet<string>();
		private const string Version = "1.0.1.103";
		private static readonly string Include = $"Api.Core, Version={Version}, Culture=neutral, PublicKeyToken=b8761d376eb50bae, processorArchitecture=MSIL";
		private static readonly string HintPath = $@"..\..\packages\Public.API.Core.{Version}\lib\net461\Api.Core.dll";

		public void AttachTo(SolutionWalker walker)
		{
			walker.OnAfterVisitSolution += Walker_OnAfterVisitSolution;
			walker.OnVisitProjectRootItem += replaceAPI;
			walker.OnVisitProjectRootItem += replaceNewtonSoft;
			walker.OnVisitProjectItem += Walker_OnVisitProjectItem;
		}

		private void Walker_OnVisitProjectItem(ProjectItemElement projectItemElement)
		{
//			if (!projectItemElement.ItemType.Equals("ProjectReference")) return;
			if (!projectItemElement.Include.Contains("Api.Core.csproj")) return;

			projectItemElement.Parent.RemoveChild(projectItemElement);
			ProjectItemElement itemElement = projectItemElement.ContainingProject.AddItem("Reference", Include);
			AddOrUpdateReference(itemElement);
		}

		private void Walker_OnAfterVisitSolution(SolutionFile solutionFile)
		{
			foreach (string projectPath in _projectPaths)
			{
				string projectDirectory = Path.GetDirectoryName(projectPath);
				string packageFilePath = Path.Combine(projectDirectory, "packages.config");


				var xmlDocument = new XmlDocument();
				if (!File.Exists(packageFilePath))
				{
					File.WriteAllText(packageFilePath, "<?xml version=\"1.0\" encoding=\"utf-8\"?>\r\n<packages>\r\n</packages>");
				}

				xmlDocument.Load(packageFilePath);
				AddOrUpdateElement(xmlDocument, "Public.API.Core", Version);
				AddOrUpdateElement(xmlDocument, "Newtonsoft.Json", "8.0.3");
				xmlDocument.Save(packageFilePath);
			}
		}

		private static void AddOrUpdateElement(XmlDocument xmlDocument, string name, string version)
		{
			XmlElement xmlElement = GetElement(xmlDocument, name);
			if (xmlElement == null)
			{
				xmlElement = (XmlElement)xmlDocument.SelectSingleNode("//packages");
				var child = AddPackage(xmlDocument, name, version);
				xmlElement.AppendChild(child);
			}
			else
			{
				xmlElement.Attributes["version"].Value = version;
			}
		}

		private static XmlElement AddPackage(XmlDocument xmlDocument, string id, string version)
		{
			var child = xmlDocument.CreateElement("package");
			addAttribute(xmlDocument, child, "id", id);
			addAttribute(xmlDocument, child, "version", version);
			addAttribute(xmlDocument, child, "targetFramework", "net461");
			return child;
		}

		private static void addAttribute(XmlDocument xmlDocument, XmlElement elem, string name, string version)
		{
			XmlAttribute xmlAttribute = xmlDocument.CreateAttribute(name);
			xmlAttribute.Value = version;
			elem.Attributes.Append(xmlAttribute);
		}

		private static XmlElement GetElement(XmlDocument xmlDocument, string id)
		{
			XmlNodeList xmlNodeList = xmlDocument.SelectNodes("//package");
			if (xmlNodeList == null) return null;
			foreach (XmlElement elem in xmlNodeList)
			{
				if (elem.Attributes["id"].Value.Equals(id))
				{
					return elem;
				}
			}
			return null;
		}

		private void replaceNewtonSoft(ProjectRootElement projectRootElement)
		{
			List<ProjectItemElement> oldRef = projectRootElement.Items.Where(x => x.Include.Contains("Newtonsoft.Json")).ToList();
			if (!oldRef.Any()) return;
			_projectPaths.Add(projectRootElement.FullPath);
			foreach (ProjectItemElement projectItemElement in oldRef)
			{
				projectItemElement.Include =
					"Newtonsoft.Json, Version=8.0.0.0, Culture=neutral, PublicKeyToken=30ad4fe6b2a6aeed, processorArchitecture=MSIL";
				ProjectMetadataElement hintPath = projectItemElement.Metadata.FirstOrDefault(x => x.Name.Equals("HintPath"));
				if (hintPath != null) hintPath.Value = @"..\..\packages\Newtonsoft.Json.8.0.3\lib\net45\Newtonsoft.Json.dll";
			}
		}

		private void replaceAPI(ProjectRootElement projectRootElement)
		{
			List<ProjectItemElement> oldApiRef = projectRootElement.Items.Where(Predicate).ToList();
			if (!oldApiRef.Any()) return;
			_projectPaths.Add(projectRootElement.FullPath);
			foreach (ProjectItemElement projectItemElement in oldApiRef)
			{
				AddOrUpdateReference(projectItemElement);
			}
		}

		private static void AddOrUpdateReference(ProjectItemElement projectItemElement)
		{
			projectItemElement.Include = Include;
			AddOrUpdateMetaData(projectItemElement, "HintPath", HintPath);
			AddOrUpdateMetaData(projectItemElement, "SpecificVersion", false.ToString());
		}

		private static void AddOrUpdateMetaData(ProjectItemElement projectItemElement, string name, string value)
		{
			ProjectMetadataElement hintPath = projectItemElement.Metadata.FirstOrDefault(x => x.Name.Equals(name));
			if (hintPath == null)
				hintPath = projectItemElement.AddMetadata(name, value);
			else
			{
				hintPath.Value = value;
			}
		}

		private static bool Predicate(ProjectItemElement x)
		{
			return x.Include.StartsWith("Api.Core");
		}
	}
}

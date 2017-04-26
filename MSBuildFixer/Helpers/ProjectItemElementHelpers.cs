using Microsoft.Build.Construction;
using System;
using System.IO;
using System.Linq;
// ReSharper disable StringIndexOfIsCultureSpecific.1
// ReSharper disable StringIndexOfIsCultureSpecific.2

namespace MSBuildFixer.Helpers
{
	public static class ProjectItemElementHelpers
	{
		public static string GetAssemblyName(ProjectItemElement projectItemElement)
		{
			if (projectItemElement.ItemType.Equals("Reference"))
			{
				string include = projectItemElement.Include;
				int firstComma = include.IndexOf(",");
				return firstComma == -1 ? include : include.Substring(0, firstComma);
			}
			if (projectItemElement.ItemType.Equals("ProjectReference"))
			{
				string value = projectItemElement.Metadata.FirstOrDefault(x=>x.Name.Equals("Name"))?.Value;
				if (string.IsNullOrEmpty(value))
				{
					//In this case there wasn't a metadata so lets assume the filename is the assembly name....
					value = Path.GetFileNameWithoutExtension(projectItemElement.Include);
				}
				return value;
			}
			return projectItemElement.Include;
		}

		public static bool IsStrongName(string include)
		{
			return include.Contains(",");
		}

		public static string GetIncludeVersion(string include)
		{
			int firstComma = include.IndexOf(",");
			int secondComma = include.IndexOf(",", firstComma+1);
			int version = include.IndexOf("Version=");
			int versionLength = "Version=".Length;
			//Console.Out.Write($"{firstComma}, {secondComma}, {version}, {versionLength}");
			return firstComma == -1 ? null : include.Substring(version+versionLength, secondComma-(versionLength+version));
		}

		public static string GetHintPathVersion(ProjectItemElement projectItemElement)
		{
			ProjectMetadataElement hintPath = GetHintPath(projectItemElement);
			if (hintPath?.Value == null) return null;

			string assemblyName = GetAssemblyName(projectItemElement);
			string hintPathValue = hintPath.Value;
			return GetHintPathVersion(hintPathValue, assemblyName);
		}

		public static string GetHintPathVersion(string hintPathValue, string assemblyName)
		{
			int assemblyNameLocation = hintPathValue.IndexOf(assemblyName, StringComparison.OrdinalIgnoreCase);
			int firstDot = hintPathValue.IndexOf(".", assemblyNameLocation + assemblyName.Length);
			int slash = hintPathValue.IndexOf(@"\", firstDot + 1);
//			Console.Out.Write($"{assemblyNameLocation}, {firstDot}, {slash}");
			if (assemblyNameLocation != -1 && slash > firstDot && firstDot > assemblyNameLocation)
				return hintPathValue.Substring(firstDot + 1, slash - firstDot - 1);
			assemblyNameLocation = hintPathValue.IndexOf("packages\\");
			slash = hintPathValue.IndexOf(@"\", assemblyNameLocation);
			firstDot = hintPathValue.IndexOf(".", assemblyNameLocation);
			if (firstDot > slash) return null;
			slash = hintPathValue.IndexOf(@"\", firstDot);
			if(slash > firstDot && firstDot != -1)
				return hintPathValue.Substring(firstDot + 1, slash - firstDot - 1);
			return null;
		}

		public static ProjectMetadataElement GetHintPath(ProjectItemElement projectItemElement)
		{
			return GetMetadataElement(projectItemElement, "HintPath");
		}

		public static ProjectMetadataElement GetPrivate(ProjectItemElement projectItemElement)
		{
			return GetMetadataElement(projectItemElement, "Private");
		}

		public static ProjectMetadataElement GetMetadataElement(ProjectItemElement projectItemElement, string name)
		{
			return projectItemElement.Metadata.FirstOrDefault(x => x.Name.Equals(name));
		}

		public static string GetFileName(string solutionDirectory, string projectFullPath, string hintPathValue)
		{
			if (hintPathValue.Contains("$(SolutionDir)"))
			{
				return hintPathValue.Replace("$(SolutionDir)", solutionDirectory);
			}
			if(Path.IsPathRooted(hintPathValue)) return hintPathValue;

			string directoryName = Path.GetDirectoryName(projectFullPath);
			return directoryName == null ? hintPathValue : Path.Combine(directoryName, hintPathValue);
		}

		public static void AddOrUpdateMetaData(ProjectItemElement projectItemElement, string name, string value)
		{
			ProjectMetadataElement metadataElement = projectItemElement.Metadata.FirstOrDefault(x => x.Name.Equals(name));
			if (metadataElement == null)
				projectItemElement.AddMetadata(name, value);
			else
			{
				metadataElement.Value = value;
			}
		}

		public static void AddOrUpdateReference(ProjectItemElement projectItemElement, string include, string hintPath)
		{
			projectItemElement.Include = include;
			AddOrUpdateMetaData(projectItemElement, "HintPath", hintPath);
			AddOrUpdateMetaData(projectItemElement, "SpecificVersion", false.ToString());
		}
	}
}

using Microsoft.Build.Construction;
using System.IO;
using System.Linq;
// ReSharper disable StringIndexOfIsCultureSpecific.1
// ReSharper disable StringIndexOfIsCultureSpecific.2

namespace MSBuildFixer.Helpers
{
	public static class ProjectItemElementHelpers
	{
		public static string GetAssemblyName(string include)
		{
			int firstComma = include.IndexOf(",");
			return firstComma == -1 ? include : include.Substring(0,firstComma);
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

			string assemblyName = GetAssemblyName(projectItemElement.Include);
			int assemblyNameLocation = hintPath.Value.IndexOf(assemblyName);
			int firstDot = hintPath.Value.IndexOf(".", assemblyNameLocation+assemblyName.Length);
			int slash = hintPath.Value.IndexOf(@"\", firstDot+1);
			if (assemblyNameLocation == -1 || firstDot == -1 || slash == -1) return null;
//			Console.Out.Write($"{assemblyNameLocation}, {firstDot}, {slash}");
			return assemblyNameLocation == -1 ? null : hintPath.Value.Substring(firstDot+1, slash-firstDot-1);
		}

		public static ProjectMetadataElement GetHintPath(ProjectItemElement projectItemElement)
		{
			return projectItemElement.Metadata.FirstOrDefault(x=>x.Name.Equals("HintPath"));
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
			ProjectMetadataElement hintPath = projectItemElement.Metadata.FirstOrDefault(x => x.Name.Equals(name));
			if (hintPath == null)
				projectItemElement.AddMetadata(name, value);
			else
			{
				hintPath.Value = value;
			}
		}

		public static void AddOrUpdateReference(ProjectItemElement projectItemElement, string include, string hintPath)
		{
			projectItemElement.Include = include;
			ProjectItemElementHelpers.AddOrUpdateMetaData(projectItemElement, "HintPath", hintPath);
			ProjectItemElementHelpers.AddOrUpdateMetaData(projectItemElement, "SpecificVersion", false.ToString());
		}
	}
}

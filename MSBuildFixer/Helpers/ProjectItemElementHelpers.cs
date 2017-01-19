﻿using System.IO;

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

		public static string GetVersion(string include)
		{
			int firstComma = include.IndexOf(",");
			int secondComma = include.IndexOf(",", firstComma+1);
			int version = include.IndexOf("Version=");
			int versionLength = "Version=".Length;
			//Console.Out.Write($"{firstComma}, {secondComma}, {version}, {versionLength}");
			return firstComma == -1 ? null : include.Substring(version+versionLength, secondComma-(versionLength+version));
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
	}
}

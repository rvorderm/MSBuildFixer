namespace MSBuildFixer.Helpers
{
	public static class ProjectItemElementHelpers
	{
		public static string GetAssemblyName(string include)
		{
			int firstComma = include.IndexOf(",");
			return firstComma == -1 ? include : include.Substring(0,firstComma);
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
	}
}

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MSBuildFixerTests
{
	[TestClass]
	public class Tools
	{
		[TestClass]
		public class FixXCopyTools
		{
			[TestMethod]
			public void GenerateScriptFromFolders()
			{
				var solutionDir = @"C:\Repos\Default\Foundation\";
				var destinations = new List<string> {".out", ".pkg"};
				var source = @"bin\Debug";

				new ScriptBuilder(solutionDir, source, destinations).BuildScripts();
			}
		}
	}
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace MSBuildFixerTests
{
	public class ScriptBuilder
	{
		private readonly string _solutionDir;
		private readonly string _target;
		private readonly string _libraryPath = null;
		private readonly IEnumerable<string> _destinations;

		public ScriptBuilder(string solutionDir, string target, IEnumerable<string> destinations, string libPath = null)
		{
			_solutionDir = solutionDir;
			_target = target;
			_destinations = destinations;
			if (libPath != null)
			{
				_libraryPath = Path.Combine(solutionDir, libPath);
			}
		}
		private class Comparer : IEqualityComparer<string>
		{
			public bool Equals(string x, string y)
			{
				return Path.GetFileName(x).Equals(Path.GetFileName(y));
			}

			public int GetHashCode(string obj)
			{
				return obj.GetHashCode();
			}
		}

		public void BuildScripts()
		{
			var _sourceFolder = Path.Combine(_solutionDir, _target);
			var fullDestinations = _destinations.Select(x => Path.Combine(_solutionDir, x));

			var sourceFiles =
				Directory.EnumerateFiles(_sourceFolder, "*", SearchOption.AllDirectories).GroupBy(Path.GetFileName).ToDictionary(x=>x.Key, x=>x.First());

			Dictionary<string, List<string>> libraryFiles = null;
			if (!string.IsNullOrEmpty(_libraryPath))
			{
				libraryFiles = Directory.EnumerateFiles(_libraryPath, "*", SearchOption.AllDirectories)
					.GroupBy(Path.GetFileName)
					.ToDictionary(x=>x.Key, x => x.ToList());
			}

			var allFiles = Directory.EnumerateFiles(_solutionDir, "*", SearchOption.AllDirectories)
				.Where(x => !x.Contains("bin\\Debug") && !InDestination(x, fullDestinations))
				.GroupBy(Path.GetFileName, x => x)
				.ToDictionary(x => x.Key, x => x.ToList());

			foreach (var destination in fullDestinations)
			{
				var stringBuilder = BuildScript(destination, sourceFiles, libraryFiles, allFiles);
				var scriptName = GetScriptName(destination);
				WriteScript(scriptName, stringBuilder);
			}
		}

		private static void WriteScript(string scriptName, StringBuilder stringBuilder)
		{
			if (File.Exists(scriptName)) File.Create(scriptName).Close();
			File.WriteAllText(scriptName, stringBuilder.ToString());
		}

		private string GetScriptName(string destination)
		{
			var scriptName = Path.GetFileName(destination);
			var newFileName = $"CopyTo_{scriptName}.bat";
			var copyFile = Path.Combine(_solutionDir, newFileName);
			return copyFile;
		}

		private StringBuilder BuildScript(string destination, IReadOnlyDictionary<string, string> sourceFiles, Dictionary<string, List<string>> libraryFiles, Dictionary<string, List<string>> allFiles)
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(@"set Configuration=Debug");
			stringBuilder.AppendLine(@"set _solutionDir=%CD%");
			stringBuilder.AppendLine(@"set TargetDir=%_solutionDir%\bin\%Configuration%");
			stringBuilder.AppendLine();

			var destFiles = Directory.EnumerateFiles(destination, "*", SearchOption.AllDirectories).OrderBy(Path.GetFileName);

			AddFilesToScript(destFiles, sourceFiles, libraryFiles, allFiles, stringBuilder);
			return stringBuilder;
		}

		private void AddFilesToScript(IEnumerable<string> destFiles, 
			IReadOnlyDictionary<string, string> sourceFiles, 
			Dictionary<string, List<string>> libraryFiles, 
			IReadOnlyDictionary<string, List<string>> allFiles,
			StringBuilder stringBuilder)
		{
			foreach (var destFile in destFiles)
			{
				var sourceFilename = GetSourceFile(destFile, sourceFiles, libraryFiles, allFiles);
				var xcopy = BuildXCopy(_target, sourceFilename, destFile);
				stringBuilder.AppendLine(xcopy);
			}
		}

		private string GetSourceFile(string destinationFile, 
			IReadOnlyDictionary<string, string> targetFiles, 
			Dictionary<string, List<string>> libraryFiles, 
			IReadOnlyDictionary<string, List<string>> allFiles)
		{
			var fileName = Path.GetFileName(destinationFile);
			string sourceFile;
			var inSource = targetFiles.TryGetValue(fileName, out sourceFile);
			if (!inSource)
			{
				List<string> allFrom;
				if (libraryFiles != null)
				{
					if (libraryFiles.TryGetValue(fileName, out allFrom))
					{
						sourceFile = allFrom.FirstOrDefault();
					}
				}

				if (string.IsNullOrEmpty(sourceFile))
				{
					allFiles.TryGetValue(fileName, out allFrom);
					sourceFile = allFrom.FirstOrDefault();
				}
			}

			if (string.IsNullOrEmpty(sourceFile)) throw new FileNotFoundException(fileName);
			return sourceFile;
		}

		private string BuildXCopy(string targetFolder, string sourceFile, string destinationFile)
		{
			sourceFile = sourceFile.Replace(_solutionDir, string.Empty).Replace(targetFolder, "%TargetDir%");
			if (sourceFile.StartsWith("\\")) sourceFile = sourceFile.Substring(1);
			var to = destinationFile.Replace(_solutionDir, "%solutionDir%").Replace(Path.GetFileName(destinationFile), "*.*");
			var xcopy = $"xcopy /Y {sourceFile} {to}";
			return xcopy;
		}

		private bool InDestination(string s, IEnumerable<string> destinations)
		{
			foreach (var destination in destinations)
			{
				if (s.StartsWith(destination)) return true;
			}
			return false;
		}
	}
}
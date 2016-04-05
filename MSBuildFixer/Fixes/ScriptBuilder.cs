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
		private readonly IEnumerable<string> _destinations;

		public ScriptBuilder(string solutionDir, string target, IEnumerable<string> destinations)
		{
			_solutionDir = solutionDir;
			_target = target;
			_destinations = destinations;
		}

		public void BuildScripts()
		{
			var _sourceFolder = Path.Combine(_solutionDir, _target);
			var fullDestinations = _destinations.Select(x => Path.Combine(_solutionDir, x));

			var sourceFiles =
				Directory.EnumerateFiles(_sourceFolder, "*", SearchOption.AllDirectories).ToDictionary(Path.GetFileName, x => x);

			var allFiles = Directory.EnumerateFiles(_solutionDir, "*", SearchOption.AllDirectories)
				.Where(x => !x.Contains("bin\\Debug") && !InDestination(x, fullDestinations))
				.GroupBy(Path.GetFileName, x => x)
				.ToDictionary(x => x.Key, x => x.ToList());

			foreach (var destination in fullDestinations)
			{
				var stringBuilder = BuildScript(destination, sourceFiles, allFiles);
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

		private StringBuilder BuildScript(string destination, Dictionary<string, string> sourceFiles, Dictionary<string, List<string>> allFiles)
		{
			var stringBuilder = new StringBuilder();
			stringBuilder.AppendLine(@"set Configuration=Debug");
			stringBuilder.AppendLine(@"set _solutionDir=%CD%");
			stringBuilder.AppendLine(@"set TargetDir=%_solutionDir%\bin\%Configuration%");
			stringBuilder.AppendLine();

			var destFiles = Directory.EnumerateFiles(destination, "*", SearchOption.AllDirectories).OrderBy(Path.GetFileName);

			AddFilesToScript(destFiles, sourceFiles, allFiles, stringBuilder);
			return stringBuilder;
		}

		private void AddFilesToScript(IEnumerable<string> destFiles, 
			IReadOnlyDictionary<string, string> sourceFiles, 
			IReadOnlyDictionary<string, List<string>> allFiles,
			StringBuilder stringBuilder)
		{
			foreach (var destFile in destFiles)
			{
				var sourceFilename = GetSourceFile(destFile, sourceFiles, allFiles);
				var xcopy = BuildXCopy(_target, sourceFilename, destFile);
				stringBuilder.AppendLine(xcopy);
			}
		}

		private string GetSourceFile(string destinationFile, 
			IReadOnlyDictionary<string, string> targetFiles, 
			IReadOnlyDictionary<string, List<string>> allFiles)
		{
			var fileName = Path.GetFileName(destinationFile);
			string sourceFile;
			var inSource = targetFiles.TryGetValue(fileName, out sourceFile);
			if (!inSource)
			{
				List<string> allFrom;
				allFiles.TryGetValue(fileName, out allFrom);
				sourceFile = allFrom.First();
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
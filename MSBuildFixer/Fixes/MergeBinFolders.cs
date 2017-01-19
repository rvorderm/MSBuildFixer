using System;
using System.IO;

namespace MSBuildFixer.Fixes
{
	public class MergeBinFolders : IFix
	{
		private string _solutionFile;

		public void OnOpenSolution(string solutionPath)
		{
			_solutionFile = solutionPath;
		}

		public void OnOpenProjectFile(string projectFile)
		{
			if (string.IsNullOrEmpty(_solutionFile)) return;
			if (string.IsNullOrEmpty(projectFile)) return;
			var root = Path.GetDirectoryName(_solutionFile);
			var project = Path.GetDirectoryName(projectFile);

			MergeProjectIntoRoot(project, root);
		}

		public static void MergeProjectIntoRoot(string project, string root)
		{
			if (project == null) return;
			if (root == null) return;

			var binFolders = Directory.EnumerateDirectories(project, "bin");
			var destination = new DirectoryInfo(Path.Combine(root, "bin"));
			foreach (var binFolder in binFolders)
			{
				var source = new DirectoryInfo(binFolder);
				CopyAll(source, destination);
				Directory.Delete(source.FullName, true);
			}
		}

		public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
		{
			if (String.Equals(source.FullName, target.FullName, StringComparison.CurrentCultureIgnoreCase))
			{
				return;
			}

			// Check if the target directory exists, if not, create it.
			if (Directory.Exists(target.FullName) == false)
			{
				Directory.CreateDirectory(target.FullName);
			}

			// Copy each file into it's new directory.
			foreach (var fi in source.GetFiles())
			{
				fi.CopyTo(Path.Combine(target.ToString(), fi.Name), true);
			}

			// Copy each subdirectory using recursion.
			foreach (var diSourceSubDir in source.GetDirectories())
			{
				var nextTargetSubDir =
					target.CreateSubdirectory(diSourceSubDir.Name);
				CopyAll(diSourceSubDir, nextTargetSubDir);
			}
		}

		public void AttachTo(SolutionWalker walker)
		{
			walker.OnOpenSolution += OnOpenSolution;
			walker.OnOpenProjectFile += OnOpenProjectFile;
		}
	}
}

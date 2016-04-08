using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MSBuildFixer.Fixes
{
	public class MergeBinFolders
	{
		private string _solutionFile;

		public void OnOpenSolution(object sender, EventArgs eventArgs)
		{
			_solutionFile = sender as string;
		}

		public void OnOpenProjectFile(object sender, EventArgs eventArgs)
		{
			if (string.IsNullOrEmpty(_solutionFile)) return;
			var projectFile = sender as string;
			if (projectFile == null) return;
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
			if (string.Equals(source.FullName, target.FullName, StringComparison.CurrentCultureIgnoreCase))
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
	}
}

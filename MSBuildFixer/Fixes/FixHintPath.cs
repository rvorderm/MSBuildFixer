using Microsoft.Build.Construction;
using MSBuildFixer.SampleFeatureToggles;
using System;
using System.IO;
using System.Linq;
using static System.Configuration.ConfigurationManager;

namespace MSBuildFixer.Fixes
{
	public class FixHintPath : IFix
	{
		private static string _libraryPath;
		private static string _solutionPath;

		public FixHintPath()
			: this(Path.GetDirectoryName(AppSettings["SolutionPath"]), AppSettings["LibraryFolder"])
		{
		}

		public FixHintPath(string solutionPath, string libraryDirectory)
		{
			if (string.IsNullOrEmpty(solutionPath)) throw new ArgumentException(solutionPath);
			if (string.IsNullOrEmpty(libraryDirectory)) throw new ArgumentException(nameof(libraryDirectory));
			_libraryPath = Path.Combine(solutionPath, libraryDirectory);
			_solutionPath = solutionPath;
			if (!Directory.Exists(_libraryPath)) throw new ArgumentException("The given library path does not exist");
		}

		public void OnVisitProjectItem(object sender, EventArgs evetArgs)
		{
			var projectItemElement = sender as ProjectItemElement;
			if (projectItemElement == null) return;
			if (!projectItemElement.ItemType.Equals("Reference")) return;
			var metadataCollection = projectItemElement.Metadata;
			if (metadataCollection.Any(x=>x.Name.Equals("HintPath"))) return;
			if (!HintPathToggle.Enabled) return;

			var fileName = Path.GetFileName(projectItemElement.Include.Split(' ').First());
			fileName = fileName.Substring(0, fileName.Length - 1) + ".dll";
			var libraryPath = Directory.EnumerateFiles(_libraryPath, fileName, SearchOption.AllDirectories).LastOrDefault();
			if (libraryPath != null)
			{
				projectItemElement.AddMetadata("HintPath", libraryPath.Replace(_solutionPath, @"$(SolutionDir)"));
			}
		}

		public void OnVisitMetadata(object sender, EventArgs eventArgs)
		{
			var projectMetadataElement = sender as ProjectMetadataElement;
			if (projectMetadataElement == null) return;
			if (!HintPathToggle.Enabled) return;
			if (!projectMetadataElement.Name.Equals("HintPath")) return;

			var fileName = Path.GetFileName(projectMetadataElement.Value);
			var libraryPath = Directory.EnumerateFiles(_libraryPath, fileName, SearchOption.AllDirectories).LastOrDefault();
			if (libraryPath != null)
			{
				projectMetadataElement.Value = HintPathToggle_UseRelative.Enabled 
					? MakeRelativePath(projectMetadataElement.ContainingProject.FullPath, libraryPath) 
					: libraryPath.Replace(_solutionPath, @"$(SolutionDir)");
			}
		}

		/// <summary>
		/// 2/20/2016 From
		/// http://stackoverflow.com/questions/275689/how-to-get-relative-path-from-absolute-path
		/// Creates a relative path from one file or folder to another.
		/// </summary>
		/// <param name="fromPath">Contains the directory that defines the start of the relative path.</param>
		/// <param name="toPath">Contains the path that defines the endpoint of the relative path.</param>
		/// <returns>The relative path from the start directory to the end path or <c>toPath</c> if the paths are not related.</returns>
		/// <exception cref="ArgumentNullException"></exception>
		/// <exception cref="UriFormatException"></exception>
		/// <exception cref="InvalidOperationException"></exception>
		public static string MakeRelativePath(string fromPath, string toPath)
		{
			if (String.IsNullOrEmpty(fromPath)) throw new ArgumentNullException("fromPath");
			if (String.IsNullOrEmpty(toPath)) throw new ArgumentNullException("toPath");

			var fromUri = new Uri(fromPath);
			var toUri = new Uri(toPath);

			if (fromUri.Scheme != toUri.Scheme) { return toPath; } // path can't be made relative.

			var relativeUri = fromUri.MakeRelativeUri(toUri);
			var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

			if (toUri.Scheme.Equals("file", StringComparison.InvariantCultureIgnoreCase))
			{
				relativePath = relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			}

			return relativePath;
		}

		public void AttachTo(SolutionWalker walker)
		{
			walker.OnVisitMetadata += OnVisitMetadata;
			walker.OnVisitProjectItem += OnVisitProjectItem;
		}
	}
}

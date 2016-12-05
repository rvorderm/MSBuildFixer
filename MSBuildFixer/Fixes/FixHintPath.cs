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
		private ILookup<string, string> _library;

		public ILookup<string, string> Library
		{
			get
			{
				if (!Directory.Exists(LibraryPath)) throw new ArgumentException("The given library path does not exist");
				if (Library == null)
				{
					Library =
						Directory.EnumerateFiles(Path.GetFullPath(LibraryPath), "*", SearchOption.AllDirectories)
							.ToLookup(Path.GetFileName);
				}
				return _library;
			}
			private set { _library = value; }
		}

		public string LibraryPath { get; set; } = AppSettings["LibraryFolder"];

		public string SolutionPath { get; set; }
		

		public void OnVisitProjectItem(object sender, EventArgs evetArgs)
		{
			if (string.IsNullOrEmpty(SolutionPath)) throw new ArgumentException(SolutionPath);
			if (string.IsNullOrEmpty(LibraryPath)) throw new ArgumentException(nameof(LibraryPath));
			var projectItemElement = sender as ProjectItemElement;
			if (projectItemElement == null) return;
			if (!projectItemElement.ItemType.Equals("Reference")) return;
			var metadataCollection = projectItemElement.Metadata;
			if (metadataCollection.Any(x=>x.Name.Equals("HintPath"))) return;
			if (!HintPathToggle.Enabled) return;

			string fileName = Path.GetFileName(projectItemElement.Include.Split(' ').First());
			fileName = fileName.Substring(0, fileName.Length - 1) + ".dll";
			
			if (!Library.Contains(fileName)) return;
			string libraryPath = Library[fileName].Last();
			projectItemElement.AddMetadata("HintPath", libraryPath.Replace(SolutionPath, @"$(SolutionDir)"));
		}

		public void OnVisitMetadata(object sender, EventArgs eventArgs)
		{
			if (string.IsNullOrEmpty(SolutionPath)) throw new ArgumentException(SolutionPath);
			if (string.IsNullOrEmpty(LibraryPath)) throw new ArgumentException(nameof(LibraryPath));
			var projectMetadataElement = sender as ProjectMetadataElement;
			if (projectMetadataElement == null) return;
			if (!HintPathToggle.Enabled) return;
			if (!projectMetadataElement.Name.Equals("HintPath")) return;

			string fileName = Path.GetFileName(projectMetadataElement.Value);
			string libraryPath = Directory.EnumerateFiles(LibraryPath, fileName, SearchOption.AllDirectories).LastOrDefault();
			if (libraryPath != null)
			{
				projectMetadataElement.Value = UseRelativePathing.Enabled 
					? MakeRelativePath(projectMetadataElement.ContainingProject.FullPath, libraryPath) 
					: libraryPath.Replace(SolutionPath, @"$(SolutionDir)");
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

		private void OnOpenSolution(object sender, EventArgs e)
		{
			SolutionPath = sender as string;
			if(SolutionPath == null) throw new NotImplementedException();
		}

		public void AttachTo(SolutionWalker walker)
		{
			walker.OnOpenSolution += OnOpenSolution;
			walker.OnVisitMetadata += OnVisitMetadata;
			walker.OnVisitProjectItem += OnVisitProjectItem;
		}
	}
}

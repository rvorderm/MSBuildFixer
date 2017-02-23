using Microsoft.Build.Construction;
using MSBuildFixer.SampleFeatureToggles;
using System;
using System.IO;
using System.Linq;
using MSBuildFixer.Helpers;
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
		

		public void OnvisitReference(ProjectItemElement projectItemElement)
		{
			if (string.IsNullOrEmpty(SolutionPath)) throw new ArgumentException(SolutionPath);
			if (string.IsNullOrEmpty(LibraryPath)) throw new ArgumentException(nameof(LibraryPath));
			var metadataCollection = projectItemElement.Metadata;
			if (metadataCollection.Any(x=>x.Name.Equals("HintPath"))) return;
			if (!HintPathToggle.Enabled) return;

			string fileName = Path.GetFileName(projectItemElement.Include.Split(' ').First());
			fileName = fileName.Substring(0, fileName.Length - 1) + ".dll";
			
			if (!Library.Contains(fileName)) return;
			string libraryPath = Library[fileName].Last();
			projectItemElement.AddMetadata("HintPath", libraryPath.Replace(SolutionPath, @"$(SolutionDir)"));
		}

		public void OnVisitMetadata(ProjectMetadataElement projectMetadataElement)
		{
			if (string.IsNullOrEmpty(SolutionPath)) throw new ArgumentException(SolutionPath);
			if (string.IsNullOrEmpty(LibraryPath)) throw new ArgumentException(nameof(LibraryPath));
			if (!HintPathToggle.Enabled) return;
			if (!projectMetadataElement.Name.Equals("HintPath")) return;

			string fileName = Path.GetFileName(projectMetadataElement.Value);
			string libraryPath = Directory.EnumerateFiles(LibraryPath, fileName, SearchOption.AllDirectories).LastOrDefault();
			if (libraryPath != null)
			{
				projectMetadataElement.Value = UseRelativePathing.Enabled 
					? PathHelpers.MakeRelativePath(projectMetadataElement.ContainingProject.FullPath, libraryPath) 
					: libraryPath.Replace(SolutionPath, @"$(SolutionDir)");
			}
		}

	    private void OnOpenSolution(string solutionPath)
		{
			SolutionPath = solutionPath;
			if(SolutionPath == null) throw new NotImplementedException();
		}

		public void AttachTo(SolutionWalker walker)
		{
			walker.OnOpenSolution += OnOpenSolution;
			walker.OnVisitMetadata += OnVisitMetadata;
			walker.OnVisitProjectItem_Reference += OnvisitReference;
		}
	}
}

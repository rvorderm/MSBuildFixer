using Microsoft.Build.Construction;
using MSBuildFixer.Helpers;
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
				if (_library != null) return _library;
				if (!Directory.Exists(LibraryPath)) throw new ArgumentException("The given library path does not exist");
				_library =
					Directory.EnumerateFiles(Path.GetFullPath(LibraryPath), "*", SearchOption.AllDirectories)
						.ToLookup(Path.GetFileName);
				return _library;
			}
			set { _library = value; }
		}

		private ILookup<string, string> _solutionLookup;

		public ILookup<string, string> SolutionLookup
		{
			get
			{
				if (_solutionLookup != null) return _solutionLookup;
				_solutionLookup =
					Directory.EnumerateFiles(Path.GetFullPath(SolutionPath), "*", SearchOption.AllDirectories)
						.ToLookup(Path.GetFileName);
				return _solutionLookup;
			}
			set { _solutionLookup = value; }
		}

		public string LibraryPath { get; set; } = AppSettings["LibraryFolder"];

		public string SolutionPath { get; set; }

		private string GetHintPath(string fileName)
		{
			string hintPath = null;
			if (Library.Contains(fileName))
			{
				hintPath = Library[fileName].Last();
			}
			else if (SolutionLookup.Contains(fileName))
			{
				hintPath = SolutionLookup[fileName].Last();
			}
			return hintPath?.Replace(SolutionPath, @"$(SolutionDir)");
		}

		public void OnVisitReference(ProjectItemElement projectItemElement)
		{
			if (ProjectItemElementHelpers.GetHintPath(projectItemElement) != null) return;

			string fileName = $"{ProjectItemElementHelpers.GetAssemblyName(projectItemElement)}.dll";

			string hintPath = GetHintPath(fileName);
			if (string.IsNullOrEmpty(hintPath)) return;
			ProjectItemElementHelpers.AddOrUpdateMetaData(projectItemElement, "HintPath", hintPath);
		}

		public void OnVisitMetadata(ProjectMetadataElement projectMetadataElement)
		{
			if (!projectMetadataElement.Name.Equals("HintPath")) return;

			string fileName = Path.GetFileName(projectMetadataElement.Value);
			string hintPath = GetHintPath(fileName);
			if (string.IsNullOrEmpty(hintPath)) return;
			projectMetadataElement.Value = hintPath;
		}

		private void OnOpenSolution(string solutionPath)
		{
			SolutionPath = solutionPath;
		}

		public void AttachTo(SolutionWalker walker)
		{
			walker.OnOpenSolution += OnOpenSolution;
			walker.OnVisitMetadata += OnVisitMetadata;
			walker.OnVisitProjectItem_Reference += OnVisitReference;
		}
	}
}

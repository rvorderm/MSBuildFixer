using Microsoft.Build.Construction;
using System.IO;
using MSBuildFixer.Helpers;

namespace MSBuildFixer.Fixes
{
	public class FixColocateAssemblyInfo : IFix
	{
		private string _sharedAssemblyInfoPath;
		private string _solutionDirectoryName;
		private string _currentProjectDirectoryPath;
		private string _currentSharedAssemblyInfoContents;

		public void AttachTo(SolutionWalker walker)
		{
			walker.OnVisitProjectRootItem += Walker_OnVisitProjectRootItem;
			walker.OnVisitProjectItem_Compile += OnVisitCompile;
			walker.OnOpenSolution += Walker_OnOpenSolution;
		}

		private void Walker_OnVisitProjectRootItem(ProjectRootElement rootElement)
		{
			_currentProjectDirectoryPath = Path.GetDirectoryName(rootElement.FullPath);
		}

		private void Walker_OnOpenSolution(string solutionPath)
		{
			_solutionDirectoryName = Path.GetDirectoryName(solutionPath);
			if (_solutionDirectoryName != null) _sharedAssemblyInfoPath = Path.Combine(_solutionDirectoryName, "AssemblyInfo.cs");
		}

		private void OnVisitCompile(ProjectItemElement projectItemElement)
		{
			if (!IsAssemblyInfo(projectItemElement)) return;
			string currentFilePath = Path.Combine(_currentProjectDirectoryPath, projectItemElement.Include);
			string destinationRelativePath = PathHelpers.MakeRelativePath(currentFilePath, _sharedAssemblyInfoPath);
			if (projectItemElement.Include.Equals(destinationRelativePath)) return;

			if(!File.Exists(_sharedAssemblyInfoPath)) File.Copy(currentFilePath, _sharedAssemblyInfoPath);
			if (!CompareFileAssemblyInfoFiles(currentFilePath)) return;
			File.Delete(currentFilePath);
			projectItemElement.Include = destinationRelativePath;
		}

		private bool CompareFileAssemblyInfoFiles(string currentFilePath)
		{
			string currentFileContents = File.ReadAllText(currentFilePath);
			return !currentFileContents.Equals(GetCurrentSharedAssemblyInfoContents());
		}

		private static bool IsAssemblyInfo(ProjectItemElement projectItemElement)
		{
			string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(projectItemElement.Include);
			return fileNameWithoutExtension != null && fileNameWithoutExtension.Equals("AssemblyInfo");
		}

		private string GetCurrentSharedAssemblyInfoContents()
		{
			if (!string.IsNullOrEmpty(_currentSharedAssemblyInfoContents)) return _currentSharedAssemblyInfoContents;
			if (File.Exists(_sharedAssemblyInfoPath))
			{
				_currentSharedAssemblyInfoContents = File.ReadAllText(_sharedAssemblyInfoPath);
			}
			return _currentSharedAssemblyInfoContents;
		}
	}
}

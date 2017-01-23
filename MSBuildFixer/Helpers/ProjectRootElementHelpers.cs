// ReSharper disable StringIndexOfIsCultureSpecific.1
// ReSharper disable StringIndexOfIsCultureSpecific.2

using Microsoft.Build.Construction;
using System.Linq;

namespace MSBuildFixer.Helpers
{
	public static class ProjectRootElementHelpers
	{
		public static string GetAssemblyName(ProjectRootElement rootElement)
		{
			return rootElement.Properties.FirstOrDefault(x=>x.Name.Equals("Assembly"))?.Value;
		}
	}
}

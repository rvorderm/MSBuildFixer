using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Build.Construction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSBuildFixer;
using MSBuildFixer.FeatureToggles;
using MSBuildFixer.Fixes;

namespace MSBuildFixerTests.Fixes
{
	[TestClass]
	public class FixProjectReferencesTests
	{
		[TestClass]
		public class OnVisitMetadataTests
		{
			[TestMethod]
			public void SetsValue()
			{
				TestSetup.SetToggleTo(ProjectReferencesToggle.Instance, true);

				var projectRootElement = TestSetup.GetTestProject();

				var projectItemElement = CheckForReference(projectRootElement);
				Assert.IsNotNull(projectItemElement);

				var fixCopyToOutputDirectory = new FixProjectRefences();
				fixCopyToOutputDirectory.VisitProjectItem(projectItemElement, EventArgs.Empty);
				Assert.IsNull(CheckForReference(projectRootElement));
			}

			private static ProjectItemElement CheckForReference(ProjectRootElement projectRootElement)
			{
				var reference = "Reference";
				return CheckFor(projectRootElement, reference);
			}

			private static ProjectItemElement CheckForProjectReference(ProjectRootElement projectRootElement)
			{
				var reference = "ProjectReference";
				return CheckFor(projectRootElement, reference);
			}

			private static ProjectItemElement CheckFor(ProjectRootElement projectRootElement, string reference)
			{
				return
					projectRootElement.Items.FirstOrDefault(x => x.ItemType.Equals(reference) && x.Include.Contains("Reporting.Model"));
			}
		}
	}
}

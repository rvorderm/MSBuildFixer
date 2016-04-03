using System.IO;
using System.Xml;
using FakeItEasy;
using FakeItEasy.ExtensionSyntax.Full;
using FeatureToggle.Core;
using Microsoft.Build.Construction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSBuildFixer.SampleFeatureToggles;
using MSBuildFixerTests.Properties;

namespace MSBuildFixerTests.Fixes
{
	[TestClass]
	public static class TestSetup
	{
		private static ProjectRootElement _projectRootElement;

		[AssemblyInitialize]
		public static void AssemblyInit(TestContext context)
		{
			var xmlReader = XmlReader.Create(new StringReader(Resources.Test));
			_projectRootElement = ProjectRootElement.Create(xmlReader);
		}

		public static ProjectRootElement GetTestProject()
		{
			return _projectRootElement.DeepClone();
		}

		public static void SetOutputPathToggleTo(bool value)
		{
			var booleanToggleValueProvider = A.Fake<IBooleanToggleValueProvider>();
			booleanToggleValueProvider.CallsTo(x => x.EvaluateBooleanToggleValue(OutputPathToggle.Instance)).Returns(value);
			OutputPathToggle.Instance.ToggleValueProvider = booleanToggleValueProvider;
		}
	}
}
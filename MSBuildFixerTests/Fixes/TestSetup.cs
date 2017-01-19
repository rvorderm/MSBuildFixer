using FakeItEasy;
using FeatureToggle.Core;
using FeatureToggle.Toggles;
using Microsoft.Build.Construction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSBuildFixer;
using MSBuildFixerTests.Properties;
using System;
using System.IO;
using System.Xml;

namespace MSBuildFixerTests.Fixes
{
	[TestClass]
	public static class TestSetup
	{
		private const string _solutionPath = @"..\..\TestData\Test.sln";
		public static readonly string SolutionPath = Path.Combine(Environment.CurrentDirectory, _solutionPath);
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

		public static void SetToggleTo(SimpleFeatureToggle toggle, bool value)
		{
			var booleanToggleValueProvider = A.Fake<IBooleanToggleValueProvider>();
			A.CallTo(booleanToggleValueProvider)
				.Where(call => call.Method.Name.Equals(nameof(booleanToggleValueProvider.EvaluateBooleanToggleValue))).
				WithReturnType<bool>()
				.Returns(value);
			//booleanToggleValueProvider.CallsTo(x => x.EvaluateBooleanToggleValue(toggle)).Returns(value);
			toggle.ToggleValueProvider = booleanToggleValueProvider;
		}

		public static SolutionWalker GetTestWalker()
		{
			return new SolutionWalker(SolutionPath);
		}
	}
}
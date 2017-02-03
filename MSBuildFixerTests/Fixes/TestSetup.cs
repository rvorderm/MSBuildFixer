using FakeItEasy;
using FeatureToggle.Core;
using FeatureToggle.Toggles;
using Microsoft.Build.Construction;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSBuildFixer;
using MSBuildFixerTests.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace MSBuildFixerTests.Fixes
{
	[TestClass]
	public static class TestSetup
	{
		private const string _solutionPath = @"..\..\MSBuildFixerTests\TestData\Test.sln";
		public static readonly string SolutionPath = Path.Combine(Environment.CurrentDirectory, _solutionPath);
		private static ProjectRootElement _projectRootElement;

		[AssemblyInitialize]
		public static void AssemblyInit(TestContext context)
		{
			var xmlReader = XmlReader.Create(new StringReader(Resources.Test));
			_projectRootElement = ProjectRootElement.Create(xmlReader);
		}

		public static SolutionFile GetTestSolution()
		{
			return SolutionFile.Parse(_solutionPath);
		}

		public static ProjectRootElement GetTestProject()
		{
			return _projectRootElement.DeepClone();
		}

		public static ProjectPropertyElement GetProperty(string name)
		{
			ProjectRootElement projectRootElement = TestSetup.GetTestProject();
			return projectRootElement.Properties.FirstOrDefault(x => x.Name.Equals(name));
		}

		public static void SetToggleTo(SimpleFeatureToggle toggle, bool value)
		{
			var booleanToggleValueProvider = A.Fake<IBooleanToggleValueProvider>();
			A.CallTo(booleanToggleValueProvider)
				.Where(call => call.Method.Name.Equals(nameof(booleanToggleValueProvider.EvaluateBooleanToggleValue))).
				WithReturnType<bool>()
				.Returns(value);
			toggle.ToggleValueProvider = booleanToggleValueProvider;
		}

		public static SolutionWalker GetTestWalker()
		{
			return new SolutionWalker(SolutionPath);
		}

		public static SolutionWalker BuildWalker<T>(Action<T> action = null) where T : IFix, new()
		{
			var fix = new T();
			return BuildAndAttach(action, fix);
		}

		public static SolutionWalker BuildWalker<T>(T fix, Action<T> action = null) where T : IFix
		{
			return BuildAndAttach(action, fix);
		}

		private static SolutionWalker BuildAndAttach<T>(Action<T> action, T fix) where T : IFix
		{
			SolutionWalker solutionWalker = GetTestWalker();
			fix.AttachTo(solutionWalker);
			action?.Invoke(fix);
			return solutionWalker;
		}

		public static void AssertPropertyValues(IEnumerable<ProjectRootElement> projectRootElements, string propertyName, string propertyValue)
		{
			IEnumerable<ProjectPropertyElement> badProperties = projectRootElements.SelectMany(x => x.Properties)
				.Where(x => x.Name.Equals(propertyName))
				.Where(x => !x.Value.Equals(propertyValue));
			foreach (ProjectPropertyElement property in badProperties)
			{
				Assert.Fail($"{property.Value} found in {property.ContainingProject.FullPath}");
			}
		}
	}
}
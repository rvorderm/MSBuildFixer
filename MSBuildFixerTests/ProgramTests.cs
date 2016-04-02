using System;
using System.Diagnostics;
using System.IO;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSBuildFixer;

namespace MSBuildFixerTests
{
	[TestClass]
	public class ProgramTests
	{
		[TestClass]
		public class FixOutputPropertyTests
		{
			[TestMethod]
			public void SetValue()
			{
				var projectRootElement = ProjectRootElement.Create();
				var projectPropertyGroupElement = projectRootElement.AddPropertyGroup();
				var projectPropertyElement = projectPropertyGroupElement.AddProperty("Output", string.Empty);
				var projectFixer = new SolutionWalker();
				projectFixer.FixOutputProperty(projectPropertyElement);
				Assert.AreEqual(Path.Combine("$(SolutionDir)", "bin", "$(Configuration)"), projectPropertyElement.Value);
			}
		}

		[TestClass]
		public class IntegrationTests
		{
			public void Execute(string fileName, string args)
			{
				Process process = new Process();
				ProcessStartInfo startInfo = new ProcessStartInfo();
				startInfo.WindowStyle = ProcessWindowStyle.Hidden;
				startInfo.FileName = fileName;
				startInfo.Arguments = args;
				process.StartInfo = startInfo;
				process.Start();
				process.WaitForExit();
			}

			public void QPop()
			{
				Execute("hg", "qpop");
			}

			public void QPush()
			{
				Execute("hg", "qpop");
			}

			public void Purge()
			{
				Execute("hg", "purge --all --files --dirs");
			}

			public void Update()
			{
				Execute("hg", "up -C");
			}

			public void CopyLib()
			{
				Execute("xcopy", "/S ..\\.lib .\\.lib\\");
			}

			public void MSBuild()
			{
				Execute(@"C:\Program Files (x86)\MSBuild\14.0\Bin\MSBuild.exe", "Personnel.sln");
			}

			[TestMethod]
			public void Timing()
			{
				long repetitions = 1;

				Environment.CurrentDirectory = @"C:\Repos\Default\Personnel - Copy\";
				var stopwatch = new Stopwatch();
				QPop();
				
				var initialTime = PurgeAndBuild(repetitions, stopwatch);

				QPush();
				var modifiedTime = PurgeAndBuild(repetitions, stopwatch);
				Console.WriteLine("Repetitions: " + repetitions);
				Console.WriteLine("Initial: " + new TimeSpan(initialTime) + " Average: " + new TimeSpan(initialTime / repetitions));
				Console.WriteLine("Initial: " + new TimeSpan(modifiedTime) + " Average: " + new TimeSpan(modifiedTime / repetitions));
			}

			private long PurgeAndBuild(long repetitions, Stopwatch stopwatch)
			{
				long totalBuildTime = 0;
				for (int i = 0; i < repetitions; i++)
				{
					Purge();
					CopyLib();
					stopwatch.Start();
					MSBuild();
					stopwatch.Stop();
					totalBuildTime += stopwatch.ElapsedMilliseconds;
				}
				return totalBuildTime;
			}
		}
	}
}

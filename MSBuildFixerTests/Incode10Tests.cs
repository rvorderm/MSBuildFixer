﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MSBuildFixer;
using MSBuildFixer.Fixes;
using MSBuildFixer.SampleFeatureToggles;
using MSBuildFixerTests.Fixes;

namespace MSBuildFixerTests
{
	[TestClass]
	public class Incode10Tests
	{
		private const string RootDir = @"C:\Repos\Incode10\";
		private const string BinPath = @"C:\Repos\Incode10\bin";
	    private const string Filename = "Incode10.sln";

	    private static readonly ProcessStartInfo Build = new ProcessStartInfo
		{
			WorkingDirectory = RootDir,
			WindowStyle = ProcessWindowStyle.Hidden,
			FileName = @"C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild.exe",
            Arguments = "Incode10.sln /p:Platform=\"Any CPU\" /maxcpucount:3 /verbosity:quiet /t:Build"
        };

        private static readonly ProcessStartInfo Rebuild = new ProcessStartInfo
		{
			WorkingDirectory = RootDir,
			WindowStyle = ProcessWindowStyle.Hidden,
			FileName = @"C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild.exe",
            Arguments = "Incode10.sln /p:Platform=\"Any CPU\" /maxcpucount:3 /fl1 /flp:logfile=Incode10.build.log;verbosity=diagnostic /fl2 /flp2:logfile=JustErrors.log;errorsonly \t:Rebuild"
        };

        private static readonly ProcessStartInfo Clean = new ProcessStartInfo
		{
			WorkingDirectory = RootDir,
			WindowStyle = ProcessWindowStyle.Hidden,
			FileName = @"C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild.exe",
            Arguments = "Incode10.sln /p:Platform=\"Any CPU\" /maxcpucount:3 /verbosity:quiet /t:Build"
        };

		private static readonly ProcessStartInfo HG_UP = new ProcessStartInfo
		{
			WorkingDirectory = RootDir,
			WindowStyle = ProcessWindowStyle.Hidden,
			FileName = "hg",
			Arguments = "up -C"
		};

		[TestMethod]
		public void PerformanceTestCopyLocal()
		{
			TimeAction(()=>DoProcess(HG_UP));
            Purge();
            var initialDuration = TimeAction(() => DoProcess(Build));
			Console.WriteLine($@"Initial Duration was {initialDuration}");
			TestSetup.SetToggleTo(CopyLocalToggle.Instance, true);
			DoFix<FixCopyLocal>(RootDir, Filename);
            Purge();
            var vanillaDuration = TimeAction(() => DoProcess(Build));
			Console.WriteLine($@"Vanilla Duration was {vanillaDuration}");
			DoFix<FixCopyLocal>(RootDir, Filename, x=>x.CopyStyle = CopyStyle.FirstOnly);
            Purge();
            var firstOnlyDuration = TimeAction(() => DoProcess(Build));
			Console.WriteLine($@"SingleCopy Duration was {firstOnlyDuration}");
			CleanDirectory(BinPath);
            DoFix<FixCopyLocal>(RootDir, Filename, x => x.CopyStyle = CopyStyle.LastOnly);
            var lastOnlyDuration = TimeAction(() => DoProcess(Build));
			Console.WriteLine($@"SingleCopy Duration was {lastOnlyDuration}");
			CleanDirectory(BinPath);
		}

		[TestMethod]
		public void PerformanceTestCopyToOutput()
		{
            Purge();
            TimeAction(() => DoProcess(HG_UP));
			var initialDuration = TimeAction(() => DoProcess(Build));
			var initialRebuild = TimeAction(() => DoProcess(Build));
			Console.WriteLine($@"Initial Duration was {initialDuration}, rebuild was {initialRebuild}");
			TestSetup.SetToggleTo(CopyToOutputDirectoryToggle.Instance, true);
			DoFix<FixCopyToOutputDirectory>(RootDir, Filename);
            Purge();
            var vanillaDuration = TimeAction(() => DoProcess(Build));
			var vanillaRebuild = TimeAction(() => DoProcess(Build));
			Console.WriteLine($@"Vanilla Duration was {vanillaDuration}, rebuild was {vanillaRebuild}");
			TimeAction(() => DoProcess(HG_UP));
		}

		[TestMethod]
		public void PerformanceRebuild()
		{
            TimeAction(() => DoProcess(Clean));
            Purge();
		    TimeAction(() => DoProcess(HG_UP));
			var initialDuration = TimeAction(() => DoProcess(Build));
			var initialRebuild = TimeAction(() => DoProcess(Rebuild));
			var initialClean = TimeAction(() => DoProcess(Clean));
			Console.WriteLine($@"Initial Duration was {initialDuration}, rebuild was {initialRebuild}, initial clean was {initialClean}");
			
		}

	    private static void Purge()
	    {
	        CleanDirectory(BinPath);
	        var objFolders = Directory.EnumerateDirectories(RootDir, "obj", SearchOption.AllDirectories);
	        foreach (var objFolder in objFolders)
	        {
	            CleanDirectory(objFolder);
	        }
	    }

	    private static void CleanDirectory(string path)
		{
			if (Directory.Exists(path)) Directory.Delete(path, true);
		}

		private static void DoFix<T>(string dir, string filename, Action<T> action = null) where T : IFix, new()
		{
			var projectFixer = new SolutionWalker();
			var fix = new T();
			fix.AttachTo(projectFixer);
			action?.Invoke(fix);
			projectFixer.VisitSolution(dir, filename);
		}

		private static TimeSpan TimeAction(Action a)
		{
		    var start = DateTime.Now;
            a.Invoke();
		    var end = DateTime.Now;
			var duration = end - start;
			return duration;
		}

	    private static void DoProcess(ProcessStartInfo processStartInfo)
	    {
	        var process = new Process
	        {
	            StartInfo = processStartInfo
	        };
	        process.Start();
	        process.WaitForExit();
	    }
	}
}
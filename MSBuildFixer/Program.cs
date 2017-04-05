using MSBuildFixer.Configuration;
using Serilog;
using Serilog.Filters;
using System;
using System.Diagnostics;
using System.IO;
using Serilog.Events;

namespace MSBuildFixer
{
	public class Program
	{
		static void Main(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
			ConfigureLogs();

			foreach (string solutionPath in SolutionConfiguration.Instance.Solutions)
			{
				Log.Information("Creating walker for {solutionPath}", solutionPath);
				SolutionWalker projectFixer = SolutionWalker.CreateWalker(solutionPath);
				projectFixer.VisitSolution();
				
			}

			Log.Information("Finished");
			if (Debugger.IsAttached)
			{
				Console.ReadLine();
			}
		}

		private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			Log.Fatal(e.ExceptionObject as Exception, "SolutionWalker ate it.");
		}

		private static void ConfigureLogs()
		{
			if (File.Exists(ReportsConfiguration.Instance.ReportFilePath))
			{
				File.Delete(ReportsConfiguration.Instance.ReportFilePath);
			}
			Log.Logger = new LoggerConfiguration()
				.Enrich.FromLogContext()
				.MinimumLevel.Is(ReportsConfiguration.Instance.LogEventLevel)
				.WriteTo.LiterateConsole()
				.WriteTo.Logger(lc => lc
					.Filter.ByIncludingOnly(Matching.WithProperty(ReportsConfiguration.PropertyName))
					.WriteTo.File(ReportsConfiguration.Instance.ReportFilePath))
				.CreateLogger();
		}
	}
}

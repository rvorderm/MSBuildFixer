using Serilog.Events;
using System;
using System.Configuration;

namespace MSBuildFixer.Configuration
{
	public class ReportsConfiguration
	{
		public const string PropertyName = "Report";
		public static ReportsConfiguration Instance = (dynamic) ConfigurationManager.GetSection("ReportsConfiguration");
		public string LogLevel { get; set; }

		public string ReportFilePath { get; set; }
		public string ReferenceRegex { get; set; }
		public bool ListUntrackedProjects { get; set; }
		public bool ListCircularDependencies { get; set; }

		public LogEventLevel LogEventLevel
		{
			get
			{
				LogEventLevel level;
				if (string.IsNullOrEmpty(LogLevel) || !Enum.TryParse(LogLevel, out level)) return LogEventLevel.Information;
				return level;
			}
		}
	}
}
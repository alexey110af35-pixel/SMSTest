namespace SmsTestWpf.Models
{
	public class AppSettings
	{
		public List<string> EnvironmentVariables { get; set; } = new List<string>();
		public LoggingSettings Logging { get; set; } = new LoggingSettings();
	}

	public class LoggingSettings
	{
		public string LogDirectory { get; set; } = "Logs";
		public string LogLevel { get; set; } = "Info";
	}
}
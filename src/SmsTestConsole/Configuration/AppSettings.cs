namespace SmsTestConsole.Configuration
{
	public class AppSettings
	{
		public ConnectionStrings ConnectionStrings { get; set; } = new ConnectionStrings();
		public ServerSettings ServerSettings { get; set; } = new ServerSettings();
		public LoggingSettings Logging { get; set; } = new LoggingSettings();
	}

	public class ConnectionStrings
	{
		public string DefaultConnection { get; set; } = string.Empty;
	}

	public class ServerSettings
	{
		public string BaseUrl { get; set; } = string.Empty;
		public string Username { get; set; } = string.Empty;
		public string Password { get; set; } = string.Empty;
		public bool UseGrpc { get; set; } = false;
		public string GrpcAddress { get; set; } = string.Empty;
		public bool UseMockData { get; set; } = false; // Добавлено
	}

	public class LoggingSettings
	{
		public string LogDirectory { get; set; } = "Logs";
		public string LogLevel { get; set; } = "Info";
	}
}
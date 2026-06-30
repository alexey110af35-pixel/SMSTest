using Microsoft.Extensions.Configuration;

namespace SmsTestConsole.Logging
{
	public static class LogConfiguration
	{
		public static string GetFilePath(IConfiguration configuration)
		{
			var logDir = configuration.GetValue<string>("Logging:LogDirectory") ?? "Logs";
			if (!Directory.Exists(logDir))
			{
				Directory.CreateDirectory(logDir);
			}

			var dateStr = DateTime.Now.ToString("yyyyMMdd");
			return Path.Combine(logDir, $"test-sms-console-app-{dateStr}.log");
		}

		public static string MaskConnectionString(string connString)
		{
			if (string.IsNullOrEmpty(connString))
				return connString;

			var parts = connString.Split(';');
			var result = new List<string>();

			foreach (var part in parts)
			{
				if (part.Trim().StartsWith("Password=", StringComparison.OrdinalIgnoreCase))
				{
					result.Add("Password=***");
				}
				else
				{
					result.Add(part);
				}
			}

			return string.Join(";", result);
		}
	}
}
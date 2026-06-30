using Microsoft.Extensions.Logging;

namespace SmsTestConsole.Helpers
{
	public static class LogHelper
	{
		public static void LogWithColor(this ILogger logger, LogLevel level, string message, params object[] args)
		{
			var originalColor = Console.ForegroundColor;

			try
			{
				switch (level)
				{
					case LogLevel.Error:
						Console.ForegroundColor = ConsoleColor.Red;
						break;
					case LogLevel.Warning:
						Console.ForegroundColor = ConsoleColor.Yellow;
						break;
					case LogLevel.Information:
						Console.ForegroundColor = ConsoleColor.Cyan;
						break;
					case LogLevel.Debug:
						Console.ForegroundColor = ConsoleColor.Gray;
						break;
					case LogLevel.Critical:
						Console.ForegroundColor = ConsoleColor.Magenta;
						break;
				}

				logger.Log(level, message, args);
			}
			finally
			{
				Console.ForegroundColor = originalColor;
			}
		}

		public static void LogSuccess(this ILogger logger, string message, params object[] args)
		{
			var originalColor = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Green;
			logger.LogInformation($"{message}", args);
			Console.ForegroundColor = originalColor;
		}

		public static void LogError(this ILogger logger, string message, params object[] args)
		{
			var originalColor = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Red;
			logger.LogError($"{message}", args);
			Console.ForegroundColor = originalColor;
		}

		public static void LogWarning(this ILogger logger, string message, params object[] args)
		{
			var originalColor = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Yellow;
			logger.LogWarning($"{message}", args);
			Console.ForegroundColor = originalColor;
		}

		public static void LogInfo(this ILogger logger, string message, params object[] args)
		{
			var originalColor = Console.ForegroundColor;
			Console.ForegroundColor = ConsoleColor.Cyan;
			logger.LogInformation($"{message}", args);
			Console.ForegroundColor = originalColor;
		}
	}
}
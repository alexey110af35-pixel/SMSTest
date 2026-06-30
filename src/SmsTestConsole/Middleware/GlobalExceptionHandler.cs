using Microsoft.Extensions.Logging;

namespace SmsTestConsole.Middleware
{
	public class GlobalExceptionHandler
	{
		private readonly ILogger<GlobalExceptionHandler> _logger;

		public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
		{
			_logger = logger;
		}

		public async Task HandleAsync(Func<Task> action, string context = "Приложение")
		{
			try
			{
				await action();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Ошибка в {context}");
				Console.WriteLine($"\nКритическая ошибка: {ex.Message}");
				Console.ReadKey();
				throw;
			}
		}

		public async Task<T> HandleAsync<T>(Func<Task<T>> action, string context = "Приложение")
		{
			try
			{
				return await action();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, $"Ошибка в {context}");
				Console.WriteLine($"\nКритическая ошибка: {ex.Message}");
				throw; 
			}
		}
	}
}
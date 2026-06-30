using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using SmsTestConsole.Database;
using SmsTestConsole.Middleware;
using SmsTestConsole.Models;

namespace SmsTestConsole
{
	internal class Program
	{
		static async Task Main(string[] args)
		{
			using var app = CreateHostBuilder(args).Build();

			var logger = app.Services.GetRequiredService<ILogger<Program>>();
			var exceptionHandler = app.Services.GetRequiredService<GlobalExceptionHandler>();

			await exceptionHandler.HandleAsync(async () =>
			{
				logger.LogInformation("Приложение запущено");
				logger.LogInformation("Инициализация базы данных...");

				using (var scope = app.Services.CreateScope())
				{
					var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
					await dbContext.Database.MigrateAsync();
				}

				logger.LogInformation("База данных успешно инициализирована");

				Console.WriteLine("\nБаза данных и таблица успешно созданы!");
				Console.WriteLine("\nНажмите любую клавишу для выхода...");
				Console.ReadKey();

			}, "Запуск приложения");
		}

		static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureAppConfiguration((context, config) =>
				{
					config.SetBasePath(Directory.GetCurrentDirectory());
					config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
					config.AddEnvironmentVariables();
				})
				.ConfigureServices((context, services) =>
				{
					// Конфигурация
					var appSettings = context.Configuration.Get<AppSettings>() ?? new AppSettings();
					services.Configure<AppSettings>(options =>
					{
						options.ConnectionStrings = appSettings.ConnectionStrings;
						options.ServerSettings = appSettings.ServerSettings;
						options.Logging = appSettings.Logging;
					});
					services.AddSingleton(appSettings);
					services.AddDbContext<AppDbContext>((serviceProvider, options) =>
					{
						var settings = serviceProvider.GetRequiredService<AppSettings>();
						var connectionString = settings.ConnectionStrings.DefaultConnection;

						if (string.IsNullOrEmpty(connectionString))
						{
							throw new InvalidOperationException(
								"Строка подключения не найдена в appsettings.json. "
							);
						}

						options.UseNpgsql(connectionString);
						//options.LogTo(Console.WriteLine, LogLevel.Information);
					});

					services.AddSingleton<GlobalExceptionHandler>();
				})
				.UseSerilog((context, config) =>
				{
					var logFilePath = GetLogFilePath(context.Configuration);

					config.ReadFrom.Configuration(context.Configuration)
						.WriteTo.Console(
							outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"
						)
						.WriteTo.File(
							logFilePath,
							outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
							rollingInterval: RollingInterval.Day,
							retainedFileCountLimit: 7,
							fileSizeLimitBytes: 10 * 1024 * 1024,
							rollOnFileSizeLimit: true
						)
						.Enrich.FromLogContext();

					config.MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning);
					config.MinimumLevel.Override("Microsoft.Hosting.Lifetime", Serilog.Events.LogEventLevel.Information);
					config.MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning);
					config.MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning);
				});

		static string GetLogFilePath(IConfiguration configuration)
		{
			var logDir = configuration.GetValue<string>("Logging:LogDirectory") ?? "Logs";
			if (!Directory.Exists(logDir))
			{
				Directory.CreateDirectory(logDir);
			}

			var dateStr = DateTime.Now.ToString("yyyyMMdd");
			return Path.Combine(logDir, $"test-sms-console-app-{dateStr}.log");
		}
	}
}
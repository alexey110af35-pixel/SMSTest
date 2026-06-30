using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using SmsTestConsole.Logging;

namespace SmsTestConsole.Host
{
	public static class HostBuilder
	{
		public static IHostBuilder Create()
		{
			return Microsoft.Extensions.Hosting.Host.CreateDefaultBuilder()
				.ConfigureAppConfiguration((context, config) =>
				{
					config.SetBasePath(Directory.GetCurrentDirectory());
					config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
					config.AddEnvironmentVariables();
				})
				.ConfigureServices((context, services) =>
				{
					// Регистрация всех сервисов
					ServiceRegistration.Register(services, context.Configuration);
				})
				.UseSerilog((context, config) =>
				{
					// Настройка Serilog
					var logFilePath = LogConfiguration.GetFilePath(context.Configuration);

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
		}
	}
}
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmsTestConsole.Configuration;
using SmsTestConsole.Database;
using SmsTestConsole.Middleware;
using SmsTestConsole.Services;
using SmsTestLibrary.Services;

namespace SmsTestConsole.Host
{
	public static class ServiceRegistration
	{
		public static void Register(IServiceCollection services, IConfiguration configuration)
		{
			// Конфигурация
			var appSettings = configuration.Get<AppSettings>() ?? new AppSettings();
			services.Configure<AppSettings>(options =>
			{
				options.ConnectionStrings = appSettings.ConnectionStrings;
				options.ServerSettings = appSettings.ServerSettings;
				options.Logging = appSettings.Logging;
			});
			services.AddSingleton(appSettings);

			// DbContext
			services.AddDbContext<AppDbContext>((serviceProvider, options) =>
			{
				var settings = serviceProvider.GetRequiredService<AppSettings>();
				var connectionString = settings.ConnectionStrings.DefaultConnection;

				if (string.IsNullOrEmpty(connectionString))
				{
					throw new InvalidOperationException(
						"Строка подключения не найдена в appsettings.json"
					);
				}

				options.UseNpgsql(connectionString, npgsqlOptions =>
				{
					npgsqlOptions.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName);
				});
			});

			// SmsService из библиотеки
			services.AddSingleton(sp =>
			{
				var settings = sp.GetRequiredService<AppSettings>();

				if (settings.ServerSettings.UseGrpc)
				{
					return new SmsService(
						settings.ServerSettings.GrpcAddress,
						useGrpc: true
					);
				}
				else
				{
					return new SmsService(
						settings.ServerSettings.BaseUrl,
						settings.ServerSettings.Username,
						settings.ServerSettings.Password
					);
				}
			});

			// Сервисы приложения
			services.AddScoped<IApiService, ApiService>();
			services.AddScoped<IDatabaseService, DatabaseService>();
			services.AddScoped<IConsoleAppService, ConsoleAppService>();

			// Глобальный обработчик исключений
			services.AddSingleton<GlobalExceptionHandler>();

			// Логирование
			services.AddLogging(loggingBuilder =>
			{
				loggingBuilder.ClearProviders();
				loggingBuilder.AddConsole();
			});
		}
	}
}
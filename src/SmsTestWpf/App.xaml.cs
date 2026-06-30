using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using SmsTestWpf.Models;
using SmsTestWpf.Services;
using SmsTestWpf.ViewModels;
using SmsTestWpf.Views;
using System.IO;
using System.Windows;

namespace SmsTestWpf
{
	public partial class App : Application
	{
		private ServiceProvider? _serviceProvider;

		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			// Настройка Serilog
			var logFilePath = GetLogFilePath();
			Log.Logger = new LoggerConfiguration()
				.MinimumLevel.Information()
				.WriteTo.File(
					logFilePath,
					outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}",
					rollingInterval: RollingInterval.Day,
					retainedFileCountLimit: 7
				)
				.CreateLogger();

			// Настройка DI
			var services = new ServiceCollection();
			ConfigureServices(services);
			_serviceProvider = services.BuildServiceProvider();

			// Создаем и показываем окно
			var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
			mainWindow.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();
			mainWindow.Show();
		}

		private void ConfigureServices(ServiceCollection services)
		{
			// Конфигурация
			var configuration = new ConfigurationBuilder()
				.SetBasePath(AppContext.BaseDirectory)
				.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
				.Build();

			var appSettings = configuration.Get<AppSettings>() ?? new AppSettings();

			if (appSettings.EnvironmentVariables == null || appSettings.EnvironmentVariables.Count == 0)
			{
				appSettings.EnvironmentVariables = new List<string>
				{
					"MY_APP_SETTING_1",
					"MY_APP_SETTING_2",
					"MY_APP_SETTING_3"
				};
			}

			services.AddSingleton(appSettings);
			services.AddSingleton(appSettings.EnvironmentVariables);

			// Логирование
			services.AddLogging(loggingBuilder =>
			{
				loggingBuilder.ClearProviders();
				loggingBuilder.AddSerilog(Log.Logger);
			});

			// Сервисы
			services.AddSingleton<IEnvironmentService, EnvironmentService>();

			// ViewModel
			services.AddSingleton<MainViewModel>();

			// View
			services.AddTransient<MainWindow>();
		}

		private string GetLogFilePath()
		{
			var logDir = "Logs";
			if (!Directory.Exists(logDir))
			{
				Directory.CreateDirectory(logDir);
			}

			var dateStr = DateTime.Now.ToString("yyyyMMdd");
			return Path.Combine(logDir, $"test-sms-wpf-app-{dateStr}.log");
		}

		protected override void OnExit(ExitEventArgs e)
		{
			_serviceProvider?.Dispose();
			Log.CloseAndFlush();
			base.OnExit(e);
		}
	}
}
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SmsTestConsole.Database;
using SmsTestConsole.Middleware;
using SmsTestConsole.Services;

using var app = SmsTestConsole.Host.HostBuilder.Create().Build();

var logger = app.Services.GetRequiredService<ILogger<Program>>();
var exceptionHandler = app.Services.GetRequiredService<GlobalExceptionHandler>();

using var cts = new CancellationTokenSource();

Console.CancelKeyPress += (sender, e) =>
{
	e.Cancel = true;
	logger.LogWarning("Получен сигнал отмены (Ctrl+C)");
	cts.Cancel();
};

await exceptionHandler.HandleAsync(async () =>
{
	logger.LogInformation("Приложение запущено");
	logger.LogInformation("Инициализация базы данных...");

	using (var scope = app.Services.CreateScope())
	{
		var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
		var databaseService = scope.ServiceProvider.GetRequiredService<IDatabaseService>();
		await databaseService.EnsureDatabaseCreatedAsync(cts.Token);
	}

	logger.LogInformation("База данных успешно инициализирована");

	var consoleApp = app.Services.GetRequiredService<IConsoleAppService>();
	await consoleApp.RunAsync(cts.Token);

}, "Запуск приложения");
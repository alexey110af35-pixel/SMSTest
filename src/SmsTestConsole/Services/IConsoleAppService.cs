namespace SmsTestConsole.Services
{
	public interface IConsoleAppService
	{
		Task RunAsync(CancellationToken cancellationToken = default);
	}
}
using SmsTestLibrary.Models;

namespace SmsTestConsole.Services
{
	public interface IApiService
	{
		Task<List<Dish>> GetMenuAsync(CancellationToken cancellationToken = default);
		Task<bool> SendOrderAsync(Order order);
	}
}
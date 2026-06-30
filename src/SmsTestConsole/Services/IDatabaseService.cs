using SmsTestLibrary.Models;

namespace SmsTestConsole.Services
{
	public interface IDatabaseService
	{
		Task SaveDishesAsync(List<Dish> dishes);
		Task<List<Dish>> GetDishesAsync();
		Task EnsureDatabaseCreatedAsync(CancellationToken cancellationToken = default);
	}
}
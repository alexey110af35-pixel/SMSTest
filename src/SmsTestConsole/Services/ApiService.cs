using Microsoft.Extensions.Logging;
using SmsTestConsole.Configuration;
using SmsTestLibrary.Models;
using SmsTestLibrary.Services;

namespace SmsTestConsole.Services
{
	public class ApiService : IApiService
	{
		private readonly SmsService _smsService;
		private readonly ILogger<ApiService> _logger;
		private readonly bool _useMockData;

		public ApiService(SmsService smsService, ILogger<ApiService> logger, AppSettings settings)
		{
			_smsService = smsService;
			_logger = logger;
			_useMockData = settings.ServerSettings.UseMockData;
		}
		
		public async Task<List<Dish>> GetMenuAsync(CancellationToken cancellationToken = default)
		{
			if (_useMockData)
			{
				_logger.LogWarning("Используются тестовые данные (режим Mock)");
				await Task.Delay(100, cancellationToken); // Имитация задержки с поддержкой отмены
				return GetMockDishes();
			}

			_logger.LogInformation("Запрос меню с сервера...");

			try
			{
				var dishes = await _smsService.GetMenuAsync();
				_logger.LogInformation($"Получено {dishes.Count} блюд");
				return dishes;
			}
			catch (OperationCanceledException)
			{
				_logger.LogWarning("Запрос меню был отменен");
				throw;
			}
			catch (Exception ex)
			{
				_logger.LogError($"Ошибка при получении меню: {ex.Message}");
				throw;
			}
		}

		public async Task<bool> SendOrderAsync(Order order)
		{
			if (_useMockData)
			{
				_logger.LogWarning("Используются тестовые данные (режим Mock)");
				_logger.LogInformation($"Отправка заказа {order.Id}...");
				await Task.Delay(500); // Имитация задержки
				_logger.LogInformation($"Заказ {order.Id} успешно отправлен (Mock)");
				return true;
			}

			_logger.LogInformation($"Отправка заказа {order.Id}...");

			try
			{
				var result = await _smsService.SendOrderAsync(order);
				_logger.LogInformation($"Заказ {order.Id} успешно отправлен");
				return result;
			}
			catch (Exception ex)
			{
				_logger.LogError($"Ошибка при отправке заказа: {ex.Message}");
				throw;
			}
		}

		private List<Dish> GetMockDishes()
		{
			return new List<Dish>
			{
				new Dish
				{
					Id = "5979224",
					Article = "A1004292",
					Name = "Каша гречневая",
					Price = 50,
					IsWeighted = false,
					FullPath = "ПРОИЗВОДСТВО\\Гарниры",
					Barcodes = new List<string> { "57890975627974236429" }
				},
				new Dish
				{
					Id = "9084246",
					Article = "A1004293",
					Name = "Конфеты Коровка",
					Price = 300,
					IsWeighted = true,
					FullPath = "ДЕСЕРТЫ\\Развес",
					Barcodes = new List<string>()
				},
				new Dish
				{
					Id = "1234567",
					Article = "A1004294",
					Name = "Салат Цезарь",
					Price = 250,
					IsWeighted = false,
					FullPath = "САЛАТЫ\\Горячие",
					Barcodes = new List<string> { "12345678901234567890" }
				},
				new Dish
				{
					Id = "7654321",
					Article = "A1004295",
					Name = "Стейк Рибай",
					Price = 450,
					IsWeighted = true,
					FullPath = "МЯСО\\Стейки",
					Barcodes = new List<string>()
				}
			};
		}
	}
}
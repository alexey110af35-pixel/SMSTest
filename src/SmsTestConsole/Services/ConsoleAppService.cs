using Microsoft.Extensions.Logging;
using SmsTestLibrary.Models;

namespace SmsTestConsole.Services
{
	public class ConsoleAppService : IConsoleAppService
	{
		private readonly IApiService _apiService;
		private readonly IDatabaseService _databaseService;
		private readonly ILogger<ConsoleAppService> _logger;

		public ConsoleAppService(
			IApiService apiService,
			IDatabaseService databaseService,
			ILogger<ConsoleAppService> logger)
		{
			_apiService = apiService;
			_databaseService = databaseService;
			_logger = logger;
		}

		public async Task RunAsync(CancellationToken cancellationToken = default)
		{
			// Шаг 1: Получение меню с сервера
			_logger.LogInformation("Получение меню с сервера...");
			var dishes = await _apiService.GetMenuAsync(cancellationToken);

			// Шаг 2: Сохранение в БД
			await _databaseService.SaveDishesAsync(dishes);

			// Шаг 3: Вывод меню в консоль
			DisplayMenu(dishes);

			// Шаг 4: Ввод заказа от пользователя (с поддержкой отмены)
			var order = await GetOrderFromUserAsync(dishes, cancellationToken);

			// Шаг 5: Отправка заказа
			var result = await _apiService.SendOrderAsync(order);

			// Шаг 6: Вывод результата
			Console.WriteLine(result ? "УСПЕХ" : "Ошибка при отправке заказа");
		}

		private void DisplayMenu(List<Dish> dishes)
		{
			Console.WriteLine("\n=== МЕНЮ ===\n");

			foreach (var dish in dishes)
			{
				Console.WriteLine($"{dish.Name} – {dish.Article} – {dish.Price:F2} руб.");
			}

			Console.WriteLine($"\nВсего блюд: {dishes.Count}\n");
		}
		
		private async Task<Order> GetOrderFromUserAsync(List<Dish> availableDishes, CancellationToken cancellationToken = default)
		{
			var order = new Order
			{
				Id = Guid.NewGuid().ToString()
			};

			await Task.Run(() =>
			{
				while (!cancellationToken.IsCancellationRequested)
				{
					try
					{
						Console.WriteLine("\n=== ВВОД ЗАКАЗА ===");
						Console.WriteLine("Формат ввода: Код1:Количество1;Код2:Количество2;...");
						Console.WriteLine("Например: A1004292:2;A1004293:0.408");
						Console.WriteLine("(Для выхода нажмите Ctrl+C)");
						Console.Write("Введите заказ: ");

						var input = Console.ReadLine()?.Trim();
						if (string.IsNullOrEmpty(input))
						{
							Console.WriteLine("Ввод не может быть пустым. Попробуйте снова.");
							continue;
						}

						var entries = input.Split(';', StringSplitOptions.RemoveEmptyEntries);
						var orderItems = new List<OrderItem>();
						var invalidEntries = new List<string>();

						foreach (var entry in entries)
						{
							// Проверяем отмену при обработке каждой позиции
							if (cancellationToken.IsCancellationRequested)
							{
								Console.WriteLine("\nВвод отменен пользователем");
								throw new OperationCanceledException(cancellationToken);
							}

							var parts = entry.Split(':');
							if (parts.Length != 2)
							{
								Console.WriteLine($"Неверный формат: '{entry}'. Используйте формат 'Код:Количество'");
								invalidEntries.Add(entry);
								continue;
							}

							var code = parts[0].Trim();
							if (!double.TryParse(parts[1].Trim(),
								System.Globalization.NumberStyles.Any,
								System.Globalization.CultureInfo.InvariantCulture,
								out var quantity))
							{
								Console.WriteLine($"Неверное количество: '{parts[1]}'");
								invalidEntries.Add(entry);
								continue;
							}

							if (quantity <= 0)
							{
								Console.WriteLine($"Количество должно быть больше нуля: '{quantity}'");
								invalidEntries.Add(entry);
								continue;
							}

							var dish = availableDishes.FirstOrDefault(d => d.Article == code);
							if (dish == null)
							{
								Console.WriteLine($"Код '{code}' не найден в меню");
								invalidEntries.Add(entry);
								continue;
							}

							orderItems.Add(new OrderItem
							{
								Id = dish.Id,
								Quantity = quantity
							});
						}

						if (invalidEntries.Count > 0)
						{
							Console.WriteLine($"\nОбнаружены некорректные позиции: {string.Join(", ", invalidEntries)}");
							Console.WriteLine("Попробуйте снова.");
							continue;
						}

						if (orderItems.Count == 0)
						{
							Console.WriteLine("Не введено ни одной корректной позиции. Попробуйте снова.");
							continue;
						}

						order.OrderItems = orderItems;
						Console.WriteLine($"\nВведено {orderItems.Count} позиций");

						// Выходим из цикла, если всё успешно
						break;
					}
					catch (OperationCanceledException)
					{
						Console.WriteLine("\nВвод отменен пользователем");
						throw;
					}
					catch (Exception ex)
					{
						Console.WriteLine($"Ошибка при вводе: {ex.Message}. Попробуйте снова.");
					}
				}
			}, cancellationToken);

			// Если цикл прерван из-за отмены
			if (cancellationToken.IsCancellationRequested)
			{
				Console.WriteLine("\nВвод заказа отменен");
				throw new OperationCanceledException(cancellationToken);
			}

			return order;
		}
	}
}
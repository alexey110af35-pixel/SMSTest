using System.Text;
using System.Text.Json;
using SmsTestLibrary.Models;

namespace SmsTestLibrary.Services
{
	public class HttpSmsService : IDisposable
	{
		private readonly HttpClient _httpClient;
		private readonly string _baseUrl;

		public HttpSmsService(string baseUrl, string username, string password)
		{
			_baseUrl = baseUrl;

			_httpClient = new HttpClient();
			
			var authString = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
			_httpClient.DefaultRequestHeaders.Authorization =
				new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authString);

			_httpClient.DefaultRequestHeaders.Accept.Add(
				new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
		}

		public async Task<List<Dish>> GetMenuAsync()
		{
			var request = new
			{
				Command = "GetMenu",
				CommandParameters = new { WithPrice = true }
			};

			var json = JsonSerializer.Serialize(request);
			var content = new StringContent(json, Encoding.UTF8, "application/json");

			var response = await _httpClient.PostAsync(_baseUrl, content);
			var responseContent = await response.Content.ReadAsStringAsync();

			using var doc = JsonDocument.Parse(responseContent);
			var root = doc.RootElement;

			var success = root.GetProperty("Success").GetBoolean();
			if (!success)
			{
				var error = root.GetProperty("ErrorMessage").GetString();
				throw new Exception(error ?? "Ошибка получения меню");
			}

			var menuItems = new List<Dish>();
			if (root.TryGetProperty("Data", out var data) &&
				data.TryGetProperty("MenuItems", out var items))
			{
				foreach (var item in items.EnumerateArray())
				{
					var dish = new Dish
					{
						Id = item.GetProperty("Id").GetString() ?? string.Empty,
						Article = item.GetProperty("Article").GetString() ?? string.Empty,
						Name = item.GetProperty("Name").GetString() ?? string.Empty,
						Price = item.GetProperty("Price").GetDouble(),
						IsWeighted = item.GetProperty("IsWeighted").GetBoolean(),
						FullPath = item.GetProperty("FullPath").GetString() ?? string.Empty
					};

					if (item.TryGetProperty("Barcodes", out var barcodes))
					{
						foreach (var barcode in barcodes.EnumerateArray())
						{
							dish.Barcodes.Add(barcode.GetString() ?? string.Empty);
						}
					}

					menuItems.Add(dish);
				}
			}

			return menuItems;
		}
		
		public async Task<bool> SendOrderAsync(Order order)
		{
			if (order == null)
				throw new ArgumentNullException(nameof(order));

			var request = new
			{
				Command = "SendOrder",
				CommandParameters = new
				{
					OrderId = order.Id ?? Guid.NewGuid().ToString(),
					MenuItems = order.OrderItems.Select(item => new
					{
						Id = item.Id,
						Quantity = item.Quantity.ToString(System.Globalization.CultureInfo.InvariantCulture)
					})
				}
			};

			var json = JsonSerializer.Serialize(request);
			var content = new StringContent(json, Encoding.UTF8, "application/json");

			var response = await _httpClient.PostAsync(_baseUrl, content);
			var responseContent = await response.Content.ReadAsStringAsync();

			using var doc = JsonDocument.Parse(responseContent);
			var root = doc.RootElement;

			var success = root.GetProperty("Success").GetBoolean();
			if (!success)
			{
				var error = root.GetProperty("ErrorMessage").GetString();
				throw new Exception(error ?? "Ошибка отправки заказа");
			}

			return true;
		}

		public void Dispose()
		{
			_httpClient?.Dispose();
		}
	}
}
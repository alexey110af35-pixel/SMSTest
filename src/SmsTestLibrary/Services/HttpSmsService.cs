using System.Text;
using System.Text.Json;
using SmsTestLibrary.Models;

namespace SmsTestLibrary.Services
{
	public class HttpSmsService : IDisposable
	{
		private readonly HttpClient _httpClient;
		private readonly string _baseUrl;
		private static readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
		{
			PropertyNameCaseInsensitive = true,
			PropertyNamingPolicy = JsonNamingPolicy.CamelCase
		};

		public HttpSmsService(string baseUrl, string username, string password)
		{
			_baseUrl = baseUrl;

			_httpClient = new HttpClient();
			var authString = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}"));
			_httpClient.DefaultRequestHeaders.Authorization =
				new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authString);
			_httpClient.DefaultRequestHeaders.Accept.Add(
				new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
			_httpClient.Timeout = TimeSpan.FromSeconds(30);
		}

		public async Task<List<Dish>> GetMenuAsync()
		{
			try
			{
				var request = new
				{
					Command = "GetMenu",
					CommandParameters = new { WithPrice = true }
				};

				var json = JsonSerializer.Serialize(request);
				var content = new StringContent(json, Encoding.UTF8, "application/json");

				var response = await _httpClient.PostAsync(_baseUrl, content);
				response.EnsureSuccessStatusCode();

				var responseContent = await response.Content.ReadAsStringAsync();

				if (string.IsNullOrWhiteSpace(responseContent))
				{
					throw new Exception("Сервер вернул пустой ответ");
				}

				var result = JsonSerializer.Deserialize<GetMenuResponse>(responseContent, _jsonOptions);

				if (result == null)
				{
					throw new Exception("Не удалось десериализовать ответ сервера");
				}

				if (!result.Success)
				{
					throw new Exception(result.ErrorMessage ?? "Ошибка получения меню");
				}

				return result.Data?.MenuItems ?? new List<Dish>();
			}
			catch (HttpRequestException ex)
			{
				throw new Exception($"Ошибка HTTP запроса: {ex.Message}", ex);
			}
			catch (JsonException ex)
			{
				throw new Exception($"Ошибка парсинга JSON: {ex.Message}", ex);
			}
			catch (Exception ex)
			{
				throw new Exception($"Ошибка при получении меню: {ex.Message}", ex);
			}
		}

		public async Task<bool> SendOrderAsync(Order order)
		{
			try
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
				response.EnsureSuccessStatusCode();

				var responseContent = await response.Content.ReadAsStringAsync();

				if (string.IsNullOrWhiteSpace(responseContent))
				{
					throw new Exception("Сервер вернул пустой ответ");
				}

				var result = JsonSerializer.Deserialize<SendOrderResponse>(responseContent, _jsonOptions);

				if (result == null)
				{
					throw new Exception("Не удалось десериализовать ответ сервера");
				}

				if (!result.Success)
				{
					throw new Exception(result.ErrorMessage ?? "Ошибка отправки заказа");
				}

				return true;
			}
			catch (HttpRequestException ex)
			{
				throw new Exception($"Ошибка HTTP запроса: {ex.Message}", ex);
			}
			catch (JsonException ex)
			{
				throw new Exception($"Ошибка парсинга JSON: {ex.Message}", ex);
			}
			catch (Exception ex)
			{
				throw new Exception($"Ошибка при отправке заказа: {ex.Message}", ex);
			}
		}

		public void Dispose()
		{
			_httpClient?.Dispose();
		}
	}
}
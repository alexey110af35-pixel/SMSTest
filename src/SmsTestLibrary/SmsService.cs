using System.Text;
using System.Text.Json;
using SmsTestLibrary.Models;

namespace SmsTestLibrary
{
    public class SmsService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;
        private readonly string _username;
        private readonly string _password;

        public SmsService(string baseUrl, string username, string password)
        {
            _baseUrl = baseUrl;
            _username = username;
            _password = password;

            var handler = new HttpClientHandler();
            handler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true;

            _httpClient = new HttpClient(handler);

            var authString = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_username}:{_password}"));
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authString);
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        /// <summary>
        /// Метод 1: Получение списка блюд с сервера
        /// </summary>
        public async Task<List<Dish>> GetMenuAsync(bool withPrice = true)
        {
            try
            {
                var request = new GetMenuRequest
                {
                    CommandParameters = new GetMenuParameters { WithPrice = withPrice }
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_baseUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<GetMenuResponse>(responseContent);

                if (result == null)
                {
                    throw new Exception("Не удалось десериализовать ответ сервера");
                }

                if (!result.Success)
                {
                    throw new Exception(result.ErrorMessage ?? "Неизвестная ошибка сервера");
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
        }

        /// <summary>
        /// Метод 2: Отправка заказа на сервер
        /// </summary>
        public async Task<bool> SendOrderAsync(Order order)
        {
            try
            {
                if (order == null)
                {
                    throw new ArgumentNullException(nameof(order), "Заказ не может быть null");
                }

                if (string.IsNullOrEmpty(order.Id))
                {
                    order.Id = Guid.NewGuid().ToString();
                }

                var request = new SendOrderRequest
                {
                    CommandParameters = new SendOrderParameters
                    {
                        OrderId = order.Id,
                        MenuItems = order.OrderItems.Select(item => new OrderRequestItem
                        {
                            Id = item.Id,
                            Quantity = item.Quantity.ToString(System.Globalization.CultureInfo.InvariantCulture)
                        }).ToList()
                    }
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(_baseUrl, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                var result = JsonSerializer.Deserialize<SendOrderResponse>(responseContent);

                if (result == null)
                {
                    throw new Exception("Не удалось десериализовать ответ сервера");
                }

                if (!result.Success)
                {
                    throw new Exception(result.ErrorMessage ?? "Неизвестная ошибка сервера");
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
        }
    }
}
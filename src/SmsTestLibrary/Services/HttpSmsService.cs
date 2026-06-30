using System.Text;
using System.Text.Json;
using SmsTestLibrary.Models;

namespace SmsTestLibrary.Services
{
    public class HttpSmsService
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

            var result = JsonSerializer.Deserialize<GetMenuResponse>(responseContent);

            if (result == null || !result.Success)
            {
                throw new Exception(result?.ErrorMessage ?? "Ошибка получения меню");
            }

            return result.Data?.MenuItems ?? new List<Dish>();
        }

        public async Task<bool> SendOrderAsync(Order order)
        {
            var request = new
            {
                Command = "SendOrder",
                CommandParameters = new
                {
                    OrderId = order.Id,
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

            var result = JsonSerializer.Deserialize<SendOrderResponse>(responseContent);

            if (result == null || !result.Success)
            {
                throw new Exception(result?.ErrorMessage ?? "Ошибка отправки заказа");
            }

            return true;
        }

        private class GetMenuResponse
        {
            public string Command { get; set; } = string.Empty;
            public bool Success { get; set; }
            public string ErrorMessage { get; set; } = string.Empty;
            public MenuData Data { get; set; } = new MenuData();
        }

        private class MenuData
        {
            public List<Dish> MenuItems { get; set; } = new List<Dish>();
        }

        private class SendOrderResponse
        {
            public string Command { get; set; } = string.Empty;
            public bool Success { get; set; }
            public string ErrorMessage { get; set; } = string.Empty;
        }
    }
}
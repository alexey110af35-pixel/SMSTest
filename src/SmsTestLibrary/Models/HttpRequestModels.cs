using System.Text.Json.Serialization;

namespace SmsTestLibrary.Models
{
    public class BaseResponse
    {
        [JsonPropertyName("Command")]
        public string Command { get; set; } = string.Empty;

        [JsonPropertyName("Success")]
        public bool Success { get; set; }

        [JsonPropertyName("ErrorMessage")]
        public string ErrorMessage { get; set; } = string.Empty;
    }

    public class GetMenuResponse : BaseResponse
    {
        [JsonPropertyName("Data")]
        public MenuData Data { get; set; } = new MenuData();
    }

    public class MenuData
    {
        [JsonPropertyName("MenuItems")]
        public List<Dish> MenuItems { get; set; } = new List<Dish>();
    }

    public class SendOrderResponse : BaseResponse
    {
    }

    public class GetMenuRequest
    {
        [JsonPropertyName("Command")]
        public string Command { get; set; } = "GetMenu";

        [JsonPropertyName("CommandParameters")]
        public GetMenuParameters CommandParameters { get; set; } = new GetMenuParameters();
    }

    public class GetMenuParameters
    {
        [JsonPropertyName("WithPrice")]
        public bool WithPrice { get; set; } = true;
    }

    public class SendOrderRequest
    {
        [JsonPropertyName("Command")]
        public string Command { get; set; } = "SendOrder";

        [JsonPropertyName("CommandParameters")]
        public SendOrderParameters CommandParameters { get; set; } = new SendOrderParameters();
    }

    public class SendOrderParameters
    {
        [JsonPropertyName("OrderId")]
        public string OrderId { get; set; } = string.Empty;

        [JsonPropertyName("MenuItems")]
        public List<OrderRequestItem> MenuItems { get; set; } = new List<OrderRequestItem>();
    }

    public class OrderRequestItem
    {
        [JsonPropertyName("Id")]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("Quantity")]
        public string Quantity { get; set; } = string.Empty;
    }
}
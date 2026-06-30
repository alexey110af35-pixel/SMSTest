using System.Text.Json.Serialization;

namespace SmsTestLibrary.Models
{
	public class GetMenuResponse
	{
		[JsonPropertyName("command")]
		public string Command { get; set; } = string.Empty;

		[JsonPropertyName("success")]
		public bool Success { get; set; }

		[JsonPropertyName("errorMessage")]
		public string ErrorMessage { get; set; } = string.Empty;

		[JsonPropertyName("data")]
		public MenuData Data { get; set; } = new MenuData();
	}

	public class MenuData
	{
		[JsonPropertyName("menuItems")]
		public List<Dish> MenuItems { get; set; } = new List<Dish>();
	}

	public class SendOrderResponse
	{
		[JsonPropertyName("command")]
		public string Command { get; set; } = string.Empty;

		[JsonPropertyName("success")]
		public bool Success { get; set; }

		[JsonPropertyName("errorMessage")]
		public string ErrorMessage { get; set; } = string.Empty;
	}
}
namespace SmsTestConsole.Database
{
	public class DishEntity
	{
		public string Id { get; set; } = string.Empty;
		public string Article { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public decimal Price { get; set; }
		public bool IsWeighted { get; set; }
		public string FullPath { get; set; } = string.Empty;
		public string Barcodes { get; set; } = string.Empty; // Храним как строку с разделителями
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}
}
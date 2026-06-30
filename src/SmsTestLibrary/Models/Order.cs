namespace SmsTestLibrary.Models
{
    public class Order
    {
        public string Id { get; set; } = string.Empty;
        public List<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    }

    public class OrderItem
    {
        public string Id { get; set; } = string.Empty;
        public double Quantity { get; set; }
    }
}
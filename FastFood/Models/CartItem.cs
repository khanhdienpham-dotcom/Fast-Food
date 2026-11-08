namespace FastFood.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }
        public int MaxQuantity { get; set; } // Available stock in DB

        public decimal SubTotal => Price * Quantity;
    }
}

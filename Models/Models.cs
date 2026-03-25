namespace Flawls.API.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Category { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SellingPrice { get; set; }
        public string? ImageUrl { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Variant> Variants { get; set; } = [];
    }

    public class Variant
    {
        public int Id { get; set; }
        public string BarcodeId { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public int Quantity { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public ICollection<StockMovement> StockMovements { get; set; } = [];
    }

    public class StockMovement
    {
        public int Id { get; set; }
        public int VariantId { get; set; }
        public Variant Variant { get; set; } = null!;
        public int Delta { get; set; }
        public string Reason { get; set; } = "scan";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }

    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Role { get; set; } = "staff";
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

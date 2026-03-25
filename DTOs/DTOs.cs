using Flawls.API.Models;

namespace Flawls.API.DTOs
{
    public record LoginRequest(string Username, string Password);
    public record LoginResponse(string Token, string Username, string Role);
    public record CreateUserRequest(string Username, string Password, string Role = "staff");

    public record CreateProductRequest(
        string Name,
        string? Category,
        decimal CostPrice,
        decimal SellingPrice,
        string? ImageUrl,
        string? Notes,
        List<CreateVariantRequest> Variants
    );

    public record UpdateProductRequest(
        string Name,
        string? Category,
        decimal CostPrice,
        decimal SellingPrice,
        string? ImageUrl,
        string? Notes
    );

    public record ProductResponse(
        int Id,
        string Name,
        string? Category,
        decimal CostPrice,
        decimal SellingPrice,
        string? ImageUrl,
        string? Notes,
        DateTime CreatedAt,
        List<VariantResponse> Variants,
        int TotalStock
    );

    public record CreateVariantRequest(
        string Color,
        string Size,
        int InitialQuantity = 0
    );

    public record UpdateVariantRequest(string Color, string Size);

    public record VariantResponse(
        int Id,
        string BarcodeId,
        string Color,
        string Size,
        int Quantity,
        int ProductId,
        string ProductName
    );

    public record AdjustStockRequest(int Delta, string Reason = "manual");

    public record ScanRequest(string BarcodeId);
    public record ScanResponse(bool Success, string Message, VariantResponse? Variant);

    public record StatsResponse(
        int TotalProducts,
        int TotalVariants,
        int TotalUnits,
        decimal TotalStockValue,
        List<LowStockItem> LowStock,
        List<RecentScan> RecentScans
    );

    public record LowStockItem(
         int VariantId, 
         string BarcodeId, 
         string ProductName,
         string Color, 
         string Size, 
         int Quantity
    );

    public record RecentScan(
        string ProductName, 
        string Color, 
        string Size,
        int Delta, 
        string Reason, 
        DateTime ScannedAt
    );
}

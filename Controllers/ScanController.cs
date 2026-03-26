using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Flawls.API.Data;
using Flawls.API.DTOs;
using Flawls.API.Models;

namespace Flawls.API.Controllers;

[ApiController]
[Route("api/scan")]
[Authorize]
public class ScanController(AppDbContext db) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ScanResponse>> Scan(ScanRequest req)
    {
        var variant = await db.Variants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.BarcodeId == req.BarcodeId);

        if (variant is null)
            return Ok(new ScanResponse(false, $"Barcode '{req.BarcodeId}' not found.", null));

        variant.Quantity++;
        db.StockMovements.Add(new StockMovement
        {
            VariantId = variant.Id,
            Delta = 1,
            Reason = "scan"
        });

        await db.SaveChangesAsync();

        return Ok(new ScanResponse(
            true,
            $"{variant.Product.Name} — {variant.Color} / {variant.Size} updated to {variant.Quantity}",
            new VariantResponse(
                variant.Id, variant.BarcodeId, variant.Color,
                variant.Size, variant.Quantity, variant.ProductId, variant.Product.Name)
        ));
    }

    [HttpPost("sell")]
    public async Task<ActionResult<ScanResponse>> Sell(ScanRequest req)
    {
        var variant = await db.Variants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.BarcodeId == req.BarcodeId);

        if (variant is null)
            return Ok(new ScanResponse(false, $"Barcode '{req.BarcodeId}' not found.", null));

        if (variant.Quantity <= 0)
            return Ok(new ScanResponse(false, $"{variant.Product.Name} — {variant.Color} / {variant.Size} is already at 0.", null));

        variant.Quantity--;
        db.StockMovements.Add(new StockMovement
        {
            VariantId = variant.Id,
            Delta = -1,
            Reason = "sell"
        });

        await db.SaveChangesAsync();

        return Ok(new ScanResponse(
            true,
            $"{variant.Product.Name} — {variant.Color} / {variant.Size} sold. {variant.Quantity} remaining.",
            new VariantResponse(
                variant.Id, variant.BarcodeId, variant.Color,
                variant.Size, variant.Quantity, variant.ProductId, variant.Product.Name)
        ));
    }
}
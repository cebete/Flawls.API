using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Flawls.API.Data;
using Flawls.API.DTOs;
using Flawls.API.Models;
using Flawls.API.Services;

namespace Flawls.API.Controllers;

[ApiController]
[Route("api/variants")]
[Authorize]
public class VariantsController(AppDbContext db) : ControllerBase
{
    [HttpPost("product/{productId:int}")]
    public async Task<ActionResult<VariantResponse>> AddVariant(int productId, CreateVariantRequest req)
    {
        var product = await db.Products.FindAsync(productId);
        if (product is null) return NotFound(new { message = "Product not found." });

        string barcodeId;
        do { barcodeId = BarcodeService.Generate(); }
        while (await db.Variants.AnyAsync(v => v.BarcodeId == barcodeId));

        var variant = new Variant
        {
            BarcodeId = barcodeId,
            Color = req.Color.Trim(),
            Size = req.Size.Trim(),
            Quantity = req.InitialQuantity,
            ProductId = productId
        };

        db.Variants.Add(variant);
        await db.SaveChangesAsync();
        return Ok(new VariantResponse(
            variant.Id, variant.BarcodeId, variant.Color,
            variant.Size, variant.Quantity, productId, product.Name));
    }

    [HttpPatch("{id:int}/stock")]
    public async Task<ActionResult> AdjustStock(int id, AdjustStockRequest req)
    {
        var variant = await db.Variants.FindAsync(id);
        if (variant is null) return NotFound();

        variant.Quantity = Math.Max(0, variant.Quantity + req.Delta);
        db.StockMovements.Add(new StockMovement
        {
            VariantId = id,
            Delta = req.Delta,
            Reason = req.Reason
        });

        await db.SaveChangesAsync();
        return Ok(new { variant.Id, variant.Quantity });
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var variant = await db.Variants.FindAsync(id);
        if (variant is null) return NotFound();
        db.Variants.Remove(variant);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
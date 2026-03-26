using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Flawls.API.Data;
using Flawls.API.DTOs;
using Flawls.API.Models;
using Flawls.API.Services;

namespace Flawls.API.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
public class ProductsController(AppDbContext db) : ControllerBase
{
    private static ProductResponse ToResponse(Product p) => new(
        p.Id, p.Name, p.Category, p.CostPrice, p.SellingPrice,
        p.ImageUrl, p.Notes, p.CreatedAt,
        p.Variants.Select(v => new VariantResponse(
            v.Id, v.BarcodeId, v.Color, v.Size, v.Quantity, v.ProductId, p.Name
        )).ToList(),
        p.Variants.Sum(v => v.Quantity),
        p.LowStockThreshold
    );

    [HttpGet]
    public async Task<ActionResult<List<ProductResponse>>> GetAll()
    {
        var products = await db.Products
            .Include(p => p.Variants)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
        return Ok(products.Select(ToResponse));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductResponse>> GetById(int id)
    {
        var p = await db.Products.Include(p => p.Variants).FirstOrDefaultAsync(p => p.Id == id);
        return p is null ? NotFound() : Ok(ToResponse(p));
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponse>> Create(CreateProductRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Name))
            return BadRequest(new { message = "Product name is required." });
        if (!req.Variants.Any())
            return BadRequest(new { message = "At least one variant is required." });

        var product = new Product
        {
            Name = req.Name.Trim(),
            Category = req.Category?.Trim(),
            CostPrice = req.CostPrice,
            SellingPrice = req.SellingPrice,
            LowStockThreshold = req.LowStockThreshold,
            ImageUrl = req.ImageUrl?.Trim(),
            Notes = req.Notes?.Trim()
        };

        foreach (var vReq in req.Variants)
        {
            string barcodeId;
            do { barcodeId = BarcodeService.Generate(); }
            while (await db.Variants.AnyAsync(v => v.BarcodeId == barcodeId));

            product.Variants.Add(new Variant
            {
                BarcodeId = barcodeId,
                Color = vReq.Color.Trim(),
                Size = vReq.Size.Trim(),
                Quantity = vReq.InitialQuantity
            });
        }

        db.Products.Add(product);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, ToResponse(product));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductResponse>> Update(int id, UpdateProductRequest req)
    {
        var p = await db.Products.Include(p => p.Variants).FirstOrDefaultAsync(p => p.Id == id);
        if (p is null) return NotFound();

        p.Name = req.Name.Trim();
        p.Category = req.Category?.Trim();
        p.CostPrice = req.CostPrice;
        p.SellingPrice = req.SellingPrice;
        p.LowStockThreshold = req.LowStockThreshold;
        p.ImageUrl = req.ImageUrl?.Trim();
        p.Notes = req.Notes?.Trim();

        await db.SaveChangesAsync();
        return Ok(ToResponse(p));
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var p = await db.Products.FindAsync(id);
        if (p is null) return NotFound();
        db.Products.Remove(p);
        await db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("{id:int}/duplicate")]
    public async Task<ActionResult<ProductResponse>> Duplicate(int id)
    {
        var source = await db.Products
            .Include(p => p.Variants)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (source is null) return NotFound();

        var copy = new Product
        {
            Name = source.Name + " (copy)",
            Category = source.Category,
            CostPrice = source.CostPrice,
            SellingPrice = source.SellingPrice,
            ImageUrl = source.ImageUrl,
            Notes = source.Notes,
            LowStockThreshold = source.LowStockThreshold,
        };

        foreach (var v in source.Variants)
        {
            string barcodeId;
            do { barcodeId = BarcodeService.Generate(); }
            while (await db.Variants.AnyAsync(x => x.BarcodeId == barcodeId));

            copy.Variants.Add(new Variant
            {
                BarcodeId = barcodeId,
                Color = v.Color,
                Size = v.Size,
                Quantity = 0,
            });
        }

        db.Products.Add(copy);
        await db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = copy.Id }, ToResponse(copy));
    }
}
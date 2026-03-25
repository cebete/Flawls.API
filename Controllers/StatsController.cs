using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Flawls.API.Data;
using Flawls.API.DTOs;

namespace Flawls.API.Controllers;

[ApiController]
[Route("api/stats")]
[Authorize]
public class StatsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<StatsResponse>> Get()
    {
        var products = await db.Products.Include(p => p.Variants).ToListAsync();

        var lowStock = products
            .SelectMany(p => p.Variants.Where(v => v.Quantity <= 3).Select(v =>
                new LowStockItem(v.Id, v.BarcodeId, p.Name, v.Color, v.Size, v.Quantity)))
            .OrderBy(x => x.Quantity)
            .ToList();

        var recentScans = await db.StockMovements
            .Include(m => m.Variant).ThenInclude(v => v.Product)
            .OrderByDescending(m => m.CreatedAt)
            .Take(20)
            .Select(m => new RecentScan(
                m.Variant.Product.Name, m.Variant.Color,
                m.Variant.Size, m.Delta, m.Reason, m.CreatedAt))
            .ToListAsync();

        return Ok(new StatsResponse(
            products.Count,
            products.Sum(p => p.Variants.Count),
            products.Sum(p => p.Variants.Sum(v => v.Quantity)),
            products.Sum(p => p.Variants.Sum(v => v.Quantity * p.CostPrice)),
            lowStock,
            recentScans
        ));
    }
}
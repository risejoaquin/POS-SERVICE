using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosServer.Data;
using PosServer.Models;
using System.Security.Claims;

namespace PosServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly CentralDbContext _context;

    public ProductsController(CentralDbContext context)
    {
        _context = context;
    }

    private string GetTenantId() => User.FindFirstValue("TenantId") ?? string.Empty;

    [HttpGet]
    public async Task<IActionResult> GetProducts()
    {
        var tenantId = GetTenantId();
        var products = await _context.Products
            .Where(p => p.TenantId == tenantId)
            .ToListAsync();
        return Ok(products);
    }

    [HttpGet("changes")]
    public async Task<IActionResult> GetChanges([FromQuery] DateTime since)
    {
        var tenantId = GetTenantId();
        // Since SQLite/Postgres precision could differ, we might need a small buffer or just use >=
        var changedProducts = await _context.Products
            .Where(p => p.TenantId == tenantId && p.LastUpdated >= since)
            .ToListAsync();
        return Ok(changedProducts);
    }

    [HttpPost]
    public async Task<IActionResult> SyncProduct([FromBody] Product product)
    {
        var tenantId = GetTenantId();
        product.TenantId = tenantId;
        
        var existing = await _context.Products.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Barcode == product.Barcode);
        
        if (existing == null)
        {
            // Reset ID to let DB generate it if needed, though for sync we might want to keep UUIDs.
            // Since we use int Id, the local Id might conflict with the server Id.
            // A common approach is matching by Barcode for products.
            product.Id = 0; // Let the server assign ID
            _context.Products.Add(product);
        }
        else
        {
            if (product.LastUpdated > existing.LastUpdated)
            {
                existing.Name = product.Name;
                existing.Price = product.Price;
                existing.StockQuantity = product.StockQuantity;
                existing.LastUpdated = product.LastUpdated;
                _context.Products.Update(existing);
            }
        }

        await _context.SaveChangesAsync();
        return Ok(new { Success = true });
    }
}

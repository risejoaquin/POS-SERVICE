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
        try
        {
            var tenantId = GetTenantId();
            var products = await _context.Products
                .Where(p => p.TenantId == tenantId)
                .ToListAsync();
            return Ok(products);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = ex.Message, Stack = ex.StackTrace, Inner = ex.InnerException?.Message });
        }
    }

    [HttpGet("changes")]
    public async Task<IActionResult> GetChanges([FromQuery] DateTime since)
    {
        try 
        {
            var tenantId = GetTenantId();
            var changedProducts = await _context.Products
                .Where(p => p.TenantId == tenantId && p.LastUpdated >= since)
                .ToListAsync();
            return Ok(changedProducts);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = ex.Message, Stack = ex.StackTrace, Inner = ex.InnerException?.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> SyncProduct([FromBody] Product product)
    {
        try
        {
            var tenantId = GetTenantId();
            product.TenantId = tenantId;
            
            var existing = await _context.Products.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Barcode == product.Barcode);
            
            if (existing == null)
            {
                product.Id = 0;
                _context.Products.Add(product);
            }
            else
            {
                if (product.LastUpdated > existing.LastUpdated)
                {
                    existing.Name = product.Name;
                    existing.Price = product.Price;
                    existing.StockQuantity = product.StockQuantity;
                    existing.Category = product.Category;
                    existing.MinStockThreshold = product.MinStockThreshold;
                    existing.CustomAttributes = product.CustomAttributes;
                    existing.LastUpdated = product.LastUpdated;
                    _context.Products.Update(existing);
                }
            }
            await _context.SaveChangesAsync();
            return Ok(new { Success = true });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { Error = ex.Message, Stack = ex.StackTrace, Inner = ex.InnerException?.Message });
        }
    }
}

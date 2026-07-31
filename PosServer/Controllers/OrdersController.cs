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
public class OrdersController : ControllerBase
{
    private readonly CentralDbContext _context;

    public OrdersController(CentralDbContext context)
    {
        _context = context;
    }

    private string GetTenantId() => User.FindFirstValue("TenantId") ?? string.Empty;

    [HttpPost]
    public async Task<IActionResult> SyncOrder([FromBody] Order order)
    {
        var tenantId = GetTenantId();
        order.TenantId = tenantId;
        
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            foreach (var item in order.Items)
            {
                item.TenantId = tenantId;
                
                // Buscar el producto por código de barras (más confiable que ID generado localmente)
                var product = await _context.Products.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Barcode == item.ProductBarcode);
                
                if (product != null)
                {
                    // Restar stock
                    product.StockQuantity -= item.Quantity;
                    product.LastUpdated = DateTime.UtcNow;
                    _context.Products.Update(product);
                    
                    // Asignar el ID correcto del servidor para la clave foránea
                    item.ProductId = product.Id;
                    item.Product = null!; // Avoid saving duplicate
                }
                else
                {
                    // Fallback si no tiene Barcode, probar con ID
                    var productById = await _context.Products.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == item.ProductId);
                    if (productById != null)
                    {
                        productById.StockQuantity -= item.Quantity;
                        productById.LastUpdated = DateTime.UtcNow;
                        _context.Products.Update(productById);
                        item.Product = null!; // Avoid saving duplicate
                    }
                }
            }

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return Ok(new { Success = true, OrderId = order.Id });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, new { Success = false, Error = ex.Message, Stack = ex.StackTrace, Inner = ex.InnerException?.Message });
        }
    }
}

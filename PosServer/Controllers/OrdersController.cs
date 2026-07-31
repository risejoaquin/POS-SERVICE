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
        if (order == null)
            return BadRequest("La orden no puede ser nula.");

        var tenantId = GetTenantId();
        order.TenantId = tenantId;
        order.LastUpdated = DateTime.UtcNow;
        order.OrderDate = order.OrderDate.ToUniversalTime();

        if (order.Items != null && order.Items.Any())
        {
            foreach (var item in order.Items)
            {
                item.TenantId = tenantId;
                item.LastUpdated = DateTime.UtcNow;
                
                // Desvincular OrderId e Id local para evitar conflictos al insertar en PostgreSQL
                item.OrderId = 0;
                item.Id = 0;
                
                // Buscar el producto por código de barras
                var product = await _context.Products.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Barcode == item.ProductBarcode);
                if (product != null)
                {
                    product.StockQuantity -= item.Quantity;
                    product.LastUpdated = DateTime.UtcNow;
                    _context.Products.Update(product);
                    
                    item.ProductId = product.Id;
                }
                else
                {
                    var productById = await _context.Products.FirstOrDefaultAsync(p => p.TenantId == tenantId && p.Id == item.ProductId);
                    if (productById != null)
                    {
                        productById.StockQuantity -= item.Quantity;
                        productById.LastUpdated = DateTime.UtcNow;
                        _context.Products.Update(productById);
                        
                        item.ProductId = productById.Id;
                    }
                    else
                    {
                        // Si el producto no existe en el servidor (ej. fue creado localmente y la orden llegó primero),
                        // lo creamos usando los datos que vienen en item.Product o valores por defecto.
                        var newProduct = item.Product ?? new Product
                        {
                            TenantId = tenantId,
                            Name = "Producto Desconocido",
                            Barcode = string.IsNullOrEmpty(item.ProductBarcode) ? Guid.NewGuid().ToString() : item.ProductBarcode,
                            Price = item.UnitPrice,
                            StockQuantity = 0,
                            LastUpdated = DateTime.UtcNow
                        };
                        
                        newProduct.Id = 0; // Asegurar que PostgreSQL asigne ID
                        newProduct.TenantId = tenantId;
                        newProduct.StockQuantity -= item.Quantity;
                        newProduct.LastUpdated = DateTime.UtcNow;
                        
                        _context.Products.Add(newProduct);
                        await _context.SaveChangesAsync(); // Guardar para obtener el ID real
                        
                        item.ProductId = newProduct.Id;
                    }
                }
                
                // Evita que Entity Framework intente insertar el producto nuevamente
                // ya que lo procesamos manualmente
                item.Product = null!;
            }
        }
        else
        {
            order.Items = new List<OrderItem>();
        }

        try
        {
            // Reseteamos el ID si viene asignado desde la base de datos local (SQLite)
            // para que PostgreSQL genere su propio AutoIncrement.
            // (La validación de idempotencia por ID local fue removida porque colisionaba 
            // entre diferentes clientes o reinstalaciones).
            order.Id = 0;

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return Ok(new { Success = true, OrderId = order.Id });
        }
        catch (Exception ex)
        {
            Console.WriteLine("ERROR SyncOrder: " + ex.ToString());
            return StatusCode(500, new { message = "Error al guardar orden", error = ex.Message, inner = ex.InnerException?.Message });
        }
    }
}

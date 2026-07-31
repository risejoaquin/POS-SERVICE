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
            return BadRequest("Payload de la orden es nulo.");

        var tenantId = GetTenantId();
        order.TenantId = tenantId;

        // 1. Resetear ID local para que PostgreSQL asigne una clave primaria limpia
        order.Id = 0;
        order.LastUpdated = DateTime.UtcNow;
        order.OrderDate = order.OrderDate.ToUniversalTime();

        // 2. Limpiar e iterar los ítems
        if (order.Items != null && order.Items.Any())
        {
            foreach (var item in order.Items)
            {
                item.TenantId = tenantId;
                item.LastUpdated = DateTime.UtcNow;
                item.Id = 0;       // Resetear ID del ítem
                item.OrderId = 0;  // Desvincular clave foránea local
                
                // Evitar intentar guardar un producto anidado, lo que puede causar
                // conflicto de clave primaria si el producto ya existe o si 
                // ya tiene un ID en la base de datos local que choca en el servidor
                item.Product = null!; 
            }
        }
        else
        {
            order.Items = new List<OrderItem>();
        }

        try
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Orden sincronizada", ServerOrderId = order.Id });
        }
        catch (Exception ex)
        {
            // Retorna el error exacto para depuración en desarrollo/logs
            Console.WriteLine("ERROR SyncOrder: " + ex.ToString());
            return StatusCode(500, new { 
                Error = "Error al guardar la orden en PostgreSQL", 
                Details = ex.Message, 
                Inner = ex.InnerException?.Message 
            });
        }
    }
}

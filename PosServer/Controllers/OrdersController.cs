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

        if (order.Items != null && order.Items.Any())
        {
            foreach (var item in order.Items)
            {
                item.TenantId = tenantId;
                item.LastUpdated = DateTime.UtcNow;
                
                // Desvincular OrderId e Id local para evitar conflictos al insertar en PostgreSQL
                item.OrderId = 0;
                item.Id = 0;
            }
        }
        else
        {
            order.Items = new List<OrderItem>();
        }

        try
        {
            // Evitamos error de duplicados si la orden ya existe por idempotencia
            var existingOrder = await _context.Orders
                .FirstOrDefaultAsync(o => o.TenantId == tenantId && o.Id == order.Id);

            if (existingOrder != null)
            {
                return Ok(new { Success = true, OrderId = existingOrder.Id, Note = "Already synced" }); // Ya fue procesada previamente
            }

            // Reseteamos el ID si viene asignado desde la base de datos local (SQLite)
            // para que PostgreSQL genere su propio AutoIncrement
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

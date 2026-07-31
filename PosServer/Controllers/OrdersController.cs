using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PosServer.Data;
using PosServer.Models;
using PosServer.Services;

namespace PosServer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly CentralDbContext _context;
        private readonly ITenantService _tenantService;

        public OrdersController(CentralDbContext context, ITenantService tenantService)
        {
            _context = context;
            _tenantService = tenantService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateOrder([FromBody] Order order)
        {
            if (order == null)
                return BadRequest("Payload de la orden es nulo.");

            var tenantId = _tenantService.GetTenantId();
            order.TenantId = tenantId;

            // Resetear el ID de la Orden para evitar conflicto de Clave Primaria en PostgreSQL
            order.Id = 0;

            if (order.Items != null && order.Items.Any())
            {
                foreach (var item in order.Items)
                {
                    item.TenantId = tenantId;
                    item.Id = 0;      // Resetear ID del ítem
                    item.OrderId = 0; // Desvincular clave foránea asignada en el SQLite local
                    item.Product = null!; // Avoid detached entity conflicts
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

                return Ok(new { Message = "Orden sincronizada exitosamente", ServerOrderId = order.Id });
            }
            catch (Exception ex)
            {
                Console.WriteLine("ERROR CreateOrder: " + ex.ToString());
                return StatusCode(500, new { 
                    Error = "Error interno al guardar la orden en PostgreSQL", 
                    Details = ex.Message, 
                    InnerError = ex.InnerException?.Message 
                });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetOrders()
        {
            var tenantId = _tenantService.GetTenantId();
            var orders = await _context.Orders
                .Include(o => o.Items)
                .Where(o => o.TenantId == tenantId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var tenantId = _tenantService.GetTenantId();
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id && o.TenantId == tenantId);

            if (order == null)
                return NotFound();

            return Ok(order);
        }
    }
}

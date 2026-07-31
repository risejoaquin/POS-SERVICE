namespace PosServer.Middlewares;

using Microsoft.AspNetCore.Http;
using PosServer.Services;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

public class TenantMiddleware
{
    private readonly RequestDelegate _next;

    public TenantMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
    {
        var tenantId = context.User?.FindFirstValue("TenantId");
        if (string.IsNullOrEmpty(tenantId))
        {
            tenantId = context.Request.Headers["X-Tenant-Id"].FirstOrDefault() ?? string.Empty;
        }
        
        tenantService.SetTenantId(tenantId);
        
        await _next(context);
    }
}

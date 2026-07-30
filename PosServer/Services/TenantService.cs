using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace PosServer.Services;

public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetTenantId()
    {
        return _httpContextAccessor.HttpContext?.User?.FindFirstValue("TenantId") ?? string.Empty;
    }
}

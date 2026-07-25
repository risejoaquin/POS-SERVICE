using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PosServer.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class LicenseController : ControllerBase
{
    [HttpPost("validate")]
    public IActionResult ValidateLicense([FromBody] LicenseRequest request)
    {
        // Simulamos validación de licencia
        if (string.IsNullOrWhiteSpace(request.LicenseKey))
        {
            return BadRequest(new { IsValid = false, Error = "Clave de licencia vacía." });
        }

        // Simulación: Cualquier licencia que empiece con "VAL-" es válida
        if (request.LicenseKey.StartsWith("VAL-"))
        {
            return Ok(new 
            { 
                IsValid = true, 
                MaxTerminals = 5,
                ValidUntil = DateTime.UtcNow.AddYears(1)
            });
        }
        else
        {
            return Ok(new { IsValid = false, Error = "Licencia expirada o inválida." });
        }
    }
}

public class LicenseRequest
{
    public string LicenseKey { get; set; } = string.Empty;
    public string TerminalId { get; set; } = string.Empty;
}

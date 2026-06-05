using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TravelService.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/travel")]
public class HealthController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult GetHealth()
    {
        return Ok(new { service = "TravelService", status = "OK", message = "Travel OK" });
    }
}

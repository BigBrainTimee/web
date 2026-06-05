using Microsoft.AspNetCore.Mvc;

namespace BudgetService.Controllers;

[ApiController]
[Route("api/budget")]
public class HealthController : ControllerBase
{
    [HttpGet("health")]
    public IActionResult GetHealth()
    {
        return Ok(new { service = "BudgetService", status = "OK", message = "Budget OK" });
    }
}

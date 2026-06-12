using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelService.Dtos;
using TravelService.Services;

namespace TravelService.Controllers;

[ApiController]
[Route("api/travel/internal/shared")]
public class InternalShareController : ControllerBase
{
    private readonly ITravelPlanService _travelPlanService;

    public InternalShareController(ITravelPlanService travelPlanService)
    {
        _travelPlanService = travelPlanService;
    }

    [AllowAnonymous]
    [HttpGet("{token}/context")]
    public async Task<ActionResult<SharedPlanContextDto>> GetContext(string token, CancellationToken cancellationToken)
    {
        var context = await _travelPlanService.GetSharedPlanContextAsync(token, cancellationToken);
        return context is null
            ? NotFound(new { message = "Link za deljenje nije pronađen ili je istekao." })
            : Ok(context);
    }
}

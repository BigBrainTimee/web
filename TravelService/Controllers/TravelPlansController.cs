using System.Security.Claims;
using TravelService.Dtos;
using TravelService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TravelService.Controllers;

[ApiController]
[Authorize]
[Route("api/travel/travel-plans")]
public class TravelPlansController : ControllerBase
{
    private readonly ITravelPlanService _travelPlanService;

    public TravelPlansController(ITravelPlanService travelPlanService)
    {
        _travelPlanService = travelPlanService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TravelPlanResponseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var plans = await _travelPlanService.GetAllForUserAsync(userId.Value, cancellationToken);
        return Ok(plans);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TravelPlanResponseDto>> GetById(int id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var plan = await _travelPlanService.GetByIdAsync(userId.Value, id, cancellationToken);
        return plan is null ? NotFound(new { message = "Travel plan not found." }) : Ok(plan);
    }

    [HttpPost]
    public async Task<ActionResult<TravelPlanResponseDto>> Create([FromBody] CreateTravelPlanDto request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var plan = await _travelPlanService.CreateAsync(userId.Value, request, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = plan.Id }, plan);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TravelPlanResponseDto>> Update(int id, [FromBody] UpdateTravelPlanDto request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var plan = await _travelPlanService.UpdateAsync(userId.Value, id, request, cancellationToken);
            return plan is null ? NotFound(new { message = "Travel plan not found." }) : Ok(plan);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var deleted = await _travelPlanService.DeleteAsync(userId.Value, id, cancellationToken);
        return deleted ? NoContent() : NotFound(new { message = "Travel plan not found." });
    }

    [HttpGet("{id:int}/destinations")]
    public async Task<ActionResult<IReadOnlyList<DestinationResponseDto>>> GetDestinations(int id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        if (await _travelPlanService.GetByIdAsync(userId.Value, id, cancellationToken) is null)
        {
            return NotFound(new { message = "Travel plan not found." });
        }

        var destinations = await _travelPlanService.GetDestinationsAsync(userId.Value, id, cancellationToken);
        return Ok(destinations);
    }

    [HttpPost("{id:int}/destinations")]
    public async Task<ActionResult<DestinationResponseDto>> AddDestination(int id, [FromBody] CreateDestinationDto request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var destination = await _travelPlanService.AddDestinationAsync(userId.Value, id, request, cancellationToken);
            return destination is null
                ? NotFound(new { message = "Travel plan not found." })
                : CreatedAtAction(nameof(GetDestinations), new { id }, destination);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/destinations/{destinationId:int}")]
    public async Task<ActionResult<DestinationResponseDto>> UpdateDestination(
        int id,
        int destinationId,
        [FromBody] UpdateDestinationDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var destination = await _travelPlanService.UpdateDestinationAsync(userId.Value, id, destinationId, request, cancellationToken);
            return destination is null
                ? NotFound(new { message = "Destination not found." })
                : Ok(destination);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}/destinations/{destinationId:int}")]
    public async Task<IActionResult> DeleteDestination(int id, int destinationId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var deleted = await _travelPlanService.DeleteDestinationAsync(userId.Value, id, destinationId, cancellationToken);
        return deleted ? NoContent() : NotFound(new { message = "Destination not found." });
    }

    private int? GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var userId) ? userId : null;
    }
}

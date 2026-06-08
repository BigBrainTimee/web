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

    [HttpGet("{id:int}/activities")]
    public async Task<ActionResult<IReadOnlyList<ActivityResponseDto>>> GetActivities(int id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        if (await _travelPlanService.GetByIdAsync(userId.Value, id, cancellationToken) is null)
            return NotFound(new { message = "Travel plan not found." });

        var activities = await _travelPlanService.GetActivitiesAsync(userId.Value, id, cancellationToken);
        return Ok(activities);
    }

    [HttpPost("{id:int}/activities")]
    public async Task<ActionResult<ActivityResponseDto>> AddActivity(int id, [FromBody] CreateActivityDto request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        try
        {
            var activity = await _travelPlanService.AddActivityAsync(userId.Value, id, request, cancellationToken);
            return activity is null
                ? NotFound(new { message = "Travel plan not found." })
                : CreatedAtAction(nameof(GetActivities), new { id }, activity);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:int}/activities/{activityId:int}")]
    public async Task<ActionResult<ActivityResponseDto>> UpdateActivity(int id, int activityId, [FromBody] UpdateActivityDto request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        try
        {
            var activity = await _travelPlanService.UpdateActivityAsync(userId.Value, id, activityId, request, cancellationToken);
            return activity is null
                ? NotFound(new { message = "Activity not found." })
                : Ok(activity);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}/activities/{activityId:int}")]
    public async Task<IActionResult> DeleteActivity(int id, int activityId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var deleted = await _travelPlanService.DeleteActivityAsync(userId.Value, id, activityId, cancellationToken);
        return deleted ? NoContent() : NotFound(new { message = "Activity not found." });
    }

    [HttpGet("{id:int}/checklist-items")]
    public async Task<ActionResult<IReadOnlyList<ChecklistItemResponseDto>>> GetChecklistItems(int id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        if (await _travelPlanService.GetByIdAsync(userId.Value, id, cancellationToken) is null)
            return NotFound(new { message = "Travel plan not found." });

        var items = await _travelPlanService.GetChecklistItemsAsync(userId.Value, id, cancellationToken);
        return Ok(items);
    }

    [HttpPost("{id:int}/checklist-items")]
    public async Task<ActionResult<ChecklistItemResponseDto>> AddChecklistItem(int id, [FromBody] CreateChecklistItemDto request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var item = await _travelPlanService.AddChecklistItemAsync(userId.Value, id, request, cancellationToken);
        return item is null
            ? NotFound(new { message = "Travel plan not found." })
            : CreatedAtAction(nameof(GetChecklistItems), new { id }, item);
    }

    [HttpPut("{id:int}/checklist-items/{itemId:int}")]
    public async Task<ActionResult<ChecklistItemResponseDto>> UpdateChecklistItem(int id, int itemId, [FromBody] UpdateChecklistItemDto request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var item = await _travelPlanService.UpdateChecklistItemAsync(userId.Value, id, itemId, request, cancellationToken);
        return item is null
            ? NotFound(new { message = "Checklist item not found." })
            : Ok(item);
    }

    [HttpPatch("{id:int}/checklist-items/{itemId:int}/toggle")]
    public async Task<ActionResult<ChecklistItemResponseDto>> ToggleChecklistItem(int id, int itemId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var item = await _travelPlanService.ToggleChecklistItemAsync(userId.Value, id, itemId, cancellationToken);
        return item is null
            ? NotFound(new { message = "Checklist item not found." })
            : Ok(item);
    }

    [HttpDelete("{id:int}/checklist-items/{itemId:int}")]
    public async Task<IActionResult> DeleteChecklistItem(int id, int itemId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var deleted = await _travelPlanService.DeleteChecklistItemAsync(userId.Value, id, itemId, cancellationToken);
        return deleted ? NoContent() : NotFound(new { message = "Checklist item not found." });
    }

    [HttpGet("{id:int}/share-links")]
    public async Task<ActionResult<IReadOnlyList<ShareLinkResponseDto>>> GetShareLinks(int id, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        if (await _travelPlanService.GetByIdAsync(userId.Value, id, cancellationToken) is null)
            return NotFound(new { message = "Travel plan not found." });

        var links = await _travelPlanService.GetShareLinksAsync(userId.Value, id, cancellationToken);
        return Ok(links);
    }

    [HttpPost("{id:int}/share-links")]
    public async Task<ActionResult<ShareLinkResponseDto>> CreateShareLink(int id, [FromBody] CreateShareLinkDto request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        try
        {
            var link = await _travelPlanService.CreateShareLinkAsync(userId.Value, id, request, cancellationToken);
            return link is null
                ? NotFound(new { message = "Travel plan not found." })
                : CreatedAtAction(nameof(GetShareLinks), new { id }, link);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}/share-links/{linkId:int}")]
    public async Task<IActionResult> DeleteShareLink(int id, int linkId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null) return Unauthorized();

        var deleted = await _travelPlanService.DeleteShareLinkAsync(userId.Value, id, linkId, cancellationToken);
        return deleted ? NoContent() : NotFound(new { message = "Share link not found." });
    }

    private int? GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var userId) ? userId : null;
    }
}

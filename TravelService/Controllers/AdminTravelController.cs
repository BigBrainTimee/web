using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelService.Dtos;
using TravelService.Services;

namespace TravelService.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/travel/admin/users/{userId:int}")]
public class AdminTravelController : ControllerBase
{
    private readonly ITravelPlanService _travelPlanService;
    private readonly IPlanReportPdfGenerator _planReportPdfGenerator;

    public AdminTravelController(
        ITravelPlanService travelPlanService,
        IPlanReportPdfGenerator planReportPdfGenerator)
    {
        _travelPlanService = travelPlanService;
        _planReportPdfGenerator = planReportPdfGenerator;
    }

    [HttpGet("travel-plans")]
    public async Task<ActionResult<IReadOnlyList<TravelPlanResponseDto>>> GetTravelPlans(
        int userId,
        CancellationToken cancellationToken)
    {
        var plans = await _travelPlanService.GetAllForUserAsync(userId, cancellationToken);
        return Ok(plans);
    }

    [HttpPost("travel-plans")]
    public async Task<ActionResult<TravelPlanResponseDto>> CreateTravelPlan(
        int userId,
        [FromBody] CreateTravelPlanDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var plan = await _travelPlanService.CreateAsync(userId, request, cancellationToken);
            return CreatedAtAction(nameof(GetTravelPlan), new { userId, planId = plan.Id }, plan);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("travel-plans/{planId:int}")]
    public async Task<ActionResult<TravelPlanResponseDto>> GetTravelPlan(
        int userId,
        int planId,
        CancellationToken cancellationToken)
    {
        var plan = await _travelPlanService.GetByIdAsync(userId, planId, cancellationToken);
        return plan is null
            ? NotFound(new { message = "Travel plan not found." })
            : Ok(plan);
    }

    [HttpPut("travel-plans/{planId:int}")]
    public async Task<ActionResult<TravelPlanResponseDto>> UpdateTravelPlan(
        int userId,
        int planId,
        [FromBody] UpdateTravelPlanDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var plan = await _travelPlanService.UpdateAsync(userId, planId, request, cancellationToken);
            return plan is null ? NotFound(new { message = "Travel plan not found." }) : Ok(plan);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("travel-plans/{planId:int}")]
    public async Task<IActionResult> DeleteTravelPlan(
        int userId,
        int planId,
        CancellationToken cancellationToken)
    {
        var deleted = await _travelPlanService.DeleteAsync(userId, planId, cancellationToken);
        return deleted ? NoContent() : NotFound(new { message = "Travel plan not found." });
    }

    [HttpGet("travel-plans/{planId:int}/report/pdf")]
    public async Task<IActionResult> DownloadReport(int userId, int planId, CancellationToken cancellationToken)
    {
        var report = await _travelPlanService.GetPlanReportAsync(userId, planId, cancellationToken);
        if (report is null)
        {
            return NotFound(new { message = "Travel plan not found." });
        }

        var pdfBytes = _planReportPdfGenerator.Generate(report);
        return File(pdfBytes, "application/pdf", $"plan-{planId}-izvestaj.pdf");
    }

    [HttpGet("travel-plans/{planId:int}/destinations")]
    public async Task<ActionResult<IReadOnlyList<DestinationResponseDto>>> GetDestinations(
        int userId,
        int planId,
        CancellationToken cancellationToken)
    {
        if (await _travelPlanService.GetByIdAsync(userId, planId, cancellationToken) is null)
        {
            return NotFound(new { message = "Travel plan not found." });
        }

        var destinations = await _travelPlanService.GetDestinationsAsync(userId, planId, cancellationToken);
        return Ok(destinations);
    }

    [HttpPost("travel-plans/{planId:int}/destinations")]
    public async Task<ActionResult<DestinationResponseDto>> AddDestination(
        int userId,
        int planId,
        [FromBody] CreateDestinationDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var destination = await _travelPlanService.AddDestinationAsync(userId, planId, request, cancellationToken);
            return destination is null
                ? NotFound(new { message = "Travel plan not found." })
                : Ok(destination);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("travel-plans/{planId:int}/destinations/{destinationId:int}")]
    public async Task<ActionResult<DestinationResponseDto>> UpdateDestination(
        int userId,
        int planId,
        int destinationId,
        [FromBody] UpdateDestinationDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var destination = await _travelPlanService.UpdateDestinationAsync(
                userId,
                planId,
                destinationId,
                request,
                cancellationToken);

            return destination is null
                ? NotFound(new { message = "Destination not found." })
                : Ok(destination);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("travel-plans/{planId:int}/destinations/{destinationId:int}")]
    public async Task<IActionResult> DeleteDestination(
        int userId,
        int planId,
        int destinationId,
        CancellationToken cancellationToken)
    {
        var deleted = await _travelPlanService.DeleteDestinationAsync(userId, planId, destinationId, cancellationToken);
        return deleted ? NoContent() : NotFound(new { message = "Destination not found." });
    }

    [HttpGet("travel-plans/{planId:int}/activities")]
    public async Task<ActionResult<IReadOnlyList<ActivityResponseDto>>> GetActivities(
        int userId,
        int planId,
        CancellationToken cancellationToken)
    {
        if (await _travelPlanService.GetByIdAsync(userId, planId, cancellationToken) is null)
        {
            return NotFound(new { message = "Travel plan not found." });
        }

        var activities = await _travelPlanService.GetActivitiesAsync(userId, planId, cancellationToken);
        return Ok(activities);
    }

    [HttpPost("travel-plans/{planId:int}/activities")]
    public async Task<ActionResult<ActivityResponseDto>> AddActivity(
        int userId,
        int planId,
        [FromBody] CreateActivityDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var activity = await _travelPlanService.AddActivityAsync(userId, planId, request, cancellationToken);
            return activity is null
                ? NotFound(new { message = "Travel plan not found." })
                : Ok(activity);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("travel-plans/{planId:int}/activities/{activityId:int}")]
    public async Task<ActionResult<ActivityResponseDto>> UpdateActivity(
        int userId,
        int planId,
        int activityId,
        [FromBody] UpdateActivityDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var activity = await _travelPlanService.UpdateActivityAsync(userId, planId, activityId, request, cancellationToken);
            return activity is null
                ? NotFound(new { message = "Activity not found." })
                : Ok(activity);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("travel-plans/{planId:int}/activities/{activityId:int}")]
    public async Task<IActionResult> DeleteActivity(
        int userId,
        int planId,
        int activityId,
        CancellationToken cancellationToken)
    {
        var deleted = await _travelPlanService.DeleteActivityAsync(userId, planId, activityId, cancellationToken);
        return deleted ? NoContent() : NotFound(new { message = "Activity not found." });
    }

    [HttpGet("travel-plans/{planId:int}/checklist-items")]
    public async Task<ActionResult<IReadOnlyList<ChecklistItemResponseDto>>> GetChecklistItems(
        int userId,
        int planId,
        CancellationToken cancellationToken)
    {
        if (await _travelPlanService.GetByIdAsync(userId, planId, cancellationToken) is null)
        {
            return NotFound(new { message = "Travel plan not found." });
        }

        var items = await _travelPlanService.GetChecklistItemsAsync(userId, planId, cancellationToken);
        return Ok(items);
    }

    [HttpPost("travel-plans/{planId:int}/checklist-items")]
    public async Task<ActionResult<ChecklistItemResponseDto>> AddChecklistItem(
        int userId,
        int planId,
        [FromBody] CreateChecklistItemDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var item = await _travelPlanService.AddChecklistItemAsync(userId, planId, request, cancellationToken);
        return item is null
            ? NotFound(new { message = "Travel plan not found." })
            : Ok(item);
    }

    [HttpPut("travel-plans/{planId:int}/checklist-items/{itemId:int}")]
    public async Task<ActionResult<ChecklistItemResponseDto>> UpdateChecklistItem(
        int userId,
        int planId,
        int itemId,
        [FromBody] UpdateChecklistItemDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        var item = await _travelPlanService.UpdateChecklistItemAsync(userId, planId, itemId, request, cancellationToken);
        return item is null
            ? NotFound(new { message = "Checklist item not found." })
            : Ok(item);
    }

    [HttpPatch("travel-plans/{planId:int}/checklist-items/{itemId:int}/toggle")]
    public async Task<ActionResult<ChecklistItemResponseDto>> ToggleChecklistItem(
        int userId,
        int planId,
        int itemId,
        CancellationToken cancellationToken)
    {
        var item = await _travelPlanService.ToggleChecklistItemAsync(userId, planId, itemId, cancellationToken);
        return item is null
            ? NotFound(new { message = "Checklist item not found." })
            : Ok(item);
    }

    [HttpDelete("travel-plans/{planId:int}/checklist-items/{itemId:int}")]
    public async Task<IActionResult> DeleteChecklistItem(
        int userId,
        int planId,
        int itemId,
        CancellationToken cancellationToken)
    {
        var deleted = await _travelPlanService.DeleteChecklistItemAsync(userId, planId, itemId, cancellationToken);
        return deleted ? NoContent() : NotFound(new { message = "Checklist item not found." });
    }

    [HttpGet("travel-plans/{planId:int}/share-links")]
    public async Task<ActionResult<IReadOnlyList<ShareLinkResponseDto>>> GetShareLinks(
        int userId,
        int planId,
        CancellationToken cancellationToken)
    {
        if (await _travelPlanService.GetByIdAsync(userId, planId, cancellationToken) is null)
        {
            return NotFound(new { message = "Travel plan not found." });
        }

        var links = await _travelPlanService.GetShareLinksAsync(userId, planId, cancellationToken);
        return Ok(links);
    }

    [HttpPost("travel-plans/{planId:int}/share-links")]
    public async Task<ActionResult<ShareLinkResponseDto>> CreateShareLink(
        int userId,
        int planId,
        [FromBody] CreateShareLinkDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var link = await _travelPlanService.CreateShareLinkAsync(userId, planId, request, cancellationToken);
            return link is null
                ? NotFound(new { message = "Travel plan not found." })
                : Ok(link);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("travel-plans/{planId:int}/share-links/{linkId:int}")]
    public async Task<IActionResult> DeleteShareLink(
        int userId,
        int planId,
        int linkId,
        CancellationToken cancellationToken)
    {
        var deleted = await _travelPlanService.DeleteShareLinkAsync(userId, planId, linkId, cancellationToken);
        return deleted ? NoContent() : NotFound(new { message = "Share link not found." });
    }
}

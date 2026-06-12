using TravelService.Dtos;

using TravelService.Services;

using Microsoft.AspNetCore.Authorization;

using Microsoft.AspNetCore.Mvc;



namespace TravelService.Controllers;



[ApiController]
[Route("api/travel/shared/{token}")]

public class SharedPlansController : ControllerBase

{

    private readonly ITravelPlanService _travelPlanService;



    public SharedPlansController(ITravelPlanService travelPlanService)

    {

        _travelPlanService = travelPlanService;

    }



    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<SharedPlanResponseDto>> GetByToken(string token, CancellationToken cancellationToken)
    {
        var plan = await _travelPlanService.GetSharedPlanAsync(token, cancellationToken);

        if (plan is null)
        {
            return NotFound(new { message = "Link za deljenje nije pronađen ili je istekao." });
        }

        if (plan.CanEdit && !(User.Identity?.IsAuthenticated ?? false))
        {
            return StatusCode(StatusCodes.Status403Forbidden,
                new { message = "Morate biti prijavljeni da biste koristili link za izmenu." });
        }

        return Ok(plan);
    }



    [Authorize]
    [HttpPost("destinations")]

    public async Task<ActionResult<DestinationResponseDto>> AddDestination(

        string token,

        [FromBody] CreateDestinationDto request,

        CancellationToken cancellationToken)

    {

        if (!ModelState.IsValid) return ValidationProblem(ModelState);



        try

        {

            var destination = await _travelPlanService.AddSharedDestinationAsync(token, request, cancellationToken);

            return destination is null

                ? NotFound(new { message = "Share link not found, expired, or read-only." })

                : Ok(destination);

        }

        catch (ArgumentException ex)

        {

            return BadRequest(new { message = ex.Message });

        }

    }



    [Authorize]
    [HttpPut("destinations/{destinationId:int}")]

    public async Task<ActionResult<DestinationResponseDto>> UpdateDestination(

        string token,

        int destinationId,

        [FromBody] UpdateDestinationDto request,

        CancellationToken cancellationToken)

    {

        if (!ModelState.IsValid) return ValidationProblem(ModelState);



        try

        {

            var destination = await _travelPlanService.UpdateSharedDestinationAsync(token, destinationId, request, cancellationToken);

            return destination is null

                ? NotFound(new { message = "Share link not found, expired, or read-only." })

                : Ok(destination);

        }

        catch (ArgumentException ex)

        {

            return BadRequest(new { message = ex.Message });

        }

    }



    [Authorize]
    [HttpDelete("destinations/{destinationId:int}")]

    public async Task<IActionResult> DeleteDestination(string token, int destinationId, CancellationToken cancellationToken)

    {

        var deleted = await _travelPlanService.DeleteSharedDestinationAsync(token, destinationId, cancellationToken);

        return deleted

            ? NoContent()

            : NotFound(new { message = "Share link not found, expired, or read-only." });

    }



    [Authorize]
    [HttpPost("activities")]

    public async Task<ActionResult<ActivityResponseDto>> AddActivity(

        string token,

        [FromBody] CreateActivityDto request,

        CancellationToken cancellationToken)

    {

        if (!ModelState.IsValid) return ValidationProblem(ModelState);



        try

        {

            var activity = await _travelPlanService.AddSharedActivityAsync(token, request, cancellationToken);

            return activity is null

                ? NotFound(new { message = "Share link not found, expired, or read-only." })

                : Ok(activity);

        }

        catch (ArgumentException ex)

        {

            return BadRequest(new { message = ex.Message });

        }

    }



    [Authorize]
    [HttpPut("activities/{activityId:int}")]

    public async Task<ActionResult<ActivityResponseDto>> UpdateActivity(

        string token,

        int activityId,

        [FromBody] UpdateActivityDto request,

        CancellationToken cancellationToken)

    {

        if (!ModelState.IsValid) return ValidationProblem(ModelState);



        try

        {

            var activity = await _travelPlanService.UpdateSharedActivityAsync(token, activityId, request, cancellationToken);

            return activity is null

                ? NotFound(new { message = "Share link not found, expired, or read-only." })

                : Ok(activity);

        }

        catch (ArgumentException ex)

        {

            return BadRequest(new { message = ex.Message });

        }

    }



    [Authorize]
    [HttpDelete("activities/{activityId:int}")]

    public async Task<IActionResult> DeleteActivity(string token, int activityId, CancellationToken cancellationToken)

    {

        var deleted = await _travelPlanService.DeleteSharedActivityAsync(token, activityId, cancellationToken);

        return deleted

            ? NoContent()

            : NotFound(new { message = "Share link not found, expired, or read-only." });

    }



    [Authorize]
    [HttpPost("expenses")]

    public async Task<ActionResult<ExpenseResponseDto>> AddExpense(

        string token,

        [FromBody] CreateExpenseDto request,

        CancellationToken cancellationToken)

    {

        if (!ModelState.IsValid) return ValidationProblem(ModelState);



        try

        {

            var expense = await _travelPlanService.AddSharedExpenseAsync(token, request, cancellationToken);

            return expense is null

                ? NotFound(new { message = "Share link not found, expired, or read-only." })

                : Ok(expense);

        }

        catch (ArgumentException ex)

        {

            return BadRequest(new { message = ex.Message });

        }

    }



    [Authorize]
    [HttpDelete("expenses/{expenseId:int}")]

    public async Task<IActionResult> DeleteExpense(string token, int expenseId, CancellationToken cancellationToken)

    {

        var deleted = await _travelPlanService.DeleteSharedExpenseAsync(token, expenseId, cancellationToken);

        return deleted

            ? NoContent()

            : NotFound(new { message = "Share link not found, expired, or read-only." });

    }



    [Authorize]
    [HttpPost("checklist-items")]

    public async Task<ActionResult<ChecklistItemResponseDto>> AddChecklistItem(

        string token,

        [FromBody] CreateChecklistItemDto request,

        CancellationToken cancellationToken)

    {

        if (!ModelState.IsValid)

        {

            return ValidationProblem(ModelState);

        }



        var item = await _travelPlanService.AddSharedChecklistItemAsync(token, request, cancellationToken);

        return item is null

            ? NotFound(new { message = "Share link not found, expired, or read-only." })

            : Ok(item);

    }



    [Authorize]
    [HttpPatch("checklist-items/{itemId:int}/toggle")]

    public async Task<ActionResult<ChecklistItemResponseDto>> ToggleChecklistItem(

        string token,

        int itemId,

        CancellationToken cancellationToken)

    {

        var item = await _travelPlanService.ToggleSharedChecklistItemAsync(token, itemId, cancellationToken);

        return item is null

            ? NotFound(new { message = "Share link not found, expired, or read-only." })

            : Ok(item);

    }



    [Authorize]
    [HttpDelete("checklist-items/{itemId:int}")]

    public async Task<IActionResult> DeleteChecklistItem(string token, int itemId, CancellationToken cancellationToken)

    {

        var deleted = await _travelPlanService.DeleteSharedChecklistItemAsync(token, itemId, cancellationToken);

        return deleted

            ? NoContent()

            : NotFound(new { message = "Share link not found, expired, or read-only." });

    }

}



using System.Security.Claims;
using BudgetService.Dtos;
using BudgetService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetService.Controllers;

[ApiController]
[Authorize]
[Route("api/budget/travel-plans/{planId:int}")]
public class ExpensesController : ControllerBase
{
    private readonly IBudgetService _budgetService;

    public ExpensesController(IBudgetService budgetService)
    {
        _budgetService = budgetService;
    }

    [HttpGet("expenses")]
    public async Task<ActionResult<IReadOnlyList<ExpenseResponseDto>>> GetAll(int planId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var expenses = await _budgetService.GetExpensesAsync(userId.Value, planId, cancellationToken);
        return Ok(expenses);
    }

    [HttpPost("expenses")]
    public async Task<ActionResult<ExpenseResponseDto>> Create(int planId, [FromBody] CreateExpenseDto request, CancellationToken cancellationToken)
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
            var expense = await _budgetService.AddExpenseAsync(userId.Value, planId, request, cancellationToken);
            return expense is null
                ? NotFound(new { message = "Travel plan not found." })
                : CreatedAtAction(nameof(GetAll), new { planId }, expense);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("expenses/{expenseId:int}")]
    public async Task<ActionResult<ExpenseResponseDto>> Update(int planId, int expenseId, [FromBody] UpdateExpenseDto request, CancellationToken cancellationToken)
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
            var expense = await _budgetService.UpdateExpenseAsync(userId.Value, planId, expenseId, request, cancellationToken);
            return expense is null ? NotFound(new { message = "Expense not found." }) : Ok(expense);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("expenses/{expenseId:int}")]
    public async Task<IActionResult> Delete(int planId, int expenseId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var deleted = await _budgetService.DeleteExpenseAsync(userId.Value, planId, expenseId, cancellationToken);
        return deleted ? NoContent() : NotFound(new { message = "Expense not found." });
    }

    [HttpGet("summary")]
    public async Task<ActionResult<BudgetSummaryDto>> GetSummary(int planId, CancellationToken cancellationToken)
    {
        var userId = GetUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var summary = await _budgetService.GetSummaryAsync(userId.Value, planId, cancellationToken);
        return summary is null ? NotFound(new { message = "Travel plan not found." }) : Ok(summary);
    }

    private int? GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var userId) ? userId : null;
    }
}

using BudgetService.Dtos;
using BudgetService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetService.Controllers;

[ApiController]
[Route("api/budget/shared/{token}")]
public class SharedBudgetController : ControllerBase
{
    private readonly IBudgetService _budgetService;

    public SharedBudgetController(IBudgetService budgetService)
    {
        _budgetService = budgetService;
    }

    [AllowAnonymous]
    [HttpGet("summary")]
    public async Task<ActionResult<BudgetSummaryDto>> GetSummary(string token, CancellationToken cancellationToken)
    {
        var summary = await _budgetService.GetSharedSummaryAsync(token, cancellationToken);
        return summary is null
            ? NotFound(new { message = "Share link not found or expired." })
            : Ok(summary);
    }

    [AllowAnonymous]
    [HttpGet("expenses")]
    public async Task<ActionResult<IReadOnlyList<ExpenseResponseDto>>> GetExpenses(string token, CancellationToken cancellationToken)
    {
        var expenses = await _budgetService.GetSharedExpensesAsync(token, cancellationToken);
        return expenses is null
            ? NotFound(new { message = "Share link not found or expired." })
            : Ok(expenses);
    }

    [Authorize]
    [HttpPost("expenses")]
    public async Task<ActionResult<ExpenseResponseDto>> AddExpense(
        string token,
        [FromBody] CreateExpenseDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var expense = await _budgetService.AddSharedExpenseAsync(token, request, cancellationToken);
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
    [HttpPut("expenses/{expenseId:int}")]
    public async Task<ActionResult<ExpenseResponseDto>> UpdateExpense(
        string token,
        int expenseId,
        [FromBody] UpdateExpenseDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var expense = await _budgetService.UpdateSharedExpenseAsync(token, expenseId, request, cancellationToken);
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
        var deleted = await _budgetService.DeleteSharedExpenseAsync(token, expenseId, cancellationToken);
        return deleted
            ? NoContent()
            : NotFound(new { message = "Share link not found, expired, or read-only." });
    }
}

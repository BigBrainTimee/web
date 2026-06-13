using BudgetService.Dtos;
using BudgetService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetService.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/budget/admin/users/{userId:int}/travel-plans/{planId:int}")]
public class AdminBudgetController : ControllerBase
{
    private readonly IBudgetService _budgetService;

    public AdminBudgetController(IBudgetService budgetService)
    {
        _budgetService = budgetService;
    }

    [HttpGet("expenses")]
    public async Task<ActionResult<IReadOnlyList<ExpenseResponseDto>>> GetExpenses(
        int userId,
        int planId,
        CancellationToken cancellationToken)
    {
        var expenses = await _budgetService.GetAdminExpensesAsync(userId, planId, cancellationToken);
        return Ok(expenses);
    }

    [HttpPost("expenses")]
    public async Task<ActionResult<ExpenseResponseDto>> AddExpense(
        int userId,
        int planId,
        [FromBody] CreateExpenseDto request,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return ValidationProblem(ModelState);
        }

        try
        {
            var expense = await _budgetService.AddAdminExpenseAsync(userId, planId, request, cancellationToken);
            return expense is null
                ? NotFound(new { message = "Travel plan not found." })
                : Ok(expense);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("expenses/{expenseId:int}")]
    public async Task<ActionResult<ExpenseResponseDto>> UpdateExpense(
        int userId,
        int planId,
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
            var expense = await _budgetService.UpdateAdminExpenseAsync(userId, planId, expenseId, request, cancellationToken);
            return expense is null ? NotFound(new { message = "Expense not found." }) : Ok(expense);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("expenses/{expenseId:int}")]
    public async Task<IActionResult> DeleteExpense(
        int userId,
        int planId,
        int expenseId,
        CancellationToken cancellationToken)
    {
        var deleted = await _budgetService.DeleteAdminExpenseAsync(userId, planId, expenseId, cancellationToken);
        return deleted ? NoContent() : NotFound(new { message = "Expense not found." });
    }

    [HttpGet("summary")]
    public async Task<ActionResult<BudgetSummaryDto>> GetSummary(
        int userId,
        int planId,
        CancellationToken cancellationToken)
    {
        var summary = await _budgetService.GetAdminSummaryAsync(userId, planId, cancellationToken);
        return summary is null ? NotFound(new { message = "Travel plan not found." }) : Ok(summary);
    }
}

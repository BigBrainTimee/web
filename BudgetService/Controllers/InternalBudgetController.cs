using BudgetService.Services;
using BudgetService.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetService.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/budget/internal/travel-plans/{planId:int}")]
public class InternalBudgetController : ControllerBase
{
    private readonly IBudgetService _budgetService;

    public InternalBudgetController(IBudgetService budgetService)
    {
        _budgetService = budgetService;
    }

    [HttpGet("expenses")]
    public async Task<ActionResult<IReadOnlyList<ExpenseResponseDto>>> GetExpenses(
        int planId,
        CancellationToken cancellationToken)
    {
        var expenses = await _budgetService.GetExpensesByPlanIdAsync(planId, cancellationToken);
        return Ok(expenses);
    }
}

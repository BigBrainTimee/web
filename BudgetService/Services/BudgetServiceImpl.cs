using BudgetService.Data;
using BudgetService.Dtos;
using BudgetService.Mappers;
using Microsoft.EntityFrameworkCore;

namespace BudgetService.Services;

public class BudgetServiceImpl : IBudgetService
{
    private readonly BudgetDbContext _dbContext;

    public BudgetServiceImpl(BudgetDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<ExpenseResponseDto>> GetExpensesAsync(int userId, int planId, CancellationToken cancellationToken = default)
    {
        if (!await PlanExistsForUserAsync(userId, planId, cancellationToken))
        {
            return Array.Empty<ExpenseResponseDto>();
        }

        var expenses = await _dbContext.Expenses
            .AsNoTracking()
            .Where(e => e.TravelPlanId == planId)
            .OrderByDescending(e => e.ExpenseDate)
            .ThenBy(e => e.Id)
            .ToListAsync(cancellationToken);

        return expenses.Select(ExpenseMapper.ToResponseDto).ToList();
    }

    public async Task<ExpenseResponseDto?> AddExpenseAsync(int userId, int planId, CreateExpenseDto dto, CancellationToken cancellationToken = default)
    {
        if (!await PlanExistsForUserAsync(userId, planId, cancellationToken))
        {
            return null;
        }

        var expense = ExpenseMapper.ToEntity(dto, planId);
        _dbContext.Expenses.Add(expense);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ExpenseMapper.ToResponseDto(expense);
    }

    public async Task<ExpenseResponseDto?> UpdateExpenseAsync(int userId, int planId, int expenseId, UpdateExpenseDto dto, CancellationToken cancellationToken = default)
    {
        var expense = await GetOwnedExpenseAsync(userId, planId, expenseId, cancellationToken);
        if (expense is null)
        {
            return null;
        }

        ExpenseMapper.ApplyUpdate(expense, dto);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ExpenseMapper.ToResponseDto(expense);
    }

    public async Task<bool> DeleteExpenseAsync(int userId, int planId, int expenseId, CancellationToken cancellationToken = default)
    {
        var expense = await GetOwnedExpenseAsync(userId, planId, expenseId, cancellationToken);
        if (expense is null)
        {
            return false;
        }

        _dbContext.Expenses.Remove(expense);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<BudgetSummaryDto?> GetSummaryAsync(int userId, int planId, CancellationToken cancellationToken = default)
    {
        var plan = await _dbContext.TravelPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == planId && p.UserId == userId, cancellationToken);

        if (plan is null)
        {
            return null;
        }

        var byCategory = await _dbContext.Expenses
            .AsNoTracking()
            .Where(e => e.TravelPlanId == planId)
            .GroupBy(e => e.Category)
            .Select(g => new CategorySummaryDto
            {
                Category = g.Key,
                Amount = g.Sum(e => e.Amount)
            })
            .OrderBy(c => c.Category)
            .ToListAsync(cancellationToken);

        var totalSpent = byCategory.Sum(c => c.Amount);

        return new BudgetSummaryDto
        {
            TravelPlanId = planId,
            PlannedBudget = plan.PlannedBudget,
            TotalSpent = totalSpent,
            Remaining = plan.PlannedBudget - totalSpent,
            ByCategory = byCategory
        };
    }

    private async Task<bool> PlanExistsForUserAsync(int userId, int planId, CancellationToken cancellationToken)
    {
        return await _dbContext.TravelPlans
            .AsNoTracking()
            .AnyAsync(p => p.Id == planId && p.UserId == userId, cancellationToken);
    }

    private async Task<Models.Expense?> GetOwnedExpenseAsync(
        int userId,
        int planId,
        int expenseId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Expenses
            .Include(e => e.TravelPlan)
            .FirstOrDefaultAsync(
                e => e.Id == expenseId
                    && e.TravelPlanId == planId
                    && e.TravelPlan != null
                    && e.TravelPlan.UserId == userId,
                cancellationToken);
    }
}

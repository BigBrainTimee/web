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
        var plan = await GetOwnedPlanAsync(userId, planId, cancellationToken);
        if (plan is null)
        {
            return null;
        }

        ValidateExpenseDate(dto.ExpenseDate, plan.StartDate, plan.EndDate);

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

        ValidateExpenseDate(dto.ExpenseDate, expense.TravelPlan!.StartDate, expense.TravelPlan.EndDate);

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

        return await BuildSummaryAsync(planId, cancellationToken);
    }

    public async Task<BudgetSummaryDto?> GetSharedSummaryAsync(string token, CancellationToken cancellationToken = default)
    {
        var context = await GetValidShareContextAsync(token, cancellationToken);
        if (context is null)
        {
            return null;
        }

        return await BuildSummaryAsync(context.Value, cancellationToken);
    }

    public async Task<IReadOnlyList<ExpenseResponseDto>?> GetSharedExpensesAsync(string token, CancellationToken cancellationToken = default)
    {
        var context = await GetValidShareContextAsync(token, cancellationToken);
        if (context is null)
        {
            return null;
        }

        var expenses = await _dbContext.Expenses
            .AsNoTracking()
            .Where(e => e.TravelPlanId == context.Value)
            .OrderByDescending(e => e.ExpenseDate)
            .ThenBy(e => e.Id)
            .ToListAsync(cancellationToken);

        return expenses.Select(ExpenseMapper.ToResponseDto).ToList();
    }

    public async Task<ExpenseResponseDto?> AddSharedExpenseAsync(string token, CreateExpenseDto dto, CancellationToken cancellationToken = default)
    {
        var context = await GetValidShareContextAsync(token, cancellationToken, requireEdit: true);
        if (context is null)
        {
            return null;
        }

        var plan = await _dbContext.TravelPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == context.Value, cancellationToken);

        if (plan is null)
        {
            return null;
        }

        ValidateExpenseDate(dto.ExpenseDate, plan.StartDate, plan.EndDate);

        var expense = ExpenseMapper.ToEntity(dto, context.Value);
        _dbContext.Expenses.Add(expense);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ExpenseMapper.ToResponseDto(expense);
    }

    public async Task<ExpenseResponseDto?> UpdateSharedExpenseAsync(
        string token,
        int expenseId,
        UpdateExpenseDto dto,
        CancellationToken cancellationToken = default)
    {
        var context = await GetValidShareContextAsync(token, cancellationToken, requireEdit: true);
        if (context is null)
        {
            return null;
        }

        var expense = await _dbContext.Expenses
            .Include(e => e.TravelPlan)
            .FirstOrDefaultAsync(e => e.Id == expenseId && e.TravelPlanId == context.Value, cancellationToken);

        if (expense is null)
        {
            return null;
        }

        ValidateExpenseDate(dto.ExpenseDate, expense.TravelPlan!.StartDate, expense.TravelPlan.EndDate);

        ExpenseMapper.ApplyUpdate(expense, dto);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ExpenseMapper.ToResponseDto(expense);
    }

    public async Task<bool> DeleteSharedExpenseAsync(string token, int expenseId, CancellationToken cancellationToken = default)
    {
        var context = await GetValidShareContextAsync(token, cancellationToken, requireEdit: true);
        if (context is null)
        {
            return false;
        }

        var expense = await _dbContext.Expenses
            .FirstOrDefaultAsync(e => e.Id == expenseId && e.TravelPlanId == context.Value, cancellationToken);

        if (expense is null)
        {
            return false;
        }

        _dbContext.Expenses.Remove(expense);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<BudgetSummaryDto?> BuildSummaryAsync(int planId, CancellationToken cancellationToken)
    {
        var plan = await _dbContext.TravelPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == planId, cancellationToken);

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
        var totalEstimated = await _dbContext.Activities
            .AsNoTracking()
            .Where(a => a.TravelPlanId == planId && a.EstimatedCost != null)
            .SumAsync(a => a.EstimatedCost ?? 0, cancellationToken);

        return new BudgetSummaryDto
        {
            TravelPlanId = planId,
            PlannedBudget = plan.PlannedBudget,
            TotalSpent = totalSpent,
            TotalEstimated = totalEstimated,
            Remaining = plan.PlannedBudget - totalSpent - totalEstimated,
            ByCategory = byCategory
        };
    }

    private async Task<int?> GetValidShareContextAsync(
        string token,
        CancellationToken cancellationToken,
        bool requireEdit = false)
    {
        var link = await _dbContext.ShareLinks
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Token == token, cancellationToken);

        if (link is null)
        {
            return null;
        }

        if (link.ExpiresAt.HasValue && ToUtc(link.ExpiresAt) <= DateTime.UtcNow)
        {
            return null;
        }

        if (requireEdit && !link.AccessType.Equals("Edit", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return link.TravelPlanId;
    }

    private static DateTime ToUtc(DateTime? value)
    {
        if (!value.HasValue)
        {
            return DateTime.MinValue;
        }

        return value.Value.Kind switch
        {
            DateTimeKind.Utc => value.Value,
            DateTimeKind.Local => value.Value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value.Value, DateTimeKind.Utc)
        };
    }

    private async Task<bool> PlanExistsForUserAsync(int userId, int planId, CancellationToken cancellationToken)
    {
        return await GetOwnedPlanAsync(userId, planId, cancellationToken) is not null;
    }

    private async Task<Models.TravelPlan?> GetOwnedPlanAsync(int userId, int planId, CancellationToken cancellationToken)
    {
        return await _dbContext.TravelPlans
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == planId && p.UserId == userId, cancellationToken);
    }

    private static void ValidateExpenseDate(DateOnly expenseDate, DateOnly startDate, DateOnly endDate)
    {
        if (expenseDate < startDate || expenseDate > endDate)
        {
            throw new ArgumentException(
                $"Datum troška mora biti u periodu putovanja ({startDate:yyyy-MM-dd} – {endDate:yyyy-MM-dd}).");
        }
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

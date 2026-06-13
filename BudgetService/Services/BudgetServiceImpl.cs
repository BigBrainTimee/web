using BudgetService.Clients;
using BudgetService.Data;
using BudgetService.Dtos;
using BudgetService.Mappers;
using Microsoft.EntityFrameworkCore;

namespace BudgetService.Services;

public class BudgetServiceImpl : IBudgetService
{
    private readonly BudgetDbContext _dbContext;
    private readonly ITravelClient _travelClient;

    public BudgetServiceImpl(BudgetDbContext dbContext, ITravelClient travelClient)
    {
        _dbContext = dbContext;
        _travelClient = travelClient;
    }

    public async Task<IReadOnlyList<ExpenseResponseDto>> GetExpensesAsync(int userId, int planId, CancellationToken cancellationToken = default)
    {
        if (!await PlanExistsForUserAsync(userId, planId, cancellationToken))
        {
            return Array.Empty<ExpenseResponseDto>();
        }

        return await GetExpensesByPlanIdAsync(planId, cancellationToken);
    }

    public async Task<IReadOnlyList<ExpenseResponseDto>> GetExpensesByPlanIdAsync(int planId, CancellationToken cancellationToken = default)
    {
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
        var plan = await _travelClient.GetOwnedPlanAsync(planId, cancellationToken);
        if (plan is null || plan.UserId != userId)
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
        var plan = await _travelClient.GetOwnedPlanAsync(planId, cancellationToken);
        if (plan is null || plan.UserId != userId)
        {
            return null;
        }

        var expense = await _dbContext.Expenses
            .FirstOrDefaultAsync(e => e.Id == expenseId && e.TravelPlanId == planId, cancellationToken);

        if (expense is null)
        {
            return null;
        }

        ValidateExpenseDate(dto.ExpenseDate, plan.StartDate, plan.EndDate);

        ExpenseMapper.ApplyUpdate(expense, dto);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ExpenseMapper.ToResponseDto(expense);
    }

    public async Task<bool> DeleteExpenseAsync(int userId, int planId, int expenseId, CancellationToken cancellationToken = default)
    {
        var plan = await _travelClient.GetOwnedPlanAsync(planId, cancellationToken);
        if (plan is null || plan.UserId != userId)
        {
            return false;
        }

        var expense = await _dbContext.Expenses
            .FirstOrDefaultAsync(e => e.Id == expenseId && e.TravelPlanId == planId, cancellationToken);

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
        var plan = await _travelClient.GetOwnedPlanAsync(planId, cancellationToken);
        if (plan is null || plan.UserId != userId)
        {
            return null;
        }

        return await BuildSummaryAsync(plan, cancellationToken);
    }

    public async Task<IReadOnlyList<ExpenseResponseDto>> GetAdminExpensesAsync(
        int userId,
        int planId,
        CancellationToken cancellationToken = default)
    {
        var plan = await _travelClient.GetAdminPlanContextAsync(userId, planId, cancellationToken);
        if (plan is null)
        {
            return Array.Empty<ExpenseResponseDto>();
        }

        return await GetExpensesByPlanIdAsync(planId, cancellationToken);
    }

    public async Task<BudgetSummaryDto?> GetAdminSummaryAsync(
        int userId,
        int planId,
        CancellationToken cancellationToken = default)
    {
        var plan = await _travelClient.GetAdminPlanContextAsync(userId, planId, cancellationToken);
        if (plan is null)
        {
            return null;
        }

        return await BuildAdminSummaryAsync(plan, userId, cancellationToken);
    }

    public async Task<ExpenseResponseDto?> AddAdminExpenseAsync(
        int userId,
        int planId,
        CreateExpenseDto dto,
        CancellationToken cancellationToken = default)
    {
        var plan = await _travelClient.GetAdminPlanContextAsync(userId, planId, cancellationToken);
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

    public async Task<ExpenseResponseDto?> UpdateAdminExpenseAsync(
        int userId,
        int planId,
        int expenseId,
        UpdateExpenseDto dto,
        CancellationToken cancellationToken = default)
    {
        var plan = await _travelClient.GetAdminPlanContextAsync(userId, planId, cancellationToken);
        if (plan is null)
        {
            return null;
        }

        var expense = await _dbContext.Expenses
            .FirstOrDefaultAsync(e => e.Id == expenseId && e.TravelPlanId == planId, cancellationToken);

        if (expense is null)
        {
            return null;
        }

        ValidateExpenseDate(dto.ExpenseDate, plan.StartDate, plan.EndDate);

        ExpenseMapper.ApplyUpdate(expense, dto);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ExpenseMapper.ToResponseDto(expense);
    }

    public async Task<bool> DeleteAdminExpenseAsync(
        int userId,
        int planId,
        int expenseId,
        CancellationToken cancellationToken = default)
    {
        var plan = await _travelClient.GetAdminPlanContextAsync(userId, planId, cancellationToken);
        if (plan is null)
        {
            return false;
        }

        var expense = await _dbContext.Expenses
            .FirstOrDefaultAsync(e => e.Id == expenseId && e.TravelPlanId == planId, cancellationToken);

        if (expense is null)
        {
            return false;
        }

        _dbContext.Expenses.Remove(expense);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<BudgetSummaryDto?> GetSharedSummaryAsync(string token, CancellationToken cancellationToken = default)
    {
        var plan = await _travelClient.GetSharedPlanContextAsync(token, requireEdit: false, cancellationToken);
        if (plan is null)
        {
            return null;
        }

        return await BuildSummaryAsync(plan, cancellationToken);
    }

    public async Task<IReadOnlyList<ExpenseResponseDto>?> GetSharedExpensesAsync(string token, CancellationToken cancellationToken = default)
    {
        var plan = await _travelClient.GetSharedPlanContextAsync(token, requireEdit: false, cancellationToken);
        if (plan is null)
        {
            return null;
        }

        var expenses = await _dbContext.Expenses
            .AsNoTracking()
            .Where(e => e.TravelPlanId == plan.TravelPlanId)
            .OrderByDescending(e => e.ExpenseDate)
            .ThenBy(e => e.Id)
            .ToListAsync(cancellationToken);

        return expenses.Select(ExpenseMapper.ToResponseDto).ToList();
    }

    public async Task<ExpenseResponseDto?> AddSharedExpenseAsync(string token, CreateExpenseDto dto, CancellationToken cancellationToken = default)
    {
        var plan = await _travelClient.GetSharedPlanContextAsync(token, requireEdit: true, cancellationToken);
        if (plan is null)
        {
            return null;
        }

        ValidateExpenseDate(dto.ExpenseDate, plan.StartDate, plan.EndDate);

        var expense = ExpenseMapper.ToEntity(dto, plan.TravelPlanId);
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
        var plan = await _travelClient.GetSharedPlanContextAsync(token, requireEdit: true, cancellationToken);
        if (plan is null)
        {
            return null;
        }

        var expense = await _dbContext.Expenses
            .FirstOrDefaultAsync(e => e.Id == expenseId && e.TravelPlanId == plan.TravelPlanId, cancellationToken);

        if (expense is null)
        {
            return null;
        }

        ValidateExpenseDate(dto.ExpenseDate, plan.StartDate, plan.EndDate);

        ExpenseMapper.ApplyUpdate(expense, dto);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ExpenseMapper.ToResponseDto(expense);
    }

    public async Task<bool> DeleteSharedExpenseAsync(string token, int expenseId, CancellationToken cancellationToken = default)
    {
        var plan = await _travelClient.GetSharedPlanContextAsync(token, requireEdit: true, cancellationToken);
        if (plan is null)
        {
            return false;
        }

        var expense = await _dbContext.Expenses
            .FirstOrDefaultAsync(e => e.Id == expenseId && e.TravelPlanId == plan.TravelPlanId, cancellationToken);

        if (expense is null)
        {
            return false;
        }

        _dbContext.Expenses.Remove(expense);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private async Task<BudgetSummaryDto> BuildAdminSummaryAsync(
        PlanBudgetContext plan,
        int userId,
        CancellationToken cancellationToken)
    {
        var planId = plan.TravelPlanId;

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
        var totalEstimated = await _travelClient.GetAdminEstimatedActivityTotalAsync(userId, planId, cancellationToken);

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

    private async Task<BudgetSummaryDto> BuildSummaryAsync(PlanBudgetContext plan, CancellationToken cancellationToken)
    {
        var planId = plan.TravelPlanId;

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
        var totalEstimated = await _travelClient.GetEstimatedActivityTotalAsync(planId, cancellationToken);

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

    private async Task<bool> PlanExistsForUserAsync(int userId, int planId, CancellationToken cancellationToken)
    {
        var plan = await _travelClient.GetOwnedPlanAsync(planId, cancellationToken);
        return plan is not null && plan.UserId == userId;
    }

    private static void ValidateExpenseDate(DateOnly expenseDate, DateOnly startDate, DateOnly endDate)
    {
        if (expenseDate < startDate || expenseDate > endDate)
        {
            throw new ArgumentException(
                $"Datum troška mora biti u periodu putovanja ({startDate:yyyy-MM-dd} – {endDate:yyyy-MM-dd}).");
        }
    }
}

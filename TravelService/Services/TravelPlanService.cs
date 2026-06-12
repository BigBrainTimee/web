using TravelService.Data;
using TravelService.Dtos;
using TravelService.Mappers;
using Microsoft.EntityFrameworkCore;

namespace TravelService.Services;

public class TravelPlanService : ITravelPlanService
{
    private readonly TravelDbContext _dbContext;

    public TravelPlanService(TravelDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TravelPlanResponseDto>> GetAllForUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var plans = await _dbContext.TravelPlans
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);

        return plans.Select(TravelPlanMapper.ToResponseDto).ToList();
    }

    public async Task<TravelPlanResponseDto?> GetByIdAsync(int userId, int planId, CancellationToken cancellationToken = default)
    {
        var plan = await GetOwnedPlanAsync(userId, planId, asNoTracking: true, cancellationToken);
        return plan is null ? null : TravelPlanMapper.ToResponseDto(plan);
    }

    public async Task<TravelPlanReportDto?> GetPlanReportAsync(int userId, int planId, CancellationToken cancellationToken = default)
    {
        var plan = await GetOwnedPlanAsync(userId, planId, asNoTracking: true, cancellationToken);
        if (plan is null)
        {
            return null;
        }

        await EnsureDefaultChecklistItemsAsync(plan.Id, cancellationToken);
        return await BuildPlanReportAsync(plan, cancellationToken);
    }

    public async Task<TravelPlanResponseDto> CreateAsync(int userId, CreateTravelPlanDto dto, CancellationToken cancellationToken = default)
    {
        ValidateTravelPlanDates(dto.StartDate, dto.EndDate);
        ValidateBudget(dto.PlannedBudget);

        var plan = TravelPlanMapper.ToEntity(dto, userId);
        _dbContext.TravelPlans.Add(plan);
        await _dbContext.SaveChangesAsync(cancellationToken);
        await EnsureDefaultChecklistItemsAsync(plan.Id, cancellationToken);

        return TravelPlanMapper.ToResponseDto(plan);
    }

    public async Task<TravelPlanResponseDto?> UpdateAsync(int userId, int planId, UpdateTravelPlanDto dto, CancellationToken cancellationToken = default)
    {
        ValidateTravelPlanDates(dto.StartDate, dto.EndDate);
        ValidateBudget(dto.PlannedBudget);

        var plan = await GetOwnedPlanAsync(userId, planId, cancellationToken: cancellationToken);
        if (plan is null)
        {
            return null;
        }

        TravelPlanMapper.ApplyUpdate(plan, dto);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return TravelPlanMapper.ToResponseDto(plan);
    }

    public async Task<bool> DeleteAsync(int userId, int planId, CancellationToken cancellationToken = default)
    {
        var plan = await GetOwnedPlanAsync(userId, planId, cancellationToken: cancellationToken);
        if (plan is null)
        {
            return false;
        }

        _dbContext.TravelPlans.Remove(plan);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<DestinationResponseDto>> GetDestinationsAsync(int userId, int planId, CancellationToken cancellationToken = default)
    {
        if (!await PlanExistsForUserAsync(userId, planId, cancellationToken))
        {
            return Array.Empty<DestinationResponseDto>();
        }

        var destinations = await _dbContext.Destinations
            .AsNoTracking()
            .Where(d => d.TravelPlanId == planId)
            .OrderBy(d => d.ArrivalDate)
            .ToListAsync(cancellationToken);

        return destinations.Select(DestinationMapper.ToResponseDto).ToList();
    }

    public async Task<DestinationResponseDto?> AddDestinationAsync(int userId, int planId, CreateDestinationDto dto, CancellationToken cancellationToken = default)
    {
        ValidateDestinationDates(dto.ArrivalDate, dto.DepartureDate);

        if (!await PlanExistsForUserAsync(userId, planId, cancellationToken))
        {
            return null;
        }

        var destination = DestinationMapper.ToEntity(dto, planId);
        _dbContext.Destinations.Add(destination);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return DestinationMapper.ToResponseDto(destination);
    }

    public async Task<DestinationResponseDto?> UpdateDestinationAsync(int userId, int planId, int destinationId, UpdateDestinationDto dto, CancellationToken cancellationToken = default)
    {
        ValidateDestinationDates(dto.ArrivalDate, dto.DepartureDate);

        var destination = await GetOwnedDestinationAsync(userId, planId, destinationId, cancellationToken);
        if (destination is null)
        {
            return null;
        }

        DestinationMapper.ApplyUpdate(destination, dto);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return DestinationMapper.ToResponseDto(destination);
    }

    public async Task<bool> DeleteDestinationAsync(int userId, int planId, int destinationId, CancellationToken cancellationToken = default)
    {
        var destination = await GetOwnedDestinationAsync(userId, planId, destinationId, cancellationToken);
        if (destination is null)
        {
            return false;
        }

        _dbContext.Destinations.Remove(destination);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<ActivityResponseDto>> GetActivitiesAsync(int userId, int planId, CancellationToken cancellationToken = default)
    {
        if (!await PlanExistsForUserAsync(userId, planId, cancellationToken))
        {
            return Array.Empty<ActivityResponseDto>();
        }

        var activities = await _dbContext.Activities
            .AsNoTracking()
            .Where(a => a.TravelPlanId == planId)
            .OrderBy(a => a.ActivityDate)
            .ThenBy(a => a.ActivityTime)
            .ToListAsync(cancellationToken);

        return activities.Select(ActivityMapper.ToResponseDto).ToList();
    }

    public async Task<ActivityResponseDto?> AddActivityAsync(int userId, int planId, CreateActivityDto dto, CancellationToken cancellationToken = default)
    {
        ActivityMapper.NormalizeStatus(dto.Status);

        if (!await PlanExistsForUserAsync(userId, planId, cancellationToken))
        {
            return null;
        }

        await ValidateDestinationForPlanAsync(planId, dto.DestinationId, cancellationToken);

        if (dto.EstimatedCost is < 0)
        {
            throw new ArgumentException("Procenjeni trošak ne može biti negativan.");
        }

        var activity = ActivityMapper.ToEntity(dto, planId);
        _dbContext.Activities.Add(activity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ActivityMapper.ToResponseDto(activity);
    }

    public async Task<ActivityResponseDto?> UpdateActivityAsync(int userId, int planId, int activityId, UpdateActivityDto dto, CancellationToken cancellationToken = default)
    {
        ActivityMapper.NormalizeStatus(dto.Status);

        var activity = await GetOwnedActivityAsync(userId, planId, activityId, cancellationToken);
        if (activity is null)
        {
            return null;
        }

        await ValidateDestinationForPlanAsync(planId, dto.DestinationId, cancellationToken);

        if (dto.EstimatedCost is < 0)
        {
            throw new ArgumentException("Procenjeni trošak ne može biti negativan.");
        }

        ActivityMapper.ApplyUpdate(activity, dto);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ActivityMapper.ToResponseDto(activity);
    }

    public async Task<bool> DeleteActivityAsync(int userId, int planId, int activityId, CancellationToken cancellationToken = default)
    {
        var activity = await GetOwnedActivityAsync(userId, planId, activityId, cancellationToken);
        if (activity is null)
        {
            return false;
        }

        _dbContext.Activities.Remove(activity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<ChecklistItemResponseDto>> GetChecklistItemsAsync(int userId, int planId, CancellationToken cancellationToken = default)
    {
        if (!await PlanExistsForUserAsync(userId, planId, cancellationToken))
        {
            return Array.Empty<ChecklistItemResponseDto>();
        }

        await EnsureDefaultChecklistItemsAsync(planId, cancellationToken);

        var items = await _dbContext.ChecklistItems
            .AsNoTracking()
            .Where(c => c.TravelPlanId == planId)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Id)
            .ToListAsync(cancellationToken);

        return items.Select(ChecklistItemMapper.ToResponseDto).ToList();
    }

    public async Task<ChecklistItemResponseDto?> AddChecklistItemAsync(int userId, int planId, CreateChecklistItemDto dto, CancellationToken cancellationToken = default)
    {
        if (!await PlanExistsForUserAsync(userId, planId, cancellationToken))
        {
            return null;
        }

        var item = ChecklistItemMapper.ToEntity(dto, planId);
        ApplyCustomChecklistSortOrder(item);
        _dbContext.ChecklistItems.Add(item);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ChecklistItemMapper.ToResponseDto(item);
    }

    public async Task<ChecklistItemResponseDto?> UpdateChecklistItemAsync(int userId, int planId, int itemId, UpdateChecklistItemDto dto, CancellationToken cancellationToken = default)
    {
        var item = await GetOwnedChecklistItemAsync(userId, planId, itemId, cancellationToken);
        if (item is null)
        {
            return null;
        }

        ChecklistItemMapper.ApplyUpdate(item, dto);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ChecklistItemMapper.ToResponseDto(item);
    }

    public async Task<ChecklistItemResponseDto?> ToggleChecklistItemAsync(int userId, int planId, int itemId, CancellationToken cancellationToken = default)
    {
        var item = await GetOwnedChecklistItemAsync(userId, planId, itemId, cancellationToken);
        if (item is null)
        {
            return null;
        }

        item.IsCompleted = !item.IsCompleted;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ChecklistItemMapper.ToResponseDto(item);
    }

    public async Task<bool> DeleteChecklistItemAsync(int userId, int planId, int itemId, CancellationToken cancellationToken = default)
    {
        var item = await GetOwnedChecklistItemAsync(userId, planId, itemId, cancellationToken);
        if (item is null)
        {
            return false;
        }

        _dbContext.ChecklistItems.Remove(item);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<ShareLinkResponseDto>> GetShareLinksAsync(int userId, int planId, CancellationToken cancellationToken = default)
    {
        if (!await PlanExistsForUserAsync(userId, planId, cancellationToken))
        {
            return Array.Empty<ShareLinkResponseDto>();
        }

        var links = await _dbContext.ShareLinks
            .AsNoTracking()
            .Where(s => s.TravelPlanId == planId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync(cancellationToken);

        return links.Select(ShareLinkMapper.ToResponseDto).ToList();
    }

    public async Task<ShareLinkResponseDto?> CreateShareLinkAsync(int userId, int planId, CreateShareLinkDto dto, CancellationToken cancellationToken = default)
    {
        if (!await PlanExistsForUserAsync(userId, planId, cancellationToken))
        {
            return null;
        }

        if (dto.ExpiresAt.HasValue && ShareLinkMapper.ToUtc(dto.ExpiresAt) <= DateTime.UtcNow)
        {
            throw new ArgumentException("Datum isteka mora biti u budućnosti.");
        }

        var link = ShareLinkMapper.ToEntity(dto, planId);
        _dbContext.ShareLinks.Add(link);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ShareLinkMapper.ToResponseDto(link);
    }

    public async Task<bool> DeleteShareLinkAsync(int userId, int planId, int linkId, CancellationToken cancellationToken = default)
    {
        var link = await _dbContext.ShareLinks
            .Include(s => s.TravelPlan)
            .FirstOrDefaultAsync(
                s => s.Id == linkId
                    && s.TravelPlanId == planId
                    && s.TravelPlan != null
                    && s.TravelPlan.UserId == userId,
                cancellationToken);

        if (link is null)
        {
            return false;
        }

        _dbContext.ShareLinks.Remove(link);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<SharedPlanResponseDto?> GetSharedPlanAsync(string token, CancellationToken cancellationToken = default)
    {
        var context = await GetValidShareContextAsync(token, cancellationToken);
        if (context is null)
        {
            return null;
        }

        var (link, plan) = context.Value;
        await EnsureDefaultChecklistItemsAsync(plan.Id, cancellationToken);
        var report = await BuildPlanReportAsync(plan, cancellationToken);

        return new SharedPlanResponseDto
        {
            AccessType = link.AccessType,
            CanEdit = link.AccessType.Equals("Edit", StringComparison.OrdinalIgnoreCase),
            Plan = report.Plan,
            Destinations = report.Destinations,
            Activities = report.Activities,
            ChecklistItems = report.ChecklistItems,
            Expenses = report.Expenses,
            BudgetSummary = report.BudgetSummary
        };
    }

    public async Task<ChecklistItemResponseDto?> ToggleSharedChecklistItemAsync(string token, int itemId, CancellationToken cancellationToken = default)
    {
        var context = await GetValidShareContextAsync(token, cancellationToken, requireEdit: true);
        if (context is null)
        {
            return null;
        }

        var planId = context.Value.plan.Id;
        var item = await _dbContext.ChecklistItems
            .FirstOrDefaultAsync(c => c.Id == itemId && c.TravelPlanId == planId, cancellationToken);

        if (item is null)
        {
            return null;
        }

        item.IsCompleted = !item.IsCompleted;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ChecklistItemMapper.ToResponseDto(item);
    }

    public async Task<ChecklistItemResponseDto?> AddSharedChecklistItemAsync(string token, CreateChecklistItemDto dto, CancellationToken cancellationToken = default)
    {
        var context = await GetValidShareContextAsync(token, cancellationToken, requireEdit: true);
        if (context is null)
        {
            return null;
        }

        var planId = context.Value.plan.Id;
        var item = ChecklistItemMapper.ToEntity(dto, planId);
        ApplyCustomChecklistSortOrder(item);
        _dbContext.ChecklistItems.Add(item);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ChecklistItemMapper.ToResponseDto(item);
    }

    public async Task<bool> DeleteSharedChecklistItemAsync(string token, int itemId, CancellationToken cancellationToken = default)
    {
        var context = await GetValidShareContextAsync(token, cancellationToken, requireEdit: true);
        if (context is null)
        {
            return false;
        }

        var planId = context.Value.plan.Id;
        var item = await _dbContext.ChecklistItems
            .FirstOrDefaultAsync(c => c.Id == itemId && c.TravelPlanId == planId, cancellationToken);

        if (item is null)
        {
            return false;
        }

        _dbContext.ChecklistItems.Remove(item);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<DestinationResponseDto?> AddSharedDestinationAsync(string token, CreateDestinationDto dto, CancellationToken cancellationToken = default)
    {
        ValidateDestinationDates(dto.ArrivalDate, dto.DepartureDate);

        var context = await GetValidShareContextAsync(token, cancellationToken, requireEdit: true);
        if (context is null)
        {
            return null;
        }

        var planId = context.Value.plan.Id;
        var destination = DestinationMapper.ToEntity(dto, planId);
        _dbContext.Destinations.Add(destination);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return DestinationMapper.ToResponseDto(destination);
    }

    public async Task<DestinationResponseDto?> UpdateSharedDestinationAsync(
        string token,
        int destinationId,
        UpdateDestinationDto dto,
        CancellationToken cancellationToken = default)
    {
        ValidateDestinationDates(dto.ArrivalDate, dto.DepartureDate);

        var context = await GetValidShareContextAsync(token, cancellationToken, requireEdit: true);
        if (context is null)
        {
            return null;
        }

        var planId = context.Value.plan.Id;
        var destination = await _dbContext.Destinations
            .FirstOrDefaultAsync(d => d.Id == destinationId && d.TravelPlanId == planId, cancellationToken);

        if (destination is null)
        {
            return null;
        }

        DestinationMapper.ApplyUpdate(destination, dto);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return DestinationMapper.ToResponseDto(destination);
    }

    public async Task<bool> DeleteSharedDestinationAsync(string token, int destinationId, CancellationToken cancellationToken = default)
    {
        var context = await GetValidShareContextAsync(token, cancellationToken, requireEdit: true);
        if (context is null)
        {
            return false;
        }

        var planId = context.Value.plan.Id;
        var destination = await _dbContext.Destinations
            .FirstOrDefaultAsync(d => d.Id == destinationId && d.TravelPlanId == planId, cancellationToken);

        if (destination is null)
        {
            return false;
        }

        _dbContext.Destinations.Remove(destination);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ActivityResponseDto?> AddSharedActivityAsync(string token, CreateActivityDto dto, CancellationToken cancellationToken = default)
    {
        ActivityMapper.NormalizeStatus(dto.Status);

        var context = await GetValidShareContextAsync(token, cancellationToken, requireEdit: true);
        if (context is null)
        {
            return null;
        }

        var planId = context.Value.plan.Id;
        await ValidateDestinationForPlanAsync(planId, dto.DestinationId, cancellationToken);

        if (dto.EstimatedCost is < 0)
        {
            throw new ArgumentException("Procenjeni trošak ne može biti negativan.");
        }

        var activity = ActivityMapper.ToEntity(dto, planId);
        _dbContext.Activities.Add(activity);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ActivityMapper.ToResponseDto(activity);
    }

    public async Task<ActivityResponseDto?> UpdateSharedActivityAsync(
        string token,
        int activityId,
        UpdateActivityDto dto,
        CancellationToken cancellationToken = default)
    {
        ActivityMapper.NormalizeStatus(dto.Status);

        var context = await GetValidShareContextAsync(token, cancellationToken, requireEdit: true);
        if (context is null)
        {
            return null;
        }

        var planId = context.Value.plan.Id;
        var activity = await _dbContext.Activities
            .FirstOrDefaultAsync(a => a.Id == activityId && a.TravelPlanId == planId, cancellationToken);

        if (activity is null)
        {
            return null;
        }

        await ValidateDestinationForPlanAsync(planId, dto.DestinationId, cancellationToken);

        if (dto.EstimatedCost is < 0)
        {
            throw new ArgumentException("Procenjeni trošak ne može biti negativan.");
        }

        ActivityMapper.ApplyUpdate(activity, dto);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return ActivityMapper.ToResponseDto(activity);
    }

    public async Task<bool> DeleteSharedActivityAsync(string token, int activityId, CancellationToken cancellationToken = default)
    {
        var context = await GetValidShareContextAsync(token, cancellationToken, requireEdit: true);
        if (context is null)
        {
            return false;
        }

        var planId = context.Value.plan.Id;
        var activity = await _dbContext.Activities
            .FirstOrDefaultAsync(a => a.Id == activityId && a.TravelPlanId == planId, cancellationToken);

        if (activity is null)
        {
            return false;
        }

        _dbContext.Activities.Remove(activity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<ExpenseResponseDto?> AddSharedExpenseAsync(string token, CreateExpenseDto dto, CancellationToken cancellationToken = default)
    {
        var context = await GetValidShareContextAsync(token, cancellationToken, requireEdit: true);
        if (context is null)
        {
            return null;
        }

        var planId = context.Value.plan.Id;
        var expense = ExpenseMapper.ToEntity(dto, planId);
        _dbContext.Expenses.Add(expense);
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

        var planId = context.Value.plan.Id;
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

    private async Task<(Models.ShareLink link, Models.TravelPlan plan)?> GetValidShareContextAsync(
        string token,
        CancellationToken cancellationToken,
        bool requireEdit = false)
    {
        var link = await _dbContext.ShareLinks
            .Include(s => s.TravelPlan)
            .FirstOrDefaultAsync(s => s.Token == token, cancellationToken);

        if (link?.TravelPlan is null)
        {
            return null;
        }

        if (link.ExpiresAt.HasValue && ShareLinkMapper.ToUtc(link.ExpiresAt) <= DateTime.UtcNow)
        {
            return null;
        }

        if (requireEdit && !link.AccessType.Equals("Edit", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return (link, link.TravelPlan);
    }

    private async Task<TravelPlanReportDto> BuildPlanReportAsync(
        Models.TravelPlan plan,
        CancellationToken cancellationToken)
    {
        var planId = plan.Id;

        var destinations = await _dbContext.Destinations
            .AsNoTracking()
            .Where(d => d.TravelPlanId == planId)
            .OrderBy(d => d.ArrivalDate)
            .ToListAsync(cancellationToken);

        var activities = await _dbContext.Activities
            .AsNoTracking()
            .Where(a => a.TravelPlanId == planId)
            .OrderBy(a => a.ActivityDate)
            .ThenBy(a => a.ActivityTime)
            .ToListAsync(cancellationToken);

        var checklistItems = await _dbContext.ChecklistItems
            .AsNoTracking()
            .Where(c => c.TravelPlanId == planId)
            .OrderBy(c => c.SortOrder)
            .ThenBy(c => c.Id)
            .ToListAsync(cancellationToken);

        var expenses = await _dbContext.Expenses
            .AsNoTracking()
            .Where(e => e.TravelPlanId == planId)
            .OrderByDescending(e => e.ExpenseDate)
            .ThenBy(e => e.Id)
            .ToListAsync(cancellationToken);

        var byCategory = expenses
            .GroupBy(e => e.Category)
            .Select(g => new CategorySummaryDto
            {
                Category = g.Key,
                Amount = g.Sum(e => e.Amount)
            })
            .OrderBy(c => c.Category)
            .ToList();

        var totalSpent = byCategory.Sum(c => c.Amount);
        var totalEstimated = activities
            .Where(a => a.EstimatedCost.HasValue)
            .Sum(a => a.EstimatedCost ?? 0);

        return new TravelPlanReportDto
        {
            Plan = TravelPlanMapper.ToResponseDto(plan),
            Destinations = destinations.Select(DestinationMapper.ToResponseDto).ToList(),
            Activities = activities.Select(ActivityMapper.ToResponseDto).ToList(),
            ChecklistItems = checklistItems.Select(ChecklistItemMapper.ToResponseDto).ToList(),
            Expenses = expenses.Select(ExpenseMapper.ToResponseDto).ToList(),
            BudgetSummary = new BudgetSummaryDto
            {
                TravelPlanId = planId,
                PlannedBudget = plan.PlannedBudget,
                TotalSpent = totalSpent,
                TotalEstimated = totalEstimated,
                Remaining = plan.PlannedBudget - totalSpent - totalEstimated,
                ByCategory = byCategory
            }
        };
    }

    private async Task<Models.TravelPlan?> GetOwnedPlanAsync(
        int userId,
        int planId,
        bool asNoTracking = false,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.TravelPlans.Where(p => p.Id == planId && p.UserId == userId);
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    private async Task<bool> PlanExistsForUserAsync(int userId, int planId, CancellationToken cancellationToken)
    {
        return await _dbContext.TravelPlans
            .AsNoTracking()
            .AnyAsync(p => p.Id == planId && p.UserId == userId, cancellationToken);
    }

    private async Task<Models.Destination?> GetOwnedDestinationAsync(
        int userId,
        int planId,
        int destinationId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Destinations
            .Include(d => d.TravelPlan)
            .FirstOrDefaultAsync(
                d => d.Id == destinationId
                    && d.TravelPlanId == planId
                    && d.TravelPlan != null
                    && d.TravelPlan.UserId == userId,
                cancellationToken);
    }

    private async Task<Models.Activity?> GetOwnedActivityAsync(
        int userId,
        int planId,
        int activityId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Activities
            .Include(a => a.TravelPlan)
            .FirstOrDefaultAsync(
                a => a.Id == activityId
                    && a.TravelPlanId == planId
                    && a.TravelPlan != null
                    && a.TravelPlan.UserId == userId,
                cancellationToken);
    }

    private async Task<Models.ChecklistItem?> GetOwnedChecklistItemAsync(
        int userId,
        int planId,
        int itemId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.ChecklistItems
            .Include(c => c.TravelPlan)
            .FirstOrDefaultAsync(
                c => c.Id == itemId
                    && c.TravelPlanId == planId
                    && c.TravelPlan != null
                    && c.TravelPlan.UserId == userId,
                cancellationToken);
    }

    private async Task EnsureDefaultChecklistItemsAsync(int planId, CancellationToken cancellationToken)
    {
        var existingTitles = await _dbContext.ChecklistItems
            .Where(c => c.TravelPlanId == planId)
            .Select(c => c.Title)
            .ToListAsync(cancellationToken);

        var existingSet = existingTitles.ToHashSet(StringComparer.OrdinalIgnoreCase);
        var added = false;

        for (var i = 0; i < ChecklistDefaults.Titles.Length; i++)
        {
            var title = ChecklistDefaults.Titles[i];
            if (existingSet.Contains(title))
            {
                continue;
            }

            _dbContext.ChecklistItems.Add(new Models.ChecklistItem
            {
                TravelPlanId = planId,
                Title = title,
                IsCompleted = false,
                SortOrder = i + 1,
            });
            added = true;
        }

        if (added)
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    private static void ApplyCustomChecklistSortOrder(Models.ChecklistItem item)
    {
        var isDefault = ChecklistDefaults.Titles.Any(
            title => title.Equals(item.Title, StringComparison.OrdinalIgnoreCase));

        if (!isDefault && item.SortOrder < ChecklistDefaults.CustomSortOrder)
        {
            item.SortOrder = ChecklistDefaults.CustomSortOrder;
        }
    }

    private async Task ValidateDestinationForPlanAsync(int planId, int? destinationId, CancellationToken cancellationToken)
    {
        if (destinationId is null)
        {
            return;
        }

        var exists = await _dbContext.Destinations
            .AnyAsync(d => d.Id == destinationId && d.TravelPlanId == planId, cancellationToken);

        if (!exists)
        {
            throw new ArgumentException("Destinacija ne pripada ovom planu.");
        }
    }

    private static void ValidateTravelPlanDates(DateOnly startDate, DateOnly endDate)
    {
        if (endDate < startDate)
        {
            throw new ArgumentException("Krajnji datum ne može biti pre početnog.");
        }
    }

    private static void ValidateBudget(decimal plannedBudget)
    {
        if (plannedBudget < 0)
        {
            throw new ArgumentException("Planirani budžet ne može biti negativan.");
        }
    }

    private static void ValidateDestinationDates(DateOnly arrivalDate, DateOnly departureDate)
    {
        if (departureDate < arrivalDate)
        {
            throw new ArgumentException("Datum odlaska ne može biti pre dolaska.");
        }
    }
}

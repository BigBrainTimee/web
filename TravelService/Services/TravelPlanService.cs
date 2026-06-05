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

    public async Task<TravelPlanResponseDto> CreateAsync(int userId, CreateTravelPlanDto dto, CancellationToken cancellationToken = default)
    {
        ValidateTravelPlanDates(dto.StartDate, dto.EndDate);
        ValidateBudget(dto.PlannedBudget);

        var plan = TravelPlanMapper.ToEntity(dto, userId);
        _dbContext.TravelPlans.Add(plan);
        await _dbContext.SaveChangesAsync(cancellationToken);

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

    private static void ValidateTravelPlanDates(DateOnly startDate, DateOnly endDate)
    {
        if (endDate < startDate)
        {
            throw new ArgumentException("End date cannot be before start date.");
        }
    }

    private static void ValidateBudget(decimal plannedBudget)
    {
        if (plannedBudget < 0)
        {
            throw new ArgumentException("Planned budget cannot be negative.");
        }
    }

    private static void ValidateDestinationDates(DateOnly arrivalDate, DateOnly departureDate)
    {
        if (departureDate < arrivalDate)
        {
            throw new ArgumentException("Departure date cannot be before arrival date.");
        }
    }
}

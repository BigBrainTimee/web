using TravelService.Dtos;
using TravelService.Models;

namespace TravelService.Mappers;

public static class TravelPlanMapper
{
    public static TravelPlanResponseDto ToResponseDto(TravelPlan plan)
    {
        return new TravelPlanResponseDto
        {
            Id = plan.Id,
            UserId = plan.UserId,
            Name = plan.Name,
            Description = plan.Description,
            StartDate = plan.StartDate,
            EndDate = plan.EndDate,
            PlannedBudget = plan.PlannedBudget,
            Notes = plan.Notes,
            CreatedAt = plan.CreatedAt,
            UpdatedAt = plan.UpdatedAt
        };
    }

    public static TravelPlan ToEntity(CreateTravelPlanDto dto, int userId)
    {
        var now = DateTime.UtcNow;
        return new TravelPlan
        {
            UserId = userId,
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            PlannedBudget = dto.PlannedBudget,
            Notes = dto.Notes?.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };
    }

    public static void ApplyUpdate(TravelPlan plan, UpdateTravelPlanDto dto)
    {
        plan.Name = dto.Name.Trim();
        plan.Description = dto.Description?.Trim();
        plan.StartDate = dto.StartDate;
        plan.EndDate = dto.EndDate;
        plan.PlannedBudget = dto.PlannedBudget;
        plan.Notes = dto.Notes?.Trim();
        plan.UpdatedAt = DateTime.UtcNow;
    }
}

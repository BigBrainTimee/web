using TravelService.Dtos;
using TravelService.Models;

namespace TravelService.Mappers;

public static class ActivityMapper
{
    private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Planned", "Reserved", "Completed", "Cancelled"
    };

    public static ActivityResponseDto ToResponseDto(Activity activity)
    {
        return new ActivityResponseDto
        {
            Id = activity.Id,
            TravelPlanId = activity.TravelPlanId,
            DestinationId = activity.DestinationId,
            ActivityDate = activity.ActivityDate,
            ActivityTime = activity.ActivityTime,
            Name = activity.Name,
            Location = activity.Location,
            Description = activity.Description,
            EstimatedCost = activity.EstimatedCost,
            Status = activity.Status
        };
    }

    public static Activity ToEntity(CreateActivityDto dto, int travelPlanId)
    {
        return new Activity
        {
            TravelPlanId = travelPlanId,
            DestinationId = dto.DestinationId,
            ActivityDate = dto.ActivityDate,
            ActivityTime = dto.ActivityTime,
            Name = dto.Name.Trim(),
            Location = dto.Location?.Trim(),
            Description = dto.Description?.Trim(),
            EstimatedCost = dto.EstimatedCost,
            Status = NormalizeStatus(dto.Status)
        };
    }

    public static void ApplyUpdate(Activity activity, UpdateActivityDto dto)
    {
        activity.DestinationId = dto.DestinationId;
        activity.ActivityDate = dto.ActivityDate;
        activity.ActivityTime = dto.ActivityTime;
        activity.Name = dto.Name.Trim();
        activity.Location = dto.Location?.Trim();
        activity.Description = dto.Description?.Trim();
        activity.EstimatedCost = dto.EstimatedCost;
        activity.Status = NormalizeStatus(dto.Status);
    }

    public static string NormalizeStatus(string status)
    {
        if (!ValidStatuses.Contains(status))
        {
            throw new ArgumentException("Invalid activity status.");
        }

        return ValidStatuses.First(s => s.Equals(status, StringComparison.OrdinalIgnoreCase));
    }
}

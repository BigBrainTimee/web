using TravelService.Dtos;
using TravelService.Models;

namespace TravelService.Mappers;

public static class ChecklistItemMapper
{
    public static ChecklistItemResponseDto ToResponseDto(ChecklistItem item)
    {
        return new ChecklistItemResponseDto
        {
            Id = item.Id,
            TravelPlanId = item.TravelPlanId,
            Title = item.Title,
            IsCompleted = item.IsCompleted,
            SortOrder = item.SortOrder
        };
    }

    public static ChecklistItem ToEntity(CreateChecklistItemDto dto, int travelPlanId)
    {
        return new ChecklistItem
        {
            TravelPlanId = travelPlanId,
            Title = dto.Title.Trim(),
            IsCompleted = false,
            SortOrder = dto.SortOrder
        };
    }

    public static void ApplyUpdate(ChecklistItem item, UpdateChecklistItemDto dto)
    {
        item.Title = dto.Title.Trim();
        item.SortOrder = dto.SortOrder;
    }
}

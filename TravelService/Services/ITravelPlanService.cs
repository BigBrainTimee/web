using TravelService.Dtos;

namespace TravelService.Services;

public interface ITravelPlanService
{
    Task<IReadOnlyList<TravelPlanResponseDto>> GetAllForUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<TravelPlanResponseDto?> GetByIdAsync(int userId, int planId, CancellationToken cancellationToken = default);
    Task<TravelPlanReportDto?> GetPlanReportAsync(int userId, int planId, CancellationToken cancellationToken = default);
    Task<TravelPlanResponseDto> CreateAsync(int userId, CreateTravelPlanDto dto, CancellationToken cancellationToken = default);
    Task<TravelPlanResponseDto?> UpdateAsync(int userId, int planId, UpdateTravelPlanDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int userId, int planId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DestinationResponseDto>> GetDestinationsAsync(int userId, int planId, CancellationToken cancellationToken = default);
    Task<DestinationResponseDto?> AddDestinationAsync(int userId, int planId, CreateDestinationDto dto, CancellationToken cancellationToken = default);
    Task<DestinationResponseDto?> UpdateDestinationAsync(int userId, int planId, int destinationId, UpdateDestinationDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteDestinationAsync(int userId, int planId, int destinationId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ActivityResponseDto>> GetActivitiesAsync(int userId, int planId, CancellationToken cancellationToken = default);
    Task<ActivityResponseDto?> AddActivityAsync(int userId, int planId, CreateActivityDto dto, CancellationToken cancellationToken = default);
    Task<ActivityResponseDto?> UpdateActivityAsync(int userId, int planId, int activityId, UpdateActivityDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteActivityAsync(int userId, int planId, int activityId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ChecklistItemResponseDto>> GetChecklistItemsAsync(int userId, int planId, CancellationToken cancellationToken = default);
    Task<ChecklistItemResponseDto?> AddChecklistItemAsync(int userId, int planId, CreateChecklistItemDto dto, CancellationToken cancellationToken = default);
    Task<ChecklistItemResponseDto?> UpdateChecklistItemAsync(int userId, int planId, int itemId, UpdateChecklistItemDto dto, CancellationToken cancellationToken = default);
    Task<ChecklistItemResponseDto?> ToggleChecklistItemAsync(int userId, int planId, int itemId, CancellationToken cancellationToken = default);
    Task<bool> DeleteChecklistItemAsync(int userId, int planId, int itemId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ShareLinkResponseDto>> GetShareLinksAsync(int userId, int planId, CancellationToken cancellationToken = default);
    Task<ShareLinkResponseDto?> CreateShareLinkAsync(int userId, int planId, CreateShareLinkDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteShareLinkAsync(int userId, int planId, int linkId, CancellationToken cancellationToken = default);

    Task<SharedPlanResponseDto?> GetSharedPlanAsync(string token, CancellationToken cancellationToken = default);
    Task<ChecklistItemResponseDto?> ToggleSharedChecklistItemAsync(string token, int itemId, CancellationToken cancellationToken = default);
    Task<ChecklistItemResponseDto?> AddSharedChecklistItemAsync(string token, CreateChecklistItemDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteSharedChecklistItemAsync(string token, int itemId, CancellationToken cancellationToken = default);

    Task<DestinationResponseDto?> AddSharedDestinationAsync(string token, CreateDestinationDto dto, CancellationToken cancellationToken = default);
    Task<DestinationResponseDto?> UpdateSharedDestinationAsync(string token, int destinationId, UpdateDestinationDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteSharedDestinationAsync(string token, int destinationId, CancellationToken cancellationToken = default);

    Task<ActivityResponseDto?> AddSharedActivityAsync(string token, CreateActivityDto dto, CancellationToken cancellationToken = default);
    Task<ActivityResponseDto?> UpdateSharedActivityAsync(string token, int activityId, UpdateActivityDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteSharedActivityAsync(string token, int activityId, CancellationToken cancellationToken = default);

    Task<ExpenseResponseDto?> AddSharedExpenseAsync(string token, CreateExpenseDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteSharedExpenseAsync(string token, int expenseId, CancellationToken cancellationToken = default);
}

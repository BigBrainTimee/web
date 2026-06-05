using TravelService.Dtos;

namespace TravelService.Services;

public interface ITravelPlanService
{
    Task<IReadOnlyList<TravelPlanResponseDto>> GetAllForUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<TravelPlanResponseDto?> GetByIdAsync(int userId, int planId, CancellationToken cancellationToken = default);
    Task<TravelPlanResponseDto> CreateAsync(int userId, CreateTravelPlanDto dto, CancellationToken cancellationToken = default);
    Task<TravelPlanResponseDto?> UpdateAsync(int userId, int planId, UpdateTravelPlanDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int userId, int planId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DestinationResponseDto>> GetDestinationsAsync(int userId, int planId, CancellationToken cancellationToken = default);
    Task<DestinationResponseDto?> AddDestinationAsync(int userId, int planId, CreateDestinationDto dto, CancellationToken cancellationToken = default);
    Task<DestinationResponseDto?> UpdateDestinationAsync(int userId, int planId, int destinationId, UpdateDestinationDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteDestinationAsync(int userId, int planId, int destinationId, CancellationToken cancellationToken = default);
}

using TravelService.Dtos;
using TravelService.Models;

namespace TravelService.Mappers;

public static class DestinationMapper
{
    public static DestinationResponseDto ToResponseDto(Destination destination)
    {
        return new DestinationResponseDto
        {
            Id = destination.Id,
            TravelPlanId = destination.TravelPlanId,
            Name = destination.Name,
            Location = destination.Location,
            ArrivalDate = destination.ArrivalDate,
            DepartureDate = destination.DepartureDate,
            Description = destination.Description
        };
    }

    public static Destination ToEntity(CreateDestinationDto dto, int travelPlanId)
    {
        return new Destination
        {
            TravelPlanId = travelPlanId,
            Name = dto.Name.Trim(),
            Location = dto.Location.Trim(),
            ArrivalDate = dto.ArrivalDate,
            DepartureDate = dto.DepartureDate,
            Description = dto.Description?.Trim()
        };
    }

    public static void ApplyUpdate(Destination destination, UpdateDestinationDto dto)
    {
        destination.Name = dto.Name.Trim();
        destination.Location = dto.Location.Trim();
        destination.ArrivalDate = dto.ArrivalDate;
        destination.DepartureDate = dto.DepartureDate;
        destination.Description = dto.Description?.Trim();
    }
}

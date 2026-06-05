using System.ComponentModel.DataAnnotations;

namespace TravelService.Dtos;

public class UpdateDestinationDto
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(300, MinimumLength = 2)]
    public string Location { get; set; } = string.Empty;

    [Required]
    public DateOnly ArrivalDate { get; set; }

    [Required]
    public DateOnly DepartureDate { get; set; }

    public string? Description { get; set; }
}

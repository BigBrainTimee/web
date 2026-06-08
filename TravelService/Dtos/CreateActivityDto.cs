using System.ComponentModel.DataAnnotations;

namespace TravelService.Dtos;

public class CreateActivityDto
{
    public int? DestinationId { get; set; }

    [Required]
    public DateOnly ActivityDate { get; set; }

    public TimeOnly? ActivityTime { get; set; }

    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(300)]
    public string? Location { get; set; }

    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? EstimatedCost { get; set; }

    [Required]
    public string Status { get; set; } = "Planned";
}

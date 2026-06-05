using System.ComponentModel.DataAnnotations;

namespace TravelService.Dtos;

public class UpdateTravelPlanDto
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }

    [Required]
    public DateOnly EndDate { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "Planned budget cannot be negative.")]
    public decimal PlannedBudget { get; set; }

    public string? Notes { get; set; }
}

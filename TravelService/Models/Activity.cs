namespace TravelService.Models;

public class Activity
{
    public int Id { get; set; }
    public int TravelPlanId { get; set; }
    public int? DestinationId { get; set; }
    public DateOnly ActivityDate { get; set; }
    public TimeOnly? ActivityTime { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string? Description { get; set; }
    public decimal? EstimatedCost { get; set; }
    public string Status { get; set; } = "Planned";

    public TravelPlan? TravelPlan { get; set; }
    public Destination? Destination { get; set; }
}

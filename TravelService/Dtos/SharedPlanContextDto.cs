namespace TravelService.Dtos;

public class SharedPlanContextDto
{
    public int TravelPlanId { get; set; }
    public int UserId { get; set; }
    public decimal PlannedBudget { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool CanEdit { get; set; }
}

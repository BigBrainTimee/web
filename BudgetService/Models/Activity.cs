namespace BudgetService.Models;

public class Activity
{
    public int Id { get; set; }
    public int TravelPlanId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateOnly ActivityDate { get; set; }
    public decimal? EstimatedCost { get; set; }
}

namespace TravelService.Models;

public class Expense
{
    public int Id { get; set; }
    public int TravelPlanId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateOnly ExpenseDate { get; set; }
    public string? Description { get; set; }

    public TravelPlan? TravelPlan { get; set; }
}

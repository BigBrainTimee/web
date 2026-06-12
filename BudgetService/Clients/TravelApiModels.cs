namespace BudgetService.Clients;

public sealed class TravelPlanApiDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal PlannedBudget { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}

public sealed class ActivityApiDto
{
    public decimal? EstimatedCost { get; set; }
}

public sealed class SharedPlanContextApiDto
{
    public int TravelPlanId { get; set; }
    public int UserId { get; set; }
    public decimal PlannedBudget { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool CanEdit { get; set; }
}

public sealed class PlanBudgetContext
{
    public int TravelPlanId { get; set; }
    public int UserId { get; set; }
    public decimal PlannedBudget { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
}

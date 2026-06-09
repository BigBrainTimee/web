namespace BudgetService.Dtos;

public class BudgetSummaryDto
{
    public int TravelPlanId { get; set; }
    public decimal PlannedBudget { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal TotalEstimated { get; set; }
    public decimal Remaining { get; set; }
    public IReadOnlyList<CategorySummaryDto> ByCategory { get; set; } = Array.Empty<CategorySummaryDto>();
}

public class CategorySummaryDto
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

using System.ComponentModel.DataAnnotations;

namespace TravelService.Dtos;

public class CreateExpenseDto
{
    [Required]
    [StringLength(200, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public string Category { get; set; } = string.Empty;

    [Required]
    [Range(0, double.MaxValue)]
    public decimal Amount { get; set; }

    [Required]
    public DateOnly ExpenseDate { get; set; }

    public string? Description { get; set; }
}

public class ExpenseResponseDto
{
    public int Id { get; set; }
    public int TravelPlanId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateOnly ExpenseDate { get; set; }
    public string? Description { get; set; }
}

public class BudgetSummaryDto
{
    public int TravelPlanId { get; set; }
    public decimal PlannedBudget { get; set; }
    public decimal TotalSpent { get; set; }
    public decimal Remaining { get; set; }
    public IReadOnlyList<CategorySummaryDto> ByCategory { get; set; } = Array.Empty<CategorySummaryDto>();
}

public class CategorySummaryDto
{
    public string Category { get; set; } = string.Empty;
    public decimal Amount { get; set; }
}

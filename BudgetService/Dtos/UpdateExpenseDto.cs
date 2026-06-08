using System.ComponentModel.DataAnnotations;

namespace BudgetService.Dtos;

public class UpdateExpenseDto
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

using BudgetService.Dtos;
using BudgetService.Models;

namespace BudgetService.Mappers;

public static class ExpenseMapper
{
    private static readonly HashSet<string> ValidCategories = new(StringComparer.OrdinalIgnoreCase)
    {
        "Transport", "Accommodation", "Food", "Tickets", "Shopping", "Other"
    };

    public static ExpenseResponseDto ToResponseDto(Expense expense)
    {
        return new ExpenseResponseDto
        {
            Id = expense.Id,
            TravelPlanId = expense.TravelPlanId,
            Name = expense.Name,
            Category = expense.Category,
            Amount = expense.Amount,
            ExpenseDate = expense.ExpenseDate,
            Description = expense.Description
        };
    }

    public static Expense ToEntity(CreateExpenseDto dto, int travelPlanId)
    {
        return new Expense
        {
            TravelPlanId = travelPlanId,
            Name = dto.Name.Trim(),
            Category = NormalizeCategory(dto.Category),
            Amount = dto.Amount,
            ExpenseDate = dto.ExpenseDate,
            Description = dto.Description?.Trim()
        };
    }

    public static void ApplyUpdate(Expense expense, UpdateExpenseDto dto)
    {
        expense.Name = dto.Name.Trim();
        expense.Category = NormalizeCategory(dto.Category);
        expense.Amount = dto.Amount;
        expense.ExpenseDate = dto.ExpenseDate;
        expense.Description = dto.Description?.Trim();
    }

    public static string NormalizeCategory(string category)
    {
        if (!ValidCategories.Contains(category))
        {
            throw new ArgumentException("Neispravna kategorija troška.");
        }

        return ValidCategories.First(c => c.Equals(category, StringComparison.OrdinalIgnoreCase));
    }
}

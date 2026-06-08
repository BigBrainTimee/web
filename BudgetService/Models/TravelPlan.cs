namespace BudgetService.Models;

public class TravelPlan
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public decimal PlannedBudget { get; set; }

    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}

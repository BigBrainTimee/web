using BudgetService.Dtos;

namespace BudgetService.Services;

public interface IBudgetService
{
    Task<IReadOnlyList<ExpenseResponseDto>> GetExpensesAsync(int userId, int planId, CancellationToken cancellationToken = default);
    Task<ExpenseResponseDto?> AddExpenseAsync(int userId, int planId, CreateExpenseDto dto, CancellationToken cancellationToken = default);
    Task<ExpenseResponseDto?> UpdateExpenseAsync(int userId, int planId, int expenseId, UpdateExpenseDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteExpenseAsync(int userId, int planId, int expenseId, CancellationToken cancellationToken = default);
    Task<BudgetSummaryDto?> GetSummaryAsync(int userId, int planId, CancellationToken cancellationToken = default);
}

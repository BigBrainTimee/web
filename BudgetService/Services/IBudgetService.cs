using BudgetService.Dtos;

namespace BudgetService.Services;

public interface IBudgetService
{
    Task<IReadOnlyList<ExpenseResponseDto>> GetExpensesAsync(int userId, int planId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExpenseResponseDto>> GetExpensesByPlanIdAsync(int planId, CancellationToken cancellationToken = default);
    Task<ExpenseResponseDto?> AddExpenseAsync(int userId, int planId, CreateExpenseDto dto, CancellationToken cancellationToken = default);
    Task<ExpenseResponseDto?> UpdateExpenseAsync(int userId, int planId, int expenseId, UpdateExpenseDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteExpenseAsync(int userId, int planId, int expenseId, CancellationToken cancellationToken = default);
    Task<BudgetSummaryDto?> GetSummaryAsync(int userId, int planId, CancellationToken cancellationToken = default);

    Task<BudgetSummaryDto?> GetSharedSummaryAsync(string token, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExpenseResponseDto>?> GetSharedExpensesAsync(string token, CancellationToken cancellationToken = default);
    Task<ExpenseResponseDto?> AddSharedExpenseAsync(string token, CreateExpenseDto dto, CancellationToken cancellationToken = default);
    Task<ExpenseResponseDto?> UpdateSharedExpenseAsync(string token, int expenseId, UpdateExpenseDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteSharedExpenseAsync(string token, int expenseId, CancellationToken cancellationToken = default);
}

using TravelService.Dtos;

namespace TravelService.Clients;

public interface IBudgetClient
{
    Task<IReadOnlyList<ExpenseResponseDto>> GetExpensesAsync(int planId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ExpenseResponseDto>> GetExpensesByPlanIdInternalAsync(int planId, CancellationToken cancellationToken = default);

    Task<BudgetSummaryDto?> GetSummaryAsync(int planId, CancellationToken cancellationToken = default);

    Task<BudgetSummaryDto?> GetSharedSummaryAsync(string token, CancellationToken cancellationToken = default);
}

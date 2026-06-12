namespace BudgetService.Clients;

public interface ITravelClient
{
    Task<PlanBudgetContext?> GetOwnedPlanAsync(int planId, CancellationToken cancellationToken = default);

    Task<decimal> GetEstimatedActivityTotalAsync(int planId, CancellationToken cancellationToken = default);

    Task<PlanBudgetContext?> GetSharedPlanContextAsync(
        string token,
        bool requireEdit,
        CancellationToken cancellationToken = default);
}

namespace BudgetService.Clients;

public interface ITravelClient
{
    Task<PlanBudgetContext?> GetOwnedPlanAsync(int planId, CancellationToken cancellationToken = default);

    Task<PlanBudgetContext?> GetAdminPlanContextAsync(int userId, int planId, CancellationToken cancellationToken = default);

    Task<decimal> GetEstimatedActivityTotalAsync(int planId, CancellationToken cancellationToken = default);

    Task<decimal> GetAdminEstimatedActivityTotalAsync(int userId, int planId, CancellationToken cancellationToken = default);

    Task<PlanBudgetContext?> GetSharedPlanContextAsync(
        string token,
        bool requireEdit,
        CancellationToken cancellationToken = default);
}

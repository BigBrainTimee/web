using System.Net.Http.Headers;
using System.Net.Http.Json;
using BudgetService.Configuration;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace BudgetService.Clients;

public sealed class TravelClient : ITravelClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ServiceUrlsSettings _serviceUrls;

    public TravelClient(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        IOptions<ServiceUrlsSettings> serviceUrls)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _serviceUrls = serviceUrls.Value;
    }

    public async Task<PlanBudgetContext?> GetOwnedPlanAsync(int planId, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Get, $"/api/travel/travel-plans/{planId}");
        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        var plan = await response.Content.ReadFromJsonAsync<TravelPlanApiDto>(cancellationToken: cancellationToken);
        return plan is null ? null : ToContext(plan);
    }

    public async Task<decimal> GetEstimatedActivityTotalAsync(int planId, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Get, $"/api/travel/travel-plans/{planId}/activities");
        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return 0;
        }

        var activities = await response.Content.ReadFromJsonAsync<List<ActivityApiDto>>(cancellationToken: cancellationToken)
            ?? new List<ActivityApiDto>();

        return activities.Where(a => a.EstimatedCost.HasValue).Sum(a => a.EstimatedCost ?? 0);
    }

    public async Task<PlanBudgetContext?> GetSharedPlanContextAsync(
        string token,
        bool requireEdit,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_serviceUrls.TravelService.TrimEnd('/')}/api/travel/internal/shared/{token}/context");
        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        var shared = await response.Content.ReadFromJsonAsync<SharedPlanContextApiDto>(cancellationToken: cancellationToken);

        if (shared is null)
        {
            return null;
        }

        if (requireEdit && !shared.CanEdit)
        {
            return null;
        }

        return new PlanBudgetContext
        {
            TravelPlanId = shared.TravelPlanId,
            UserId = shared.UserId,
            PlannedBudget = shared.PlannedBudget,
            StartDate = shared.StartDate,
            EndDate = shared.EndDate,
        };
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path, bool includeAuth = true)
    {
        var request = new HttpRequestMessage(method, $"{_serviceUrls.TravelService.TrimEnd('/')}{path}");

        if (includeAuth)
        {
            var authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
            if (!string.IsNullOrWhiteSpace(authHeader))
            {
                request.Headers.Authorization = AuthenticationHeaderValue.Parse(authHeader);
            }
        }

        return request;
    }

    private static PlanBudgetContext ToContext(TravelPlanApiDto plan) =>
        new()
        {
            TravelPlanId = plan.Id,
            UserId = plan.UserId,
            PlannedBudget = plan.PlannedBudget,
            StartDate = plan.StartDate,
            EndDate = plan.EndDate,
        };
}

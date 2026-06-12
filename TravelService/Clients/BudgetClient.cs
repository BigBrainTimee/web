using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using TravelService.Configuration;
using TravelService.Dtos;

namespace TravelService.Clients;

public sealed class BudgetClient : IBudgetClient
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ServiceUrlsSettings _serviceUrls;

    public BudgetClient(
        HttpClient httpClient,
        IHttpContextAccessor httpContextAccessor,
        IOptions<ServiceUrlsSettings> serviceUrls)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _serviceUrls = serviceUrls.Value;
    }

    public async Task<IReadOnlyList<ExpenseResponseDto>> GetExpensesAsync(int planId, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Get, $"/api/budget/travel-plans/{planId}/expenses");
        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return Array.Empty<ExpenseResponseDto>();
        }

        return await response.Content.ReadFromJsonAsync<List<ExpenseResponseDto>>(cancellationToken: cancellationToken)
            ?? new List<ExpenseResponseDto>();
    }

    public async Task<BudgetSummaryDto?> GetSummaryAsync(int planId, CancellationToken cancellationToken = default)
    {
        using var request = CreateRequest(HttpMethod.Get, $"/api/budget/travel-plans/{planId}/summary");
        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<BudgetSummaryDto>(cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<ExpenseResponseDto>> GetExpensesByPlanIdInternalAsync(int planId, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_serviceUrls.BudgetService.TrimEnd('/')}/api/budget/internal/travel-plans/{planId}/expenses");

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return Array.Empty<ExpenseResponseDto>();
        }

        return await response.Content.ReadFromJsonAsync<List<ExpenseResponseDto>>(cancellationToken: cancellationToken)
            ?? new List<ExpenseResponseDto>();
    }

    public async Task<BudgetSummaryDto?> GetSharedSummaryAsync(string token, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_serviceUrls.BudgetService.TrimEnd('/')}/api/budget/shared/{token}/summary");

        using var response = await _httpClient.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<BudgetSummaryDto>(cancellationToken: cancellationToken);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path)
    {
        var request = new HttpRequestMessage(method, $"{_serviceUrls.BudgetService.TrimEnd('/')}{path}");
        var authHeader = _httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();

        if (!string.IsNullOrWhiteSpace(authHeader))
        {
            request.Headers.Authorization = AuthenticationHeaderValue.Parse(authHeader);
        }

        return request;
    }
}

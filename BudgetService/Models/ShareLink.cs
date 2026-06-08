namespace BudgetService.Models;

public class ShareLink
{
    public int Id { get; set; }
    public int TravelPlanId { get; set; }
    public string Token { get; set; } = string.Empty;
    public string AccessType { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}

namespace TravelService.Dtos;

public class SharedPlanResponseDto
{
    public string AccessType { get; set; } = string.Empty;
    public bool CanEdit { get; set; }
    public TravelPlanResponseDto Plan { get; set; } = new();
    public IReadOnlyList<DestinationResponseDto> Destinations { get; set; } = Array.Empty<DestinationResponseDto>();
    public IReadOnlyList<ActivityResponseDto> Activities { get; set; } = Array.Empty<ActivityResponseDto>();
    public IReadOnlyList<ChecklistItemResponseDto> ChecklistItems { get; set; } = Array.Empty<ChecklistItemResponseDto>();
    public BudgetSummaryDto? BudgetSummary { get; set; }
    public IReadOnlyList<ExpenseResponseDto> Expenses { get; set; } = Array.Empty<ExpenseResponseDto>();
}

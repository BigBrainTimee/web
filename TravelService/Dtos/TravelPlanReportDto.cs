namespace TravelService.Dtos;

public class TravelPlanReportDto
{
    public TravelPlanResponseDto Plan { get; set; } = new();
    public IReadOnlyList<DestinationResponseDto> Destinations { get; set; } = Array.Empty<DestinationResponseDto>();
    public IReadOnlyList<ActivityResponseDto> Activities { get; set; } = Array.Empty<ActivityResponseDto>();
    public IReadOnlyList<ChecklistItemResponseDto> ChecklistItems { get; set; } = Array.Empty<ChecklistItemResponseDto>();
    public IReadOnlyList<ExpenseResponseDto> Expenses { get; set; } = Array.Empty<ExpenseResponseDto>();
    public BudgetSummaryDto? BudgetSummary { get; set; }
}

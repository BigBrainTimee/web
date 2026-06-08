namespace TravelService.Dtos;

public class ChecklistItemResponseDto
{
    public int Id { get; set; }
    public int TravelPlanId { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public int SortOrder { get; set; }
}

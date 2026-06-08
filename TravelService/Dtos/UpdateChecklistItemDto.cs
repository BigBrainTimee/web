using System.ComponentModel.DataAnnotations;

namespace TravelService.Dtos;

public class UpdateChecklistItemDto
{
    [Required]
    [StringLength(300, MinimumLength = 2)]
    public string Title { get; set; } = string.Empty;

    public int SortOrder { get; set; }
}

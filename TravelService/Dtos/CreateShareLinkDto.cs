using System.ComponentModel.DataAnnotations;

namespace TravelService.Dtos;

public class CreateShareLinkDto
{
    [Required]
    public string AccessType { get; set; } = "View";

    public DateTime? ExpiresAt { get; set; }
}

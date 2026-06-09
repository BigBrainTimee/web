using System.ComponentModel.DataAnnotations;

namespace AuthService.Dtos;

public class UpdateUserRoleDto
{
    [Required]
    public string Role { get; set; } = string.Empty;
}

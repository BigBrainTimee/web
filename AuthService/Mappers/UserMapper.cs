using AuthService.Dtos;
using AuthService.Models;

namespace AuthService.Mappers;

public static class UserMapper
{
    public static UserResponseDto ToResponseDto(User user)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Role = user.Role,
            CreatedAt = user.CreatedAt
        };
    }

    public static User ToEntity(RegisterRequestDto dto, string passwordHash)
    {
        return new User
        {
            Name = dto.Name.Trim(),
            Email = dto.Email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = "User",
            CreatedAt = DateTime.UtcNow
        };
    }

    public static string NormalizeRole(string role)
    {
        if (!role.Equals("User", StringComparison.OrdinalIgnoreCase)
            && !role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Invalid role. Use User or Admin.");
        }

        return role.Equals("Admin", StringComparison.OrdinalIgnoreCase) ? "Admin" : "User";
    }
}

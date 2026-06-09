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
        return ToEntity(dto.Name, dto.Email, passwordHash, "User");
    }

    public static User ToEntity(string name, string email, string passwordHash, string role)
    {
        return new User
        {
            Name = name.Trim(),
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Role = NormalizeRole(role),
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

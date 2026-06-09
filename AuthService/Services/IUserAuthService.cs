using AuthService.Dtos;

namespace AuthService.Services;

public interface IUserAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<UserResponseDto?> GetCurrentUserAsync(int userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<UserResponseDto>> GetAllUsersAsync(CancellationToken cancellationToken = default);
    Task<UserResponseDto?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<UserResponseDto> CreateUserAsync(AdminCreateUserDto request, CancellationToken cancellationToken = default);
    Task<UserResponseDto?> UpdateUserRoleAsync(int adminUserId, int targetUserId, UpdateUserRoleDto dto, CancellationToken cancellationToken = default);
    Task<bool> DeleteUserAsync(int adminUserId, int targetUserId, CancellationToken cancellationToken = default);
}

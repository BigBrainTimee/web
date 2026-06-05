using AuthService.Dtos;

namespace AuthService.Services;

public interface IUserAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
    Task<UserResponseDto?> GetCurrentUserAsync(int userId, CancellationToken cancellationToken = default);
}

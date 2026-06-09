using AuthService.Data;
using AuthService.Dtos;
using AuthService.Mappers;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Services;

public class UserAuthService : IUserAuthService
{
    private readonly AuthDbContext _dbContext;
    private readonly IJwtTokenService _jwtTokenService;

    public UserAuthService(AuthDbContext dbContext, IJwtTokenService jwtTokenService)
    {
        _dbContext = dbContext;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var emailExists = await _dbContext.Users
            .AnyAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (emailExists)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = UserMapper.ToEntity(request, passwordHash);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return BuildAuthResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        return BuildAuthResponse(user);
    }

    public async Task<UserResponseDto?> GetCurrentUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        return user is null ? null : UserMapper.ToResponseDto(user);
    }

    public async Task<IReadOnlyList<UserResponseDto>> GetAllUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await _dbContext.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync(cancellationToken);

        return users.Select(UserMapper.ToResponseDto).ToList();
    }

    public async Task<UserResponseDto?> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _dbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        return user is null ? null : UserMapper.ToResponseDto(user);
    }

    public async Task<UserResponseDto> CreateUserAsync(
        AdminCreateUserDto request,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = request.Email.Trim().ToLowerInvariant();

        var emailExists = await _dbContext.Users
            .AnyAsync(u => u.Email == normalizedEmail, cancellationToken);

        if (emailExists)
        {
            throw new InvalidOperationException("A user with this email already exists.");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        var user = UserMapper.ToEntity(request.Name, request.Email, passwordHash, request.Role);

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return UserMapper.ToResponseDto(user);
    }

    public async Task<UserResponseDto?> UpdateUserRoleAsync(
        int adminUserId,
        int targetUserId,
        UpdateUserRoleDto dto,
        CancellationToken cancellationToken = default)
    {
        if (adminUserId == targetUserId)
        {
            throw new InvalidOperationException("You cannot change your own role.");
        }

        var role = UserMapper.NormalizeRole(dto.Role);

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == targetUserId, cancellationToken);

        if (user is null)
        {
            return null;
        }

        user.Role = role;
        await _dbContext.SaveChangesAsync(cancellationToken);

        return UserMapper.ToResponseDto(user);
    }

    public async Task<bool> DeleteUserAsync(int adminUserId, int targetUserId, CancellationToken cancellationToken = default)
    {
        if (adminUserId == targetUserId)
        {
            throw new InvalidOperationException("You cannot delete your own account.");
        }

        var user = await _dbContext.Users
            .FirstOrDefaultAsync(u => u.Id == targetUserId, cancellationToken);

        if (user is null)
        {
            return false;
        }

        _dbContext.Users.Remove(user);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    private AuthResponseDto BuildAuthResponse(Models.User user)
    {
        var (token, expiresAt) = _jwtTokenService.GenerateToken(user);

        return new AuthResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            User = UserMapper.ToResponseDto(user)
        };
    }
}

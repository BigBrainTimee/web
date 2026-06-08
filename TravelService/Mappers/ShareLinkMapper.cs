using System.Security.Cryptography;
using TravelService.Dtos;
using TravelService.Models;

namespace TravelService.Mappers;

public static class ShareLinkMapper
{
    private static readonly HashSet<string> ValidAccessTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "View", "Edit"
    };

    public static ShareLinkResponseDto ToResponseDto(ShareLink link)
    {
        return new ShareLinkResponseDto
        {
            Id = link.Id,
            TravelPlanId = link.TravelPlanId,
            Token = link.Token,
            AccessType = link.AccessType,
            CreatedAt = ToUtc(link.CreatedAt),
            ExpiresAt = ToUtc(link.ExpiresAt)
        };
    }

    public static ShareLink ToEntity(CreateShareLinkDto dto, int travelPlanId)
    {
        return new ShareLink
        {
            TravelPlanId = travelPlanId,
            Token = GenerateToken(),
            AccessType = NormalizeAccessType(dto.AccessType),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = ToUtc(dto.ExpiresAt)
        };
    }

    public static string NormalizeAccessType(string accessType)
    {
        if (!ValidAccessTypes.Contains(accessType))
        {
            throw new ArgumentException("Invalid access type. Use View or Edit.");
        }

        return ValidAccessTypes.First(t => t.Equals(accessType, StringComparison.OrdinalIgnoreCase));
    }

    private static string GenerateToken()
    {
        return Convert.ToHexString(RandomNumberGenerator.GetBytes(32)).ToLowerInvariant();
    }

    public static DateTime ToUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
        };
    }

    public static DateTime? ToUtc(DateTime? value)
    {
        return value.HasValue ? ToUtc(value.Value) : null;
    }
}

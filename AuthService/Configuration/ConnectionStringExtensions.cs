using Microsoft.Extensions.Configuration;

namespace AuthService.Configuration;

public static class ConnectionStringExtensions
{
    public static string GetRequiredConnectionString(this IConfiguration configuration, params string[] names)
    {
        foreach (var name in names)
        {
            var value = configuration.GetConnectionString(name);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        throw new InvalidOperationException(
            $"Connection string is not configured. Expected one of: {string.Join(", ", names)}.");
    }
}

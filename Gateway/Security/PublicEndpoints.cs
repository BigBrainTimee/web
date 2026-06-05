namespace Gateway.Security;

public static class PublicEndpoints
{
    private static readonly HashSet<(string Method, string Path)> Allowed = new()
    {
        ("POST", "/api/auth/register"),
        ("POST", "/api/auth/login"),
        ("GET", "/api/auth/health"),
        ("GET", "/api/travel/health"),
        ("GET", "/api/budget/health"),
        ("GET", "/api/gateway/health")
    };

    public static bool IsPublic(string method, string path)
    {
        var normalizedPath = path.TrimEnd('/');
        if (string.IsNullOrEmpty(normalizedPath))
        {
            return false;
        }

        return Allowed.Contains((method.ToUpperInvariant(), normalizedPath));
    }
}

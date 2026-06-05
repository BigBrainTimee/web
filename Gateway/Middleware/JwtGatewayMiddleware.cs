using Gateway.Security;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace Gateway.Middleware;

public class JwtGatewayMiddleware
{
    private readonly RequestDelegate _next;

    public JwtGatewayMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        if (!path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase)
            || PublicEndpoints.IsPublic(context.Request.Method, path))
        {
            await _next(context);
            return;
        }

        var authResult = await context.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
        if (!authResult.Succeeded)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { message = "Unauthorized. Valid JWT token required." });
            return;
        }

        context.User = authResult.Principal!;
        await _next(context);
    }
}

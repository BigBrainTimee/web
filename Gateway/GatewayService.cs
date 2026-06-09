using System.Fabric;
using System.IO;
using System.Text;
using Gateway.Configuration;
using Gateway.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace Gateway;

internal sealed class GatewayService : StatelessService
{
    public GatewayService(StatelessServiceContext context)
        : base(context)
    {
    }

    protected override IEnumerable<ServiceInstanceListener> CreateServiceInstanceListeners()
    {
        return new ServiceInstanceListener[]
        {
            new ServiceInstanceListener(serviceContext =>
                new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                {
                    ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                    var builder = WebApplication.CreateBuilder();

                    builder.Services.AddSingleton(serviceContext);
                    builder.Configuration
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true);

                    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));

                    var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()
                        ?? throw new InvalidOperationException("JWT settings are missing.");

                    builder.Services
                        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                        .AddJwtBearer(options =>
                        {
                            options.TokenValidationParameters = new TokenValidationParameters
                            {
                                ValidateIssuer = true,
                                ValidateAudience = true,
                                ValidateLifetime = true,
                                ValidateIssuerSigningKey = true,
                                ValidIssuer = jwtSettings.Issuer,
                                ValidAudience = jwtSettings.Audience,
                                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Secret)),
                                ClockSkew = TimeSpan.FromMinutes(1)
                            };
                        });

                    builder.Services.AddAuthorization();
                    builder.Services.AddCors(options =>
                    {
                        options.AddPolicy("Frontend", policy =>
                        {
                            policy.SetIsOriginAllowed(origin =>
                            {
                                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                                {
                                    return false;
                                }

                                return (uri.Host is "localhost" or "127.0.0.1")
                                    && uri.Port is >= 5173 and <= 5200;
                            })
                                .AllowAnyHeader()
                                .AllowAnyMethod();
                        });
                    });
                    builder.Services.AddControllers();
                    builder.Services.AddReverseProxy()
                        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

                    builder.WebHost
                        .UseKestrel()
                        .UseContentRoot(Directory.GetCurrentDirectory())
                        .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                        .UseUrls(url);

                    var app = builder.Build();

                    app.UseCors("Frontend");
                    app.UseAuthentication();
                    app.UseMiddleware<JwtGatewayMiddleware>();
                    app.UseAuthorization();
                    app.MapControllers();
                    app.MapReverseProxy();

                    return app;
                }))
        };
    }
}

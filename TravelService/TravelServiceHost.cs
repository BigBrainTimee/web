using System.Fabric;
using System.IO;
using System.Text;
using Microsoft.Extensions.Hosting;
using TravelService.Clients;
using TravelService.Configuration;
using TravelService.Data;
using TravelService.Json;
using TravelService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Microsoft.ServiceFabric.Services.Communication.AspNetCore;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;

namespace TravelService;

internal sealed class TravelServiceHost : StatefulService
{
    public TravelServiceHost(StatefulServiceContext context)
        : base(context)
    {
    }

    protected override async Task RunAsync(CancellationToken cancellationToken)
    {
        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
    {
        return new ServiceReplicaListener[]
        {
            new ServiceReplicaListener(serviceContext =>
                new KestrelCommunicationListener(serviceContext, "ServiceEndpoint", (url, listener) =>
                {
                    ServiceEventSource.Current.ServiceMessage(serviceContext, $"Starting Kestrel on {url}");

                    var contentRoot = AppContext.BaseDirectory;
                    var builder = WebApplication.CreateBuilder(new WebApplicationOptions
                    {
                        ContentRootPath = contentRoot,
                    });

                    builder.Services.AddSingleton(serviceContext);
                    builder.Configuration
                        .SetBasePath(contentRoot)
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true);

                    builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
                    builder.Services.Configure<ServiceUrlsSettings>(builder.Configuration.GetSection("ServiceUrls"));
                    builder.Services.AddHttpContextAccessor();
                    builder.Services.AddHttpClient<IBudgetClient, BudgetClient>()
                        .ConfigureHttpClient(client => client.Timeout = TimeSpan.FromSeconds(15));
                    builder.Services.AddDbContext<TravelDbContext>(options =>
                        options.UseSqlServer(builder.Configuration.GetRequiredConnectionString("TravelDb", "TravelPlannerDb")));
                    builder.Services.AddScoped<ITravelPlanService, TravelPlanService>();
                    builder.Services.AddSingleton<IPlanReportPdfGenerator, PlanReportPdfGenerator>();

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
                    builder.Services.AddControllers()
                        .AddJsonOptions(options =>
                        {
                            options.JsonSerializerOptions.Converters.Add(new NullableTimeOnlyJsonConverter());
                        });
                    builder.Services.Configure<HostOptions>(options =>
                    {
                        options.ShutdownTimeout = TimeSpan.FromSeconds(15);
                    });

                    builder.WebHost
                        .UseKestrel()
                        .UseContentRoot(contentRoot)
                        .UseServiceFabricIntegration(listener, ServiceFabricIntegrationOptions.None)
                        .UseUrls(url);

                    var app = builder.Build();

                    app.UseAuthentication();
                    app.UseAuthorization();
                    app.MapControllers();

                    return app;
                }))
        };
    }
}

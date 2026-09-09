using GHCAA.API.Extensions;
using GHCAA.API.Middleware;
using GHCAA.API.Services;
using GHCAA.API.Utils;
using GHCAA.Application;
using GHCAA.Application.Interfaces;
using GHCAA.Application.Security;
using GHCAA.Domain;
using static GHCAA.Domain.Constants;
using GHCAA.Infrastructure;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Services;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Server.Kestrel.Core;

// Load environment variables from .env file (useful for local overrides)
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
var configuration = builder.Configuration;

// Bind to PORT if provided by cloud hosting platform (Render, Railway, etc.)
var listenPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(listenPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{listenPort}");
}

JwtSigningKeyResolver.Resolve(configuration, builder.Environment);

// Resolve application name from profile for DataProtection keyring
var profileForAppName = new InstitutionProfileProvider(configuration, builder.Environment);
var applicationName = profileForAppName.ProfileExplicitlySelected
    ? profileForAppName.OrgConfigDefaults.Branding.AppName
    : "GHCAA";

builder.Services.AddDataProtection()
    .PersistKeysToDbContext<ApplicationDbContext>()
    .SetApplicationName(applicationName);

// Register Core Layers & Services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(configuration);
builder.Services.AddMemoryCache();
builder.Services.AddAppCachingAndCompression();
builder.Services.AddJwtAuthentication(configuration, builder.Environment);
builder.Services.AddAppAuthorization();
builder.Services.AddAppRateLimiting(configuration, builder.Environment);

// Request Size & Form Limits
var maxBodySize = configuration.GetValue<long>("AppSettings:MaxRequestBodySize", 104857600);
builder.Services.Configure<KestrelServerOptions>(options =>
{
    options.Limits.MaxRequestBodySize = maxBodySize;
});
builder.Services.Configure<FormOptions>(x =>
{
    var capped = Math.Min(maxBodySize, 128L * 1024 * 1024);
    x.ValueLengthLimit = (int)Math.Min(capped, int.MaxValue);
    x.MultipartBodyLengthLimit = maxBodySize;
    x.MemoryBufferThreshold = (int)Math.Min(maxBodySize, int.MaxValue);
});

// ProblemDetails & Controllers
builder.Services.AddProblemDetails();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new DateFormatConverter());
        options.JsonSerializerOptions.Converters.Add(new NullableDateFormatConverter());
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
});

builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();
builder.Services.AddScoped<IRealTimeService, RealTimeService>();

builder.Services.AddCors(options =>
{
    var allowedOrigins = configuration.GetSection("AppSettings:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

    if (!builder.Environment.IsDevelopment() && allowedOrigins.Length == 0)
        throw new InvalidOperationException("AppSettings:AllowedOrigins must be configured in Production environments.");

    options.AddPolicy("AngularApp", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();

        if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost");
        }
    });
});

var app = builder.Build();

// Verify institution profile pack
var institutionProfile = app.Services.GetRequiredService<IInstitutionProfileProvider>();
if (!institutionProfile.ProfileExplicitlySelected)
{
    app.Logger.LogWarning(
        "ORG_PROFILE is not set. Institution configuration is being served from the hardcoded "
        + "defaults in OrgConfigService, not from profiles/. Set ORG_PROFILE (for this deployment: "
        + "ORG_PROFILE=ghc) to serve profiles/<name>/org-config.json instead. See docs/TODO.md 62.6.");
}

// Forwarded Headers for reverse proxy / TLS termination
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor
};
forwardedHeadersOptions.KnownNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

// Middleware Pipeline
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("AngularApp");

if (app.Environment.IsDevelopment())
{
    var swaggerTitle = institutionProfile.ProfileExplicitlySelected
        ? $"{institutionProfile.OrgConfigDefaults.Branding.ShortName} API V1"
        : "GHCAA API V1";

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", swaggerTitle);
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
    });
}

app.UseResponseCompression();
app.UseOutputCache();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<AuditLogMiddleware>();
app.UseRateLimiter();
app.UseWebSockets();

// Static Files & Uploads Middleware
app.UseAppStaticFiles(configuration, app.Environment);

app.UseAuthentication();
if (app.Environment.IsDevelopment() && app.Configuration["ASP_SEED_PROFILE"] == "Visual")
    app.UseMiddleware<VisualTestAuthMiddleware>();

app.UseMiddleware<SecurityStampMiddleware>();
app.UseMiddleware<XsrfMiddleware>();
app.UseAuthorization();

// Endpoints
app.MapControllers().RequireRateLimiting(RateLimitPolicies.Api);
app.MapHealthChecks("/health").AllowAnonymous();
app.MapHub<GHCAA.API.Hubs.ChatHub>("/api/hubs/chat");
app.MapHub<GHCAA.API.Hubs.NotificationHub>("/api/hubs/notifications");

// SPA Fallback Routing
app.MapSpaFallback(app.Environment);

// Boot-time Database Migrations & Seeding
await app.BootstrapDatabaseAsync();

app.Run();
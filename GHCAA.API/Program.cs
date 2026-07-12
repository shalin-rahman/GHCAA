using GHCAA.Application;
using GHCAA.Infrastructure;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using GHCAA.API.Extensions;
using GHCAA.API.Middleware;
using GHCAA.Application.Interfaces;
using GHCAA.Application.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;

// Load environment variables from .env file (useful for local overrides)
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
var configuration = builder.Configuration;

JwtSigningKeyResolver.Resolve(configuration, builder.Environment);

var keyRingPath = configuration["DataProtection:KeyRingPath"];
if (!string.IsNullOrWhiteSpace(keyRingPath))
{
    Directory.CreateDirectory(keyRingPath);
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keyRingPath))
        .SetApplicationName("GHCAA");
}

// Register layers
builder.Services.AddApplication();
builder.Services.AddInfrastructure(configuration);
builder.Services.AddMemoryCache();

// 2. Configure Response Compression (Brotli/Gzip)
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.MimeTypes = Microsoft.AspNetCore.ResponseCompression.ResponseCompressionDefaults.MimeTypes.Concat(
        new[] { "application/json", "image/svg+xml" });
});

// 3. Configure Output Caching (.NET 8+)
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(builder => builder.Cache());
    options.AddPolicy("StaticData", builder => 
        builder.Expire(TimeSpan.FromMinutes(5)).SetVaryByQuery("*"));
});

// Configure JWT Authentication
builder.Services.AddJwtAuthentication(configuration, builder.Environment);
builder.Services.AddAppAuthorization();

// 1. Configure Rate Limiting (Fixed Window)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    bool isTestEnv = configuration["ASP_SEED_PROFILE"] == "Visual" || builder.Environment.IsDevelopment();

    // 24.48: Login policy keyed per (IP, username) — each distinct caller gets its own
    // 5-req/min bucket so a single attacker cannot consume the global quota.
    options.AddPolicy<string>("auth", httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var username = httpContext.Items.TryGetValue("LoginUsername", out var u) ? u?.ToString() ?? "" : "";
        var key = isTestEnv ? "__test__" : $"{ip}:{username}";
        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            Window = TimeSpan.FromMinutes(1),
            PermitLimit = isTestEnv ? 500 : 5,
            QueueLimit = 0
        });
    });

    // 3c: Refresh policy — keyed per IP, lenient enough for legit silent-refresh retries
    // but bounded so a stolen/guessed refresh token can't be replayed unlimited times.
    options.AddPolicy<string>("refresh", httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var key = isTestEnv ? "__test__" : ip;
        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            Window = TimeSpan.FromMinutes(1),
            PermitLimit = isTestEnv ? 500 : 20,
            QueueLimit = 0
        });
    });

    // Registration Policy: Moderate (10 requests per 5 minutes)
    options.AddFixedWindowLimiter("registration", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(5);
        opt.PermitLimit = isTestEnv ? 1000 : 10;
        opt.QueueLimit = 0;
    });

    // General API Policy: (100 requests per 1 minute)
    options.AddFixedWindowLimiter("api", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = isTestEnv ? 10000 : 100;
        opt.QueueLimit = 2;
        opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });
});

// Configure Request Limits from Settings
var maxBodySize = configuration.GetValue<long>("AppSettings:MaxRequestBodySize", 104857600);
builder.Services.Configure<Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions>(options => 
{
    options.Limits.MaxRequestBodySize = maxBodySize;
});
builder.Services.Configure<Microsoft.AspNetCore.Http.Features.FormOptions>(x =>
{
    var capped = Math.Min(maxBodySize, 128L * 1024 * 1024);
    x.ValueLengthLimit = (int)Math.Min(capped, int.MaxValue);
    x.MultipartBodyLengthLimit = maxBodySize;
    x.MemoryBufferThreshold = (int)Math.Min(maxBodySize, int.MaxValue);
});

builder.Services.AddControllers()
    .AddJsonOptions(options => {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new GHCAA.API.Utils.DateFormatConverter());
        options.JsonSerializerOptions.Converters.Add(new GHCAA.API.Utils.NullableDateFormatConverter());
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddSignalR();
builder.Services.AddScoped<GHCAA.Application.Interfaces.IRealTimeService, GHCAA.API.Services.RealTimeService>();

builder.Services.AddCors(options =>
{
    var allowedOrigins = configuration.GetSection("AppSettings:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

    // 24.49: Fail fast in Production when AllowedOrigins is not configured.
    // An empty list would silently block all cross-origin requests (or allow all via the dev fallback).
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

// Use Exception Middleware first to catch all subsequent errors
app.UseMiddleware<ExceptionMiddleware>();

app.UseCors("AngularApp");
 
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GHCAA API V1");
        c.DisplayRequestDuration();
        c.EnableDeepLinking();
    });
}

// Use Response Compression and Output Caching
app.UseResponseCompression();
app.UseOutputCache();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<AuditLogMiddleware>();
app.UseMiddleware<LoginRateLimitMiddleware>(); // 24.48: peek username before rate limiter

app.UseRateLimiter(); // Apply Rate Limiting

app.UseWebSockets();

app.UseStaticFiles(); // serve wwwroot at the root /

// Map /api/ paths to wwwroot so the frontend can retrieve the physical images 
// when it concatenates the API base URL with the database's relative 'uploads/...' path.
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot", "uploads")),
    RequestPath = "/api/uploads"
});

app.UseMiddleware<QueryStringTokenMiddleware>();
app.UseAuthentication();
if (app.Environment.IsDevelopment() && app.Configuration["ASP_SEED_PROFILE"] == "Visual")
    app.UseMiddleware<GHCAA.API.Middleware.VisualTestAuthMiddleware>();
app.UseMiddleware<SecurityStampMiddleware>(); // Invalidates sessions on status change
app.UseMiddleware<GHCAA.API.Middleware.XsrfMiddleware>(); // 1a: CSRF protection for cookie-authenticated clients
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");
app.MapHub<GHCAA.API.Hubs.ChatHub>("/api/hubs/chat");
app.MapHub<GHCAA.API.Hubs.NotificationHub>("/api/hubs/notifications");

// Seed OrganizationConfig with GHCAA defaults on first boot (idempotent, fault-tolerant)
// Wrapped in try/catch so a missing table (pre-migration) or transient DB error never prevents boot.
try
{
    using var scope = app.Services.CreateScope();
    var dbCtx = scope.ServiceProvider.GetRequiredService<GHCAA.Infrastructure.Data.ApplicationDbContext>();
    if (await dbCtx.Database.CanConnectAsync() && !await dbCtx.OrganizationConfigs.AnyAsync())
    {
        var configService = scope.ServiceProvider.GetRequiredService<GHCAA.Application.Interfaces.IOrgConfigService>();
        // GetConfigAsync returns in-memory defaults when DB is empty; persist them so PUT works from day one
        var defaults = await configService.GetConfigAsync();
        await configService.UpdateConfigAsync(defaults, "system");
    }
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "OrgConfig seed skipped — table may not exist yet. Run migrations first.");
}

// Automatic Database Initialization for Visual Testing Profile
if (app.Configuration["ASP_SEED_PROFILE"] == "Visual")
{
    using var scope = app.Services.CreateScope();
    var context = scope.ServiceProvider.GetRequiredService<GHCAA.Infrastructure.Data.ApplicationDbContext>();

    if (app.Configuration.GetValue<bool>("AppSettings:RecreateDatabaseOnStartup"))
    {
        context.Database.EnsureDeleted();
    }
    context.Database.EnsureCreated();

    // MANUAL SEEDING: Force override EF Core snapshots with fresh data from Seed/Visual
    OverrideEFCoreMigratedData(context);
}

app.Run();

static void OverrideEFCoreMigratedData(GHCAA.Infrastructure.Data.ApplicationDbContext context)
{
    var infrastructurePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "GHCAA.Infrastructure");
    if (!Directory.Exists(infrastructurePath)) infrastructurePath = Path.Combine(Directory.GetCurrentDirectory(), "GHCAA.Infrastructure");

    var usersJsonPath = Path.Combine(infrastructurePath, "Data", "Seed", "Visual", "users.json");
    if (File.Exists(usersJsonPath))
    {
        var json = File.ReadAllText(usersJsonPath);
        var users = System.Text.Json.JsonSerializer.Deserialize<List<GHCAA.Domain.Models.User>>(json);
        if (users != null)
        {
            // Clear existing users to remove snapshot-seeded data
            context.Users.RemoveRange(context.Users);
            context.SaveChanges();

            foreach (var user in users)
            {
                context.Users.Add(user);
            }
            context.SaveChanges();
            Console.WriteLine($"[SEED] Authoritatively seeded {users.Count} users from {usersJsonPath}");
        }
    }
}
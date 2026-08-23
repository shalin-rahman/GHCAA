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

// Hosting platforms (Render, Railway, Heroku, etc.) inject the port to listen on via the
// PORT env var. Bind to it so the platform's health probe finds an open socket; otherwise
// it never detects the app, times out, and SIGTERMs startup (Kestrel BindAsync → TaskCanceledException).
var listenPort = Environment.GetEnvironmentVariable("PORT");
if (!string.IsNullOrWhiteSpace(listenPort))
{
    builder.WebHost.UseUrls($"http://0.0.0.0:{listenPort}");
}

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

    // 29B.5: Login policy keyed per source IP (not per {ip,username}). The previous per-username
    // key handed each distinct username its own 5/min bucket, so one IP could password-spray
    // thousands of accounts (N usernames × 5/min). Keying on IP alone bounds the total auth
    // attempts a single source can make regardless of how many accounts it targets.
    options.AddPolicy<string>("auth", httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var key = isTestEnv ? "__test__" : ip;
        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            Window = TimeSpan.FromMinutes(1),
            PermitLimit = isTestEnv ? 500 : 10,
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
    .AddJsonOptions(options =>
    {
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

// 29B.4: Removed QueryStringTokenMiddleware. Accepting the JWT via ?token=/?access_token=
// leaked it into proxy/access logs and the browser Referer header. No client relies on it —
// secure files are fetched with the Authorization header — so the query-token path was pure
// attack surface with no functional use.
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

// SPA fallback: serve the Angular index.html for any non-API, non-file route so
// client-side deep links (e.g. /portal/members) resolve on refresh. Only active
// when the SPA has been copied into wwwroot (Docker multi-stage build).
var spaIndexPath = Path.Combine(app.Environment.WebRootPath ?? "wwwroot", "index.html");
if (File.Exists(spaIndexPath))
{
    app.MapFallback(async ctx =>
    {
        // Keep unknown /api requests as API 404s — never swallow them with index.html.
        if (ctx.Request.Path.StartsWithSegments("/api"))
        {
            ctx.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }
        ctx.Response.ContentType = "text/html";
        await ctx.Response.SendFileAsync(spaIndexPath);
    }).AllowAnonymous(); // exempt the SPA shell from the global RequireAuthenticatedUser FallbackPolicy
}

// Ensure the database schema exists on boot for non-Visual profiles (Preprod/Production).
// EnsureCreated builds the schema + HasData seed from the model on an empty database; it is a
// no-op once the tables exist. The Visual profile has its own recreate/seed path below.
// Migrations are not applied at runtime for this project.
if (app.Configuration["ASP_SEED_PROFILE"] != "Visual")
{
    using var schemaScope = app.Services.CreateScope();
    var schemaCtx = schemaScope.ServiceProvider.GetRequiredService<GHCAA.Infrastructure.Data.ApplicationDbContext>();
    schemaCtx.Database.EnsureCreated();
}

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

// Publish the ratified constitution from Data/Seed/constitution.json (TODO 36.3).
// EnsureCreated above is a no-op on an existing database, so the model's HasData seed never
// re-runs there; without this, a newly ratified version could never reach preprod. The syncer is
// idempotent and only supersedes prior versions, so member amendment votes are preserved.
try
{
    using var constitutionScope = app.Services.CreateScope();
    var constitutionCtx = constitutionScope.ServiceProvider.GetRequiredService<GHCAA.Infrastructure.Data.ApplicationDbContext>();
    if (await constitutionCtx.Database.CanConnectAsync())
    {
        await GHCAA.Infrastructure.Data.ConstitutionSeeder.SyncAsync(constitutionCtx, app.Logger);
    }
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "Constitution sync skipped — table may not exist yet.");
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
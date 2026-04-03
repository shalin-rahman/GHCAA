using System.Text;
using GHCAA.Application;
using GHCAA.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using GHCAA.API.Extensions;
using GHCAA.API.Middleware;
using GHCAA.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.DataProtection;

// Load environment variables from .env file (useful for local overrides)
DotNetEnv.Env.Load();

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
var configuration = builder.Configuration;

ValidateJwtConfiguration(configuration, builder.Environment);

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

// Configure JWT Authentication
builder.Services.AddJwtAuthentication(configuration, builder.Environment);
builder.Services.AddAppAuthorization();

// 1. Configure Rate Limiting (Fixed Window)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // Login/OTP Policy: Very strict (5 requests per 1 minute)
    options.AddFixedWindowLimiter("auth", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 5;
        opt.QueueLimit = 0;
    });

    // Registration Policy: Moderate (10 requests per 5 minutes)
    options.AddFixedWindowLimiter("registration", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(5);
        opt.PermitLimit = 10;
        opt.QueueLimit = 0;
    });

    // General API Policy: (100 requests per 1 minute)
    options.AddFixedWindowLimiter("api", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100;
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

    options.AddPolicy("AngularApp", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();

        // In Development, we automatically allow any localhost port to support Flutter Web debugging
        if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost");
        }
    });
});

var app = builder.Build();

// Auto-apply Entity Framework migrations at startup for deployments like Render
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<GHCAA.Infrastructure.Data.ApplicationDbContext>();
        
        var recreateDb = configuration.GetValue<bool>("AppSettings:RecreateDatabaseOnStartup", false);
        if (recreateDb)
        {
            context.Database.EnsureDeleted();
        }

        if (context.Database.IsRelational())
        {
            context.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or initializing the database.");
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AngularApp");

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<AuditLogMiddleware>();

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

app.UseAuthentication();
app.UseMiddleware<SecurityStampMiddleware>(); // Invalidates sessions on status change
app.UseAuthorization();
app.MapControllers();
app.MapHub<GHCAA.API.Hubs.ChatHub>("/hubs/chat");
app.Run();

static void ValidateJwtConfiguration(IConfiguration configuration, IHostEnvironment environment)
{
    var key = configuration["Jwt:Key"];
    if (environment.IsDevelopment())
    {
        if (string.IsNullOrWhiteSpace(key))
            return;
        if (key.Length < 32)
            throw new InvalidOperationException("Jwt:Key must be at least 32 characters when set.");
        return;
    }

    if (string.IsNullOrWhiteSpace(key) || key.Length < 32)
    {
        throw new InvalidOperationException(
            "Jwt:Key must be configured to a strong secret (minimum 32 characters) outside Development. " +
            "Set the Jwt__Key environment variable, User Secrets, or your host's secret store.");
    }
}

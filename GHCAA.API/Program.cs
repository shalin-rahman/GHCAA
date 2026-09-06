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
using Microsoft.AspNetCore.HttpOverrides;

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
    // Same guard as OrgConfigService.BuildDefaults(): an unset ORG_PROFILE keeps "GHCAA" exactly,
    // since changing this value invalidates every existing session token/cookie under the current
    // key ring (docs/TODO.md 62.10). Built directly here, before builder.Build(), because the DI
    // container that would normally hand out IInstitutionProfileProvider doesn't exist yet at this
    // point in startup.
    var profileForAppName = new GHCAA.Infrastructure.Services.InstitutionProfileProvider(configuration, builder.Environment);
    var applicationName = profileForAppName.ProfileExplicitlySelected
        ? profileForAppName.OrgConfigDefaults.Branding.AppName
        : "GHCAA";

    Directory.CreateDirectory(keyRingPath);
    builder.Services.AddDataProtection()
        .PersistKeysToFileSystem(new DirectoryInfo(keyRingPath))
        .SetApplicationName(applicationName);
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
//
// Output Cache keys a response by URL alone — it does not vary by Cookie or Authorization
// unless a policy says to. The old base policy, builder.Cache(), cached every GET/HEAD 200
// by default, so an authenticated fixed-URL route (/api/financials/my-dues, /api/me/profile,
// any "me"-shaped route) would serve one member's cached response to the next member who
// hit the same URL inside the cache window, cookie or bearer token notwithstanding.
//
// Base policy is now NoCache. A controller action opts back in with
// [OutputCache(PolicyName = ...)] only when it's [AllowAnonymous] and returns the same body
// to every caller — see docs/TODO.md Work Package 80 for the endpoint-by-endpoint audit.
builder.Services.AddOutputCache(options =>
{
    options.AddBasePolicy(build => build.NoCache());

    // Reference data that changes rarely (lookups, governance/EC/constitution):
    // safe to hold for 2 minutes.
    options.AddPolicy(GHCAA.Domain.Constants.OutputCachePolicies.PublicReference, build =>
        build.Expire(TimeSpan.FromMinutes(2)).SetVaryByQuery("*"));

    // Content that admins edit more often (news, events, gallery, jobs, site content):
    // a shorter window keeps an edit visible sooner without giving up the cache hit on
    // the landing-page traffic that reads it.
    options.AddPolicy(GHCAA.Domain.Constants.OutputCachePolicies.PublicContent, build =>
        build.Expire(TimeSpan.FromSeconds(30)).SetVaryByQuery("*"));
});

// Configure JWT Authentication
builder.Services.AddJwtAuthentication(configuration, builder.Environment);
builder.Services.AddAppAuthorization();

// 1. Configure Rate Limiting (Fixed Window)
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    // SECURITY AUDIT (2026-08-29): was `configuration["ASP_SEED_PROFILE"] == "Visual" ||
    // IsDevelopment()` — a plain env var (ASP_SEED_PROFILE=Visual) set on the live Render service,
    // with no other symptom, silently collapsed every rate limit below to a shared "__test__"
    // bucket at 500-10000x the real limit. Visual profile is already required to ALSO be
    // Development everywhere else it's checked (see VisualTestAuthMiddleware's gate below), so
    // relaxed limits should only ever depend on being in Development, never on this env var alone.
    bool isTestEnv = builder.Environment.IsDevelopment();

    // 29B.5: Login policy keyed per source IP (not per {ip,username}). The previous per-username
    // key handed each distinct username its own 5/min bucket, so one IP could password-spray
    // thousands of accounts (N usernames × 5/min). Keying on IP alone bounds the total auth
    // attempts a single source can make regardless of how many accounts it targets.
    options.AddPolicy<string>(GHCAA.Domain.Constants.RateLimitPolicies.Auth, httpContext =>
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
    options.AddPolicy<string>(GHCAA.Domain.Constants.RateLimitPolicies.Refresh, httpContext =>
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
    options.AddFixedWindowLimiter(GHCAA.Domain.Constants.RateLimitPolicies.Registration, opt =>
    {
        opt.Window = TimeSpan.FromMinutes(5);
        opt.PermitLimit = isTestEnv ? 1000 : 10;
        opt.QueueLimit = 0;
    });

    // 80.16: self-service password reset request sends an email per call, so it needs to be
    // tighter than auth/refresh (which just check a password/token, no outbound side effect) —
    // otherwise this endpoint becomes a free way to spam a member's inbox or probe which
    // identifiers exist by other means (response time, delivery bounces, etc).
    options.AddPolicy<string>(GHCAA.Domain.Constants.RateLimitPolicies.PasswordReset, httpContext =>
    {
        var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var key = isTestEnv ? "__test__" : ip;
        return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
        {
            Window = TimeSpan.FromMinutes(15),
            PermitLimit = isTestEnv ? 1000 : 5,
            QueueLimit = 0
        });
    });

    // General API Policy: (100 requests per 1 minute)
    options.AddFixedWindowLimiter(GHCAA.Domain.Constants.RateLimitPolicies.Api, opt =>
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
builder.Services.AddSwaggerGen(c =>
{
    // 82.10a: EventsController.RegisterForEventForm/RegisterForEventJson deliberately share one
    // route, disambiguated at runtime by Content-Type ([Consumes] multipart vs. json) — a real,
    // working pattern, not a routing bug. Swashbuckle can't represent two operations under one
    // OpenAPI path item, so without this the generator throws outright rather than documenting one
    // route twice. Keeping the first (form) action's shape in the doc; the json variant is the
    // same DTO either way.
    c.ResolveConflictingActions(apiDescriptions => apiDescriptions.First());
});
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

// Resolved eagerly so a missing/invalid institution profile pack (docs/TODO.md 62.1) fails boot
// with a readable error instead of surfacing lazily on whatever request first needs it.
var institutionProfile = app.Services.GetRequiredService<GHCAA.Application.Interfaces.IInstitutionProfileProvider>();

// 62.6: with ORG_PROFILE unset, OrgConfigService keeps using its hardcoded defaults rather than
// serving the neutral "default" sample pack to whoever this deployment actually belongs to. That is
// the safe behaviour, but it is silent, so say it out loud once at boot — otherwise the profile
// packs look wired up while nothing reads them.
if (!institutionProfile.ProfileExplicitlySelected)
{
    app.Logger.LogWarning(
        "ORG_PROFILE is not set. Institution configuration is being served from the hardcoded "
        + "defaults in OrgConfigService, not from profiles/. Set ORG_PROFILE (for this deployment: "
        + "ORG_PROFILE=ghc) to serve profiles/<name>/org-config.json instead. See docs/TODO.md 62.6.");
}

// SECURITY AUDIT (2026-08-29): must run before everything else. Render terminates TLS at its edge
// and forwards to this container over plain HTTP with X-Forwarded-Proto: https — without this,
// Request.IsHttps is permanently false in production, which silently made THREE controls inert:
// SecurityHeadersMiddleware's HSTS header (gated on IsHttps, below), app.UseHsts() (added below),
// and app.UseHttpsRedirection() (which also can't resolve a redirect port without this). Render is
// the sole ingress, and its proxy IP isn't in a known private range, so KnownNetworks/KnownProxies
// must be cleared — accept the trust-any-forwarder tradeoff only because nothing but Render's edge
// can reach this container directly.
var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedFor
};
forwardedHeadersOptions.KnownNetworks.Clear();
forwardedHeadersOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeadersOptions);

// Use Exception Middleware first to catch all subsequent errors
app.UseMiddleware<ExceptionMiddleware>();

app.UseCors("AngularApp");

if (app.Environment.IsDevelopment())
{
    // Same guard as OrgConfigService.BuildDefaults(): an unset ORG_PROFILE keeps today's title
    // rather than switching to whatever the "default" sample pack says.
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

// Use Response Compression and Output Caching
app.UseResponseCompression();
app.UseOutputCache();

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
    app.UseHttpsRedirection();
}

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseMiddleware<AuditLogMiddleware>();

app.UseRateLimiter(); // Apply Rate Limiting

app.UseWebSockets();

// serve wwwroot at the root /. index.html must never be browser-cached: its <script> tags
// reference build-hashed filenames (main-*.js, chunk-*.js) that are deleted on every new
// deploy, so a cached copy of index.html from a prior deploy 404s on those old hashes until
// a hard refresh. The hashed JS/CSS themselves are fine to leave uncached (unhashed default)
// since their filename already changes whenever content changes.
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        if (ctx.File.Name.Equals("index.html", StringComparison.OrdinalIgnoreCase))
        {
            ctx.Context.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
            ctx.Context.Response.Headers.Pragma = "no-cache";
            ctx.Context.Response.Headers.Expires = "0";
        }
    }
});

// Map /api/ paths to the same physical root LocalFileStorageService writes to (and
// SecureFilesController reads from), so the frontend can retrieve the physical images
// when it concatenates the API base URL with the database's relative 'uploads/...' path.
// FileStorage:BasePhysicalPath defaults to ContentRootPath/wwwroot (this container's own
// ephemeral disk) but can be overridden to a mounted persistent volume in production.
var uploadsBasePath = builder.Configuration["FileStorage:BasePhysicalPath"]
    ?? Path.Combine(builder.Environment.ContentRootPath, "wwwroot");
var uploadsPhysicalPath = Path.Combine(uploadsBasePath, "uploads");
// PhysicalFileProvider throws DirectoryNotFoundException at construction if the root is absent —
// harmless today because wwwroot/uploads ships committed, but FileStorage:BasePhysicalPath is
// meant to be pointed at a freshly-mounted (empty) persistent disk in production, which would
// otherwise crash the app at boot.
Directory.CreateDirectory(uploadsPhysicalPath);
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(uploadsPhysicalPath),
    RequestPath = "/api/uploads"
});

// A GET/HEAD for a file-like path (has an extension) that neither UseStaticFiles block above
// served reaches here as a genuinely missing asset (e.g. a deleted upload) — return a plain 404
// instead of letting it fall through unmatched to the global RequireAuthenticatedUser
// FallbackPolicy, which would otherwise challenge it with a misleading 401.
//
// This MUST be plain middleware, not a MapFallback/routed endpoint. Endpoint routing matches
// routes before StaticFileMiddleware gets a turn, and StaticFileMiddleware unconditionally skips
// serving whenever context.GetEndpoint() is already non-null — so a routed catch-all matching
// file-like paths (as this used to be, via MapFallback("/{**path}") with no :nonfile constraint)
// silently disables static file serving for every asset, not just missing ones. Confirmed via
// Microsoft.AspNetCore.StaticFiles debug logging: "Static files was skipped as the request
// already matched an endpoint." Keep this as app.Use(...) so it never competes with routing.
var uploadImagePlaceholderPath = Path.Combine(app.Environment.WebRootPath ?? "wwwroot", "assets", "placeholders", "image-placeholder.svg");
var uploadImageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".svg" };
app.Use(async (ctx, next) =>
{
    var path = ctx.Request.Path;
    var lastSegment = path.Value?.Split('/').LastOrDefault() ?? "";
    var isFileLike = lastSegment.Contains('.');
    // Scoped to /api/uploads specifically (not all of /api) so this never shadows a real
    // controller route whose path happens to contain a dot (e.g. an email address segment).
    var isMissingUpload = path.StartsWithSegments("/api/uploads") && isFileLike;
    var isMissingNonApiAsset = !path.StartsWithSegments("/api") && isFileLike;
    if (isMissingUpload || isMissingNonApiAsset)
    {
        // A missing /uploads/* image (ephemeral Render disk wiped on redeploy, or a stale/bad
        // seed value) is expected to recur until a persistent disk is mounted — surfacing it as
        // a 404 just spams the browser console for something the UI already renders as a broken
        // image anyway. Substitute the same placeholder <img> callers already fall back to
        // on-error, with a real 200, so it's silent. Non-upload missing assets (e.g. a stale JS
        // chunk after a deploy) must keep failing loudly, so this is scoped to /uploads specifically.
        var isUploadPath = path.Value?.Contains("/uploads/", StringComparison.OrdinalIgnoreCase) == true;
        var isImage = uploadImageExtensions.Any(ext => lastSegment.EndsWith(ext, StringComparison.OrdinalIgnoreCase));
        if (isUploadPath && isImage && File.Exists(uploadImagePlaceholderPath))
        {
            ctx.Response.ContentType = "image/svg+xml";
            await ctx.Response.SendFileAsync(uploadImagePlaceholderPath);
            return;
        }

        ctx.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }
    await next(ctx);
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
// "api" (100/min) was registered above but never applied anywhere — every endpoint outside
// AuthController/RegistrationController had no rate limit at all. Applying it here as the floor
// for every controller action; [EnableRateLimiting("auth"/"refresh"/"registration")] on a specific
// action still applies on top of this, and [DisableRateLimiting] (AuthController /me, /logout)
// still overrides it.
app.MapControllers().RequireRateLimiting(GHCAA.Domain.Constants.RateLimitPolicies.Api);
app.MapHealthChecks("/health");
app.MapHub<GHCAA.API.Hubs.ChatHub>("/api/hubs/chat");
app.MapHub<GHCAA.API.Hubs.NotificationHub>("/api/hubs/notifications");

// SPA fallback: serve the Angular index.html for any non-API, non-file route so
// client-side deep links (e.g. /portal/members) resolve on refresh. Only active
// when the SPA has been copied into wwwroot (Docker multi-stage build).
var spaIndexPath = Path.Combine(app.Environment.WebRootPath ?? "wwwroot", "index.html");
if (File.Exists(spaIndexPath))
{
    // The ":nonfile" constraint is required — it excludes file-like paths (has an extension)
    // from matching this route at all, so real static assets stay fully handled by
    // UseStaticFiles above instead of being shadowed by this catch-all. See the app.Use(...)
    // 404 middleware above for how a missing file-like path still gets a plain 404.
    app.MapFallback("/{**path:nonfile}", async ctx =>
    {
        // Keep unknown /api requests as API 404s — never swallow them with index.html.
        if (ctx.Request.Path.StartsWithSegments("/api"))
        {
            ctx.Response.StatusCode = StatusCodes.Status404NotFound;
            return;
        }

        // Almost every real navigation (/, /portal/..., a refreshed deep link) lands here rather
        // than on an explicit GET /index.html, so this handler — not UseStaticFiles'
        // OnPrepareResponse above — is what actually serves the shell most of the time. It must
        // carry the same no-cache headers, or the "index.html never gets cached" guarantee above
        // is a no-op for normal traffic.
        ctx.Response.Headers.CacheControl = "no-cache, no-store, must-revalidate";
        ctx.Response.Headers.Pragma = "no-cache";
        ctx.Response.Headers.Expires = "0";
        ctx.Response.ContentType = "text/html";
        await ctx.Response.SendFileAsync(spaIndexPath);
    }).AllowAnonymous(); // exempt the SPA shell (and the 404s above) from the global RequireAuthenticatedUser FallbackPolicy
}

// Ensure the database schema is up to date on boot for non-Visual profiles (Preprod/Production).
// MigrationBootstrapper applies pending EF Core migrations automatically (baselining migration
// history first on a legacy EnsureCreated()-built database) so new columns/tables ship live
// without a manual step. The Visual profile has its own recreate/seed path below.
if (app.Configuration["ASP_SEED_PROFILE"] != "Visual")
{
    using var schemaScope = app.Services.CreateScope();
    var schemaCtx = schemaScope.ServiceProvider.GetRequiredService<GHCAA.Infrastructure.Data.ApplicationDbContext>();
    try
    {
        await GHCAA.Infrastructure.Data.MigrationBootstrapper.EnsureMigratedAsync(schemaCtx, app.Logger);
    }
    catch (Exception ex)
    {
        // Never let a migration-bootstrap failure take the whole app down: fall back to the
        // old no-op-on-existing-tables behavior so boot degrades to today's status quo instead
        // of a hard crash. Whatever caused this needs a human, not a retry loop.
        app.Logger.LogError(ex, "Migration bootstrap failed; falling back to EnsureCreated. Schema may be stale until this is fixed manually.");
        schemaCtx.Database.EnsureCreated();
    }
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

// Restore SuperAdmin on protected accounts (config-only list, not admin-UI-editable — see
// ProtectedSuperAdminSeeder for why this must never move into OrgConfig or a DB table).
// Must run LAST: the Visual-profile block above wipes and re-inserts every User row from
// Seed/Visual/users.json (which carries no role data), so restoring roles before that point
// gets silently undone (TODO 44.18).
try
{
    var protectedSuperAdmins = app.Configuration.GetSection("AppSettings:ProtectedSuperAdmins").Get<string[]>() ?? [];
    using var protectedAdminScope = app.Services.CreateScope();
    var protectedAdminCtx = protectedAdminScope.ServiceProvider.GetRequiredService<GHCAA.Infrastructure.Data.ApplicationDbContext>();
    if (await protectedAdminCtx.Database.CanConnectAsync())
    {
        await GHCAA.Infrastructure.Data.ProtectedSuperAdminSeeder.EnsureAsync(protectedAdminCtx, protectedSuperAdmins, app.Logger);
    }
}
catch (Exception ex)
{
    app.Logger.LogWarning(ex, "Protected SuperAdmin restore skipped.");
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
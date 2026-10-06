using System.Linq;
using Microsoft.AspNetCore.Hosting;
using global::Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace GHCAA.Tests.Integration
{
    // Boots the real GHCAA.API pipeline (Program.cs, unmodified) in Production so the real
    // rate limits apply (Auth = 10/min, not the Development 500/min). Migrations are only ever
    // attributed to the Postgres shim context (see DependencyInjection.cs), so on the Sqlite
    // provider MigrationBootstrapper sees zero migrations and no-ops, leaving no schema at all.
    // ASP_SEED_PROFILE=Visual takes the EnsureCreated() branch instead, which builds the schema
    // from the current model. That flag only changes DatabaseBootstrapperExtensions's seeding
    // path — the rate limiter checks IsDevelopment() directly, so this stays independent of it
    // and Production's real limits still apply.
    internal class ErrorResponseShapeTestFactory : WebApplicationFactory<Program>, IDisposable
    {
        private readonly string _contentRoot;
        private readonly string _dbPath;

        public const string JwtKey = "error-shape-test-dummy-key-please-32chars";
        public const string JwtIssuer = "GHCAA";
        public const string JwtAudience = "GHCAA";

        // Same early-override reasoning as SpaStaticFileFactory: JwtSigningKeyResolver.Resolve()
        // runs before WebApplicationFactory's config hooks can inject anything, so real process
        // env vars are the only override mechanism early enough.
        // A machine with ASPNETCORE_ENVIRONMENT=Development loads that file's origins under this
        // Production host, and the startup check refuses them. Filling the first slots hides them.
        private static readonly string[] OriginKeys = Enumerable.Range(0, 5).Select(i => $"AppSettings__AllowedOrigins__{i}").ToArray();

        private static readonly string[] EnvKeys =
        [
            "ASP_SEED_PROFILE", "Jwt__Key", "DatabaseProvider", "ConnectionStrings__SqliteConnection", "AppSettings__ClientUrl", .. OriginKeys,
        ];

        public ErrorResponseShapeTestFactory()
        {
            _contentRoot = Path.Combine(Path.GetTempPath(), "ghcaa-error-shape-test-" + Guid.NewGuid());
            var webRoot = Path.Combine(_contentRoot, "wwwroot");
            Directory.CreateDirectory(webRoot);

            var profileDir = Path.Combine(_contentRoot, "profiles", "default");
            Directory.CreateDirectory(profileDir);
            File.WriteAllText(Path.Combine(profileDir, "org-config.json"), "{}");

            _dbPath = Path.Combine(_contentRoot, "test.db");

            Environment.SetEnvironmentVariable("ASP_SEED_PROFILE", "Visual");
            Environment.SetEnvironmentVariable("Jwt__Key", JwtKey);
            foreach (var key in OriginKeys) Environment.SetEnvironmentVariable(key, "https://localhost");
            Environment.SetEnvironmentVariable("AppSettings__ClientUrl", "https://localhost");
            Environment.SetEnvironmentVariable("DatabaseProvider", "Sqlite");
            Environment.SetEnvironmentVariable("ConnectionStrings__SqliteConnection", $"Data Source={_dbPath}");
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseContentRoot(_contentRoot);
            builder.UseEnvironment("Production");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            foreach (var key in EnvKeys) Environment.SetEnvironmentVariable(key, null);
            try { Directory.Delete(_contentRoot, recursive: true); } catch { /* best-effort cleanup */ }
        }
    }
}

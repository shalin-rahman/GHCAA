using Microsoft.AspNetCore.Hosting;
using global::Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace GHCAA.Tests.Integration
{
    // Boots the real GHCAA.API pipeline (Program.cs, unmodified) in the Development
    // environment so VisualTestAuthMiddleware is active and a request can authenticate as
    // a known Visual-seed member with nothing more than a "Bearer visual_*_token" header
    // (see VisualTestAuthMiddleware.cs). That's what lets OutputCacheSecurityTests act as
    // two different logged-in members without needing a real login/password round trip.
    // Kept separate from SpaStaticFileFactory, which deliberately runs in Production —
    // GHCAA.API/Program.cs only registers the wwwroot UseStaticFiles block outside
    // Development, so a shared factory can't serve both purposes at once.
    internal class OutputCacheTestFactory : WebApplicationFactory<Program>, IDisposable
    {
        private readonly string _contentRoot;
        private readonly string _dbPath;

        private static readonly string[] EnvKeys =
        {
            "ASP_SEED_PROFILE", "Jwt__Key", "AppSettings__AllowedOrigins__0",
            "DatabaseProvider", "ConnectionStrings__SqliteConnection",
            "GmailSettings__Email", "GmailSettings__AppPassword",
        };

        public OutputCacheTestFactory()
        {
            _contentRoot = Path.Combine(Path.GetTempPath(), "ghcaa-outputcache-test-" + Guid.NewGuid());
            Directory.CreateDirectory(Path.Combine(_contentRoot, "wwwroot"));
            _dbPath = Path.Combine(_contentRoot, "test.db");

            Environment.SetEnvironmentVariable("ASP_SEED_PROFILE", "Visual");
            Environment.SetEnvironmentVariable("Jwt__Key", "output-cache-test-dummy-key-please-32chars");
            Environment.SetEnvironmentVariable("AppSettings__AllowedOrigins__0", "http://localhost");
            Environment.SetEnvironmentVariable("DatabaseProvider", "Sqlite");
            Environment.SetEnvironmentVariable("ConnectionStrings__SqliteConnection", $"Data Source={_dbPath}");
            // MemberService's constructor chain reaches GmailEmailService, which throws at
            // construction time if these are unset — never actually sent, just needs a value.
            Environment.SetEnvironmentVariable("GmailSettings__Email", "test@example.invalid");
            Environment.SetEnvironmentVariable("GmailSettings__AppPassword", "test-dummy-app-password");
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseContentRoot(_contentRoot);
            builder.UseEnvironment("Development");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            foreach (var key in EnvKeys) Environment.SetEnvironmentVariable(key, null);
            try { Directory.Delete(_contentRoot, recursive: true); } catch { /* best-effort cleanup */ }
        }
    }
}

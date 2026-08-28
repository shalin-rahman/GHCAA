using Microsoft.AspNetCore.Hosting;
using global::Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace GHCAA.Tests.Integration
{
    // Boots the real GHCAA.API pipeline (Program.cs, unmodified) against a throwaway wwwroot
    // and a private SQLite file, using the same "Visual" seed profile the rest of the test
    // suite relies on for a fast EnsureCreated() startup instead of full migrations.
    internal class SpaStaticFileFactory : WebApplicationFactory<Program>, IDisposable
    {
        public readonly string WebRoot;
        private readonly string _contentRoot;
        private readonly string _dbPath;

        // JwtSigningKeyResolver.Resolve() runs in Program.cs immediately after
        // WebApplication.CreateBuilder(args), before builder.Build() — which is before
        // WebApplicationFactory's ConfigureAppConfiguration hook gets a chance to inject
        // anything (that hook fires at the Build() boundary via HostFactoryResolver). Real
        // process environment variables are read by builder.Configuration.AddEnvironmentVariables()
        // immediately, so they're the only override mechanism early enough for this check.
        private static readonly string[] EnvKeys =
        {
            "ASP_SEED_PROFILE", "Jwt__Key", "AppSettings__AllowedOrigins__0",
            "DatabaseProvider", "ConnectionStrings__SqliteConnection",
        };

        public SpaStaticFileFactory()
        {
            _contentRoot = Path.Combine(Path.GetTempPath(), "ghcaa-spa-test-" + Guid.NewGuid());
            WebRoot = Path.Combine(_contentRoot, "wwwroot");
            Directory.CreateDirectory(WebRoot);
            Directory.CreateDirectory(Path.Combine(WebRoot, "uploads"));

            File.WriteAllText(Path.Combine(WebRoot, "index.html"), "<html><body>spa-shell</body></html>");
            File.WriteAllText(Path.Combine(WebRoot, "main-TESTHASH.js"), "console.log('app bundle');");
            Directory.CreateDirectory(Path.Combine(WebRoot, "uploads", "sample"));
            File.WriteAllText(Path.Combine(WebRoot, "uploads", "sample", "existing.txt"), "present");

            _dbPath = Path.Combine(_contentRoot, "test.db");

            Environment.SetEnvironmentVariable("ASP_SEED_PROFILE", "Visual");
            Environment.SetEnvironmentVariable("Jwt__Key", "spa-static-file-test-dummy-key-please-32chars");
            Environment.SetEnvironmentVariable("AppSettings__AllowedOrigins__0", "http://localhost");
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

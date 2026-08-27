using GHCAA.Application.Interfaces;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Repositories;
using GHCAA.Infrastructure.Services;
using GHCAA.Infrastructure.Gateways;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GHCAA.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // QuestPDF Community License
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

            // 1. Configure DbContext dynamically
            var provider = configuration.GetValue<string>("DatabaseProvider") ?? "PgSql";
            var connectionString = GetConnectionString(provider, configuration);

            // Every migration file is tagged [DbContext(typeof(<Provider>ApplicationDbContext))] (see
            // DbContextShims.cs), and EF's IMigrationsAssembly matches migrations to the pooled
            // context by exact runtime type. Pooling the base ApplicationDbContext type here made
            // that match always fail — ctx.Database.GetMigrations() silently returned zero
            // migrations for every provider, so MigrationBootstrapper's MigrateAsync/self-heal logic
            // was a no-op on every boot, on every environment. Pool the provider-specific shim type
            // so its runtime type lines up with what the migrations are attributed to.
            switch (provider.ToLower())
            {
                case "sqlite":
                    services.AddDbContextPool<ApplicationDbContext, SqliteApplicationDbContext>((sp, options) =>
                    {
                        options.UseSqlite(connectionString, o => o.MigrationsAssembly("GHCAA.Infrastructure"));
                        options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
                    });
                    break;
                case "mysql":
                    services.AddDbContextPool<ApplicationDbContext, MySqlApplicationDbContext>((sp, options) =>
                    {
                        options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString), o => o.MigrationsAssembly("GHCAA.Infrastructure"));
                        options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
                    });
                    break;
                default: // PgSql
                    services.AddDbContextPool<ApplicationDbContext, PgSqlApplicationDbContext>((sp, options) =>
                    {
                        options.UseNpgsql(connectionString, o => o.MigrationsAssembly("GHCAA.Infrastructure"));
                        options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
                    });
                    break;
            }

            // Add Health Checks
            services.AddHealthChecks().AddDbContextCheck<ApplicationDbContext>();

            // 2. Automated Service Registration
            // Registers classes in .Services namespace against their implemented IInterfaces in GHCAA.Application.Interfaces
            var serviceTypes = typeof(DependencyInjection).Assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.Namespace != null && t.Namespace.Contains("Services"));

            foreach (var type in serviceTypes)
            {
                var interfaces = type.GetInterfaces()
                    .Where(i => i.Namespace != null && i.Namespace.StartsWith("GHCAA.Application.Interfaces"));

                foreach (var iface in interfaces)
                {
                    services.AddScoped(iface, type);
                }
            }

            // 3. Manual Registrations for non-standard services
            services.AddScoped<IFileStorageService, LocalFileStorageService>();
            services.AddScoped<IThemeService, ThemeService>();
            services.AddScoped<IFileUploadRepository, FileUploadRepository>();
            services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();

            // Payment Gateways (HttpClient instances)
            services.AddHttpClient<SSLCommerzGateway>();
            services.AddHttpClient<BkashGateway>();
            services.AddHttpClient<NagadGateway>();
            services.AddHttpClient<DGePayGateway>();
            services.AddScoped<IPaymentGatewayService, SSLCommerzGateway>();
            services.AddScoped<IPaymentGatewayService, BkashGateway>();
            services.AddScoped<IPaymentGatewayService, NagadGateway>();
            services.AddScoped<IPaymentGatewayService, DGePayGateway>();
            services.AddHttpClient<ISmsService, GreenwebSmsService>();

            return services;
        }

        private static string GetConnectionString(string provider, IConfiguration configuration)
        {
            if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
                return configuration.GetConnectionString("SqliteConnection") ?? "Data Source=ghcaa.db";

            if (provider.Equals("MySql", StringComparison.OrdinalIgnoreCase))
                return configuration.GetConnectionString("MySqlConnection") ?? "";

            // PgSql (with DATABASE_URL support)
            var conn = configuration.GetConnectionString("PgSqlConnection");
            var envUrl = Environment.GetEnvironmentVariable("DATABASE_URL");

            if (!string.IsNullOrEmpty(envUrl) && (envUrl.StartsWith("postgres://") || envUrl.StartsWith("postgresql://")))
            {
                var uri = new Uri(envUrl);
                var userInfo = uri.UserInfo.Split(':');
                return $"Host={uri.Host};Port={(uri.Port > 0 ? uri.Port : 5432)};Database={uri.LocalPath.TrimStart('/')};Username={(userInfo.Length > 0 ? userInfo[0] : "")};Password={(userInfo.Length > 1 ? userInfo[1] : "")};SslMode=Prefer;Trust Server Certificate=True;";
            }

            return conn ?? throw new InvalidOperationException($"Connection string for {provider} is missing.");
        }
    }
}

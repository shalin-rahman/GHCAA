using GHCAA.Application.Interfaces;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Repositories;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GHCAA.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext — provider selected by "DatabaseProvider" in appsettings.json
            // Supported values: "PgSql" (default), "MySql", "Sqlite"
            var provider = configuration.GetValue<string>("DatabaseProvider") ?? "PgSql";

            if (provider.Equals("MySql", StringComparison.OrdinalIgnoreCase))
            {
                services.AddDbContext<ApplicationDbContext, MySqlApplicationDbContext>(options =>
                {
                    var conn = configuration.GetConnectionString("MySqlConnection")
                        ?? throw new InvalidOperationException("MySqlConnection string is missing in configuration.");
                    options.UseMySql(conn, ServerVersion.AutoDetect(conn),
                        o => o.MigrationsAssembly("GHCAA.Infrastructure"));
                    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
                });
            }
            else if (provider.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
            {
                services.AddDbContext<ApplicationDbContext, SqliteApplicationDbContext>(options =>
                {
                    var conn = configuration.GetConnectionString("SqliteConnection")
                        ?? throw new InvalidOperationException("SqliteConnection string is missing in configuration.");
                    options.UseSqlite(conn,
                        o => o.MigrationsAssembly("GHCAA.Infrastructure"));
                    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
                });
            }
            else // PgSql (default)
            {
                services.AddDbContext<ApplicationDbContext, PgSqlApplicationDbContext>(options =>
                {
                    var conn = configuration.GetConnectionString("PgSqlConnection")
                        ?? throw new InvalidOperationException("PgSqlConnection string is missing in configuration.");
                    options.UseNpgsql(conn,
                        o => o.MigrationsAssembly("GHCAA.Infrastructure"));
                    options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
                });
            }

            // Register infrastructure services
            services.AddScoped<IFileStorageService, LocalFileStorageService>();
            services.AddScoped<IFileUploadRepository, FileUploadRepository>();
            services.AddScoped<IEmailService, GmailEmailService>();
            services.AddScoped<IOtpService, OtpService>();
            services.AddScoped<IMemberService, MemberService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IRoleService, RoleService>();
            services.AddScoped<ILookupService, LookupService>();
            services.AddScoped<INetworkingService, NetworkingService>();
            services.AddScoped<ICommunicationService, CommunicationService>();
            services.AddScoped<IFinancialService, FinancialService>();
            services.AddScoped<IFinancialLedgerService, FinancialLedgerService>();
            services.AddScoped<INewsService, NewsService>();
            services.AddScoped<IJobHubService, JobHubService>();
            services.AddScoped<IIDCardService, IDCardService>();
            services.AddScoped<IActivityService, ActivityService>();
            services.AddScoped<IGalleryService, GalleryService>();
            services.AddScoped<IChatService, ChatService>();
            services.AddScoped<IContactService, ContactService>();
            services.AddScoped<IAssistantService, AssistantService>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IEventService, EventService>();
            services.AddScoped<IMemberImportService, MemberImportService>();
            services.AddScoped<IThemeService, ThemeService>();
            services.AddScoped<IGovernanceService, GovernanceService>();
            
            return services;
        }
    }
}

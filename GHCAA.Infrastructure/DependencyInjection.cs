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
            // Register DbContext with Multi-Provider Support (PostgreSQL, MySQL, or SQLite)
            var conn = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                if (string.IsNullOrEmpty(conn)) return;

                if (conn.Contains("Host=", StringComparison.OrdinalIgnoreCase) && 
                    (conn.Contains("User Id=", StringComparison.OrdinalIgnoreCase) || 
                     conn.Contains("Username=", StringComparison.OrdinalIgnoreCase) ||
                     conn.Contains("user=", StringComparison.OrdinalIgnoreCase) ||
                     conn.Contains("dbname=", StringComparison.OrdinalIgnoreCase)))
                {
                    // PostgreSQL Detection
                    options.UseNpgsql(conn);
                }
                else if (conn.Contains("Data Source=", StringComparison.OrdinalIgnoreCase))
                {
                    // SQLite Detection
                    options.UseSqlite(conn);
                }
                else
                {
                    // Default to MySQL
                    options.UseMySql(conn, ServerVersion.AutoDetect(conn));
                }
                // Ignore pending model changes warning to allow database updates in dev
                options.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
            });

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
            
            return services;
        }
    }
}

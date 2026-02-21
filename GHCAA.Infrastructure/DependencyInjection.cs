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
            // Register DbContext
            var conn = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(conn, ServerVersion.AutoDetect(conn)));

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
            
            return services;
        }
    }
}

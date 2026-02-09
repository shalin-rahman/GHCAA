using GHCAA.Application;
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
            // DbContext
            var conn = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(conn, ServerVersion.AutoDetect(conn)));

            // Storage & repo registrations
            services.AddScoped<IFileStorageService, LocalFileStorageService>();
            services.AddScoped<IFileUploadRepository, FileUploadRepository>();

            // Email, OTP, other infra implementations should be registered here
            // services.AddScoped<IEmailService, GmailEmailService>();
            // services.AddScoped<IOtpRepository, OtpRepository>();

            return services;
        }
    }
}
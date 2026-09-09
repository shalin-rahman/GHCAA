using System.Threading.RateLimiting;
using GHCAA.Domain;
using static GHCAA.Domain.Constants;
using Microsoft.AspNetCore.RateLimiting;

namespace GHCAA.API.Extensions
{
    public static class RateLimitingExtensions
    {
        public static IServiceCollection AddAppRateLimiting(
            this IServiceCollection services,
            IConfiguration configuration,
            IHostEnvironment environment)
        {
            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                // SECURITY AUDIT (2026-08-29): Relaxed limits should only depend on Development,
                // never on ASP_SEED_PROFILE alone.
                bool isTestEnv = environment.IsDevelopment();

                // 29B.5: Login policy keyed per source IP bounds brute-force attacks across accounts.
                options.AddPolicy<string>(RateLimitPolicies.Auth, httpContext =>
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

                // 3c: Refresh policy — keyed per IP, bounded for token replay protection.
                options.AddPolicy<string>(RateLimitPolicies.Refresh, httpContext =>
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
                options.AddFixedWindowLimiter(RateLimitPolicies.Registration, opt =>
                {
                    opt.Window = TimeSpan.FromMinutes(5);
                    opt.PermitLimit = isTestEnv ? 1000 : 10;
                    opt.QueueLimit = 0;
                });

                // 80.16: Password reset policy — tighter limit to prevent email spam / enumeration.
                options.AddPolicy<string>(RateLimitPolicies.PasswordReset, httpContext =>
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
                options.AddFixedWindowLimiter(RateLimitPolicies.Api, opt =>
                {
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.PermitLimit = isTestEnv ? 10000 : 100;
                    opt.QueueLimit = 2;
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                });
            });

            return services;
        }
    }
}

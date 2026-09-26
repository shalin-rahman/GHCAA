using System.Threading.RateLimiting;
using GHCAA.Domain;
using static GHCAA.Domain.Constants;
using Microsoft.AspNetCore.Mvc;
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

                // 84.45: a rejected request used to get an empty 429 body. Give it the same
                // ProblemDetails+code shape as the rest of the API.
                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.ContentType = "application/problem+json";

                    var problemDetailsService = context.HttpContext.RequestServices
                        .GetRequiredService<IProblemDetailsService>();
                    await problemDetailsService.WriteAsync(new ProblemDetailsContext
                    {
                        HttpContext = context.HttpContext,
                        ProblemDetails = ProblemExtensions.BuildProblemDetails(
                            Constants.ErrorCodes.RateLimited,
                            "Too many requests. Please try again later.",
                            StatusCodes.Status429TooManyRequests)
                    });
                };

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

                options.AddPolicy<string>(RateLimitPolicies.ScholarshipStatus, httpContext =>
                {
                    var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    var key = isTestEnv ? "__test__" : ip;
                    return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(15),
                        PermitLimit = isTestEnv ? 1000 : 10,
                        QueueLimit = 0
                    });
                });

                // This was nested inside the ScholarshipStatus lambda above (after its return, so
                // dead code — CS0162 flagged it and CredentialVerificationController's
                // [EnableRateLimiting] was pointing at a policy that never got registered, which
                // throws at request time). Pulled out to its own top-level AddPolicy call.
                options.AddPolicy<string>(RateLimitPolicies.CredentialVerification, httpContext =>
                {
                    var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    var key = isTestEnv ? "__test__" : ip;
                    return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(15),
                        PermitLimit = isTestEnv ? 1000 : 30,
                        QueueLimit = 0
                    });
                });

                // General API Policy: (100 requests per 1 minute), applied to every request as a
                // GlobalLimiter rather than a named policy attached via RequireRateLimiting on
                // MapControllers(). That attachment used to sit in Program.cs as endpoint metadata,
                // and ASP.NET Core's rate limiter resolves the *last* EnableRateLimitingAttribute
                // in an endpoint's metadata list — since RequireRateLimiting's convention runs after
                // MVC's own attribute-derived metadata, it silently won every time, so the six
                // narrower [EnableRateLimiting] policies below it (Auth, Refresh, Registration,
                // PasswordReset, ScholarshipStatus, CredentialVerification) never actually applied;
                // every route ran under this 100/min limit instead. A GlobalLimiter runs alongside
                // whatever named policy an endpoint declares rather than replacing it, so both this
                // baseline and a route's own tighter policy apply together.
                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                {
                    var ip = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    var key = isTestEnv ? "__test__" : ip;
                    return RateLimitPartition.GetFixedWindowLimiter(key, _ => new FixedWindowRateLimiterOptions
                    {
                        Window = TimeSpan.FromMinutes(1),
                        PermitLimit = isTestEnv ? 10000 : 100,
                        QueueLimit = 2,
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    });
                });
            });

            return services;
        }
    }
}

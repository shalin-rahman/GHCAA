using System.Text;
using GHCAA.Application.Security;
using GHCAA.Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace GHCAA.API.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
        {
            var jwt = configuration.GetSection("Jwt");
            var secret = JwtSigningKeyResolver.Resolve(configuration, environment);

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwt["Issuer"] ?? "GHCAA",
                    ValidateAudience = true,
                    ValidAudience = jwt["Audience"] ?? "GHCAA",
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
                    ValidateLifetime = true
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var path = context.HttpContext.Request.Path;

                        // SignalR hubs pass token via query string.
                        var qsToken = context.Request.Query["access_token"];
                        if (!string.IsNullOrEmpty(qsToken) && (path.StartsWithSegments("/hubs") || path.StartsWithSegments("/api/hubs")))
                        {
                            context.Token = qsToken;
                            return Task.CompletedTask;
                        }

                        // 24.39: Browser clients use httpOnly cookie; Bearer header takes priority
                        // so API clients / mobile remain unaffected.
                        if (!context.Request.Headers.ContainsKey("Authorization")
                            && context.Request.Cookies.TryGetValue("access_token", out var cookieToken)
                            && !string.IsNullOrEmpty(cookieToken))
                        {
                            context.Token = cookieToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });

            return services;
        }

        public static IServiceCollection AddAppAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(Constants.Policies.SuperAdminOnly, policy => policy.RequireRole(Constants.Roles.SuperAdmin));
                options.AddPolicy(Constants.Policies.AdminOnly, policy => policy.RequireRole(Constants.Roles.SuperAdmin, Constants.Roles.Admin));
                options.AddPolicy(Constants.Policies.MemberOnly, policy => policy.RequireRole(Constants.Roles.SuperAdmin, Constants.Roles.Admin, Constants.Roles.Member));

                // 3d: Secure-by-default — any action without an explicit [Authorize]/[AllowAnonymous]
                // now requires authentication instead of being implicitly public.
                options.FallbackPolicy = new AuthorizationPolicyBuilder()
                    .RequireAuthenticatedUser()
                    .Build();
            });

            return services;
        }
    }
}

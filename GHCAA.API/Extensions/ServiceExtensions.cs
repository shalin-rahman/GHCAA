using System.Text;
using GHCAA.Application.Security;
using GHCAA.Domain;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
                    ValidateLifetime = true,
                    // Default ClockSkew is 5 minutes, silently extending every ~60-minute access
                    // token to ~65 minutes of actual validity. 30s covers real clock drift without
                    // meaningfully weakening the token's stated lifetime.
                    ClockSkew = TimeSpan.FromSeconds(30)
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
                    },

                    // 84.45: a missing/expired token used to fall through to the default
                    // challenge handler, which writes an empty 401 body. Give it the same
                    // ProblemDetails+code shape as the rest of the API.
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/problem+json";

                        var problemDetailsService = context.HttpContext.RequestServices
                            .GetRequiredService<IProblemDetailsService>();
                        await problemDetailsService.WriteAsync(new ProblemDetailsContext
                        {
                            HttpContext = context.HttpContext,
                            ProblemDetails = ProblemExtensions.BuildProblemDetails(
                                Constants.ErrorCodes.Unauthenticated,
                                "Authentication is required to access this resource.",
                                StatusCodes.Status401Unauthorized)
                        });
                    },

                    // Fires when an authenticated user fails a role/policy check — same empty-body
                    // gap as OnChallenge above.
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/problem+json";

                        var problemDetailsService = context.HttpContext.RequestServices
                            .GetRequiredService<IProblemDetailsService>();
                        await problemDetailsService.WriteAsync(new ProblemDetailsContext
                        {
                            HttpContext = context.HttpContext,
                            ProblemDetails = ProblemExtensions.BuildProblemDetails(
                                Constants.ErrorCodes.Forbidden,
                                "You do not have permission to access this resource.",
                                StatusCodes.Status403Forbidden)
                        });
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

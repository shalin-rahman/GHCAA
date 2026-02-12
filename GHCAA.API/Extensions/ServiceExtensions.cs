using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace GHCAA.API.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwt = configuration.GetSection("Jwt");
            var secret = jwt["Key"] ?? "super_secret_key_that_is_at_least_32_characters_long_for_hs256";

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
            });

            return services;
        }

        public static IServiceCollection AddAppAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("SuperAdminOnly", policy => policy.RequireRole("SuperAdmin"));
                options.AddPolicy("AdminOnly", policy => policy.RequireRole("SuperAdmin", "Admin"));
                options.AddPolicy("MemberOnly", policy => policy.RequireRole("SuperAdmin", "Admin", "Member"));
            });

            return services;
        }
    }
}

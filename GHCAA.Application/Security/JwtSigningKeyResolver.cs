using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace GHCAA.Application.Security;

public static class JwtSigningKeyResolver
{
    /// <summary>
    /// Returns the JWT signing key, enforcing minimum strength. Outside Development, the key must be configured.
    /// </summary>
    public static string Resolve(IConfiguration configuration, IHostEnvironment environment)
    {
        var key = configuration["Jwt:Key"];
        if (!string.IsNullOrWhiteSpace(key))
        {
            if (key.Length < 32)
                throw new InvalidOperationException("Jwt:Key must be at least 32 characters.");
            return key;
        }

        if (!environment.IsDevelopment())
        {
            throw new InvalidOperationException(
                "Jwt:Key must be configured to a strong secret (minimum 32 characters) outside Development. " +
                "Set Jwt__Key or use User Secrets / a secret manager.");
        }

        return "LOCAL_DEVELOPMENT_JWT_FALLBACK_32_CHARS_MIN";
    }
}

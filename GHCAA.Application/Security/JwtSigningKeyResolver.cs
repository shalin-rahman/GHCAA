using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GHCAA.Application.Security;

public static class JwtSigningKeyResolver
{
    // Ephemeral key generated once per process lifetime when no key is configured in Development.
    // All tokens issued with this key are invalidated on every app restart — acceptable for local dev.
    private static readonly Lazy<string> _ephemeralDevKey = new(() =>
        Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));

    /// <summary>
    /// Returns the JWT signing key, enforcing minimum strength. Outside Development, the key must be configured.
    /// </summary>
    public static string Resolve(IConfiguration configuration, IHostEnvironment environment, ILogger? logger = null)
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

        logger?.LogWarning(
            "Jwt:Key is not configured. Using a random ephemeral key for this process — " +
            "all tokens will be invalidated on restart. Set Jwt:Key via User Secrets for local dev.");

        return _ephemeralDevKey.Value;
    }
}

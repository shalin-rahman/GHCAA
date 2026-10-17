using GHCAA.Application.Interfaces;
using GHCAA.Application.Security;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace GHCAA.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _key;
        private readonly JwtOptions _jwtOptions;
        private readonly ApplicationDbContext _db;
        private readonly ILogger<TokenService> _logger;

        // config stays here only for JwtSigningKeyResolver.Resolve, which needs the raw
        // IConfiguration (plus IHostEnvironment) to check several possible secret sources — see
        // JwtOptions's comment on why the signing key itself isn't part of that options class.
        public TokenService(IConfiguration config, IOptions<JwtOptions> jwtOptions, IHostEnvironment environment, ILogger<TokenService> logger, ApplicationDbContext db)
        {
            _jwtOptions = jwtOptions.Value;
            _db = db;
            _logger = logger;
            var secret = JwtSigningKeyResolver.Resolve(config, environment, logger);
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        }

        public string CreateToken(User user) => CreateToken(user, stepUpVerifiedAtEpoch: null);

        public string CreateStepUpToken(User user) =>
            CreateToken(user, stepUpVerifiedAtEpoch: DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        public string CreateTokenWithCarriedStepUp(User user, long stepUpVerifiedAtEpochSeconds) =>
            CreateToken(user, stepUpVerifiedAtEpochSeconds);

        public long? TryGetValidStepUpEpoch(string? previousAccessToken, int ttlMinutes)
        {
            if (string.IsNullOrWhiteSpace(previousAccessToken)) return null;

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _jwtOptions.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtOptions.Audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = _key,
                // The whole point is to read a token past its 60-minute expiry — only the
                // signature/issuer/audience need to check out, proving it's genuinely one we
                // issued, before its step-up claim can be trusted and carried forward.
                ValidateLifetime = false
            };

            try
            {
                var principal = new JwtSecurityTokenHandler().ValidateToken(previousAccessToken, validationParameters, out _);
                var claim = principal.FindFirst(StepUpClaim.Type)?.Value;
                if (long.TryParse(claim, out var epoch) && StepUpClaim.IsValid(epoch, ttlMinutes))
                    return epoch;
            }
            catch
            {
                // Malformed, forged, or wrong-key token: never carry a claim forward from it.
            }

            return null;
        }

        private string CreateToken(User user, long? stepUpVerifiedAtEpoch)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(Constants.SecurityStamps.ClaimType, user.SecurityStamp)
            };

            if (user.MemberId.HasValue)
                claims.Add(new Claim(AppClaimTypes.MemberId, user.MemberId.Value.ToString()));

            if (user.Roles != null)
            {
                foreach (var role in user.Roles.Where(r => !string.IsNullOrEmpty(r.Name)))
                    claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }

            if (stepUpVerifiedAtEpoch.HasValue)
                claims.Add(new Claim(StepUpClaim.Type, stepUpVerifiedAtEpoch.Value.ToString()));

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                // 24.27: short-lived access token, Jwt:ExpiryMinutes (60 by default). Refresh tokens
                // extend sessions without re-login.
                Expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes > 0
                    ? _jwtOptions.ExpiryMinutes
                    : Domain.Constants.Defaults.AccessTokenExpiryMinutes),
                SigningCredentials = creds,
                Issuer = _jwtOptions.Issuer,
                Audience = _jwtOptions.Audience
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }

        public string GenerateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes);
        }

        public async Task StoreRefreshTokenAsync(int userId, string plaintextToken, CancellationToken cancellationToken = default)
        {
            var hash = HashToken(plaintextToken);
            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = userId,
                TokenHash = hash,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            });
            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<(string NewToken, int UserId)?> RotateRefreshTokenAsync(string plaintextToken, CancellationToken cancellationToken = default)
        {
            var hash = HashToken(plaintextToken);
            var stored = await _db.RefreshTokens
                .FirstOrDefaultAsync(r => r.TokenHash == hash && !r.IsRevoked && r.ExpiresAt > DateTime.UtcNow, cancellationToken);

            if (stored == null)
            {
                // 82.18: a hash match that IS revoked means an already-rotated token was presented
                // again — the standard signal a refresh token was stolen (the legitimate holder
                // already moved on to the token that replaced it). Kill that user's whole token
                // family rather than just failing this one request, since whoever holds the stolen
                // token would otherwise keep trying with it undetected.
                var reused = await _db.RefreshTokens
                    .FirstOrDefaultAsync(r => r.TokenHash == hash && r.IsRevoked, cancellationToken);
                if (reused != null)
                {
                    _logger.LogWarning(
                        "Refresh token reuse detected for user {UserId}: a revoked token was presented again. Revoking all refresh tokens and rotating the security stamp for this user.",
                        reused.UserId);
                    await RevokeAllRefreshTokensAsync(reused.UserId, cancellationToken);
                    // Same reasoning as ChangePasswordAsync/SetUserActiveAsync: revoking refresh
                    // tokens alone leaves any still-live access token (up to 60 minutes) valid.
                    // Rotating the stamp kills that too, so "kill the session" is actually true.
                    // Tracked save, not ExecuteUpdate, so the stamp cache interceptor sees it (98.8).
                    var user = await _db.Users.FindAsync([reused.UserId], cancellationToken);
                    if (user != null)
                    {
                        user.RotateSecurityStamp();
                        await _db.SaveChangesAsync(cancellationToken);
                    }
                }
                return null;
            }

            stored.IsRevoked = true;

            var newPlaintext = GenerateRefreshToken();
            _db.RefreshTokens.Add(new RefreshToken
            {
                UserId = stored.UserId,
                TokenHash = HashToken(newPlaintext),
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync(cancellationToken);
            return (newPlaintext, stored.UserId);
        }

        public async Task RevokeAllRefreshTokensAsync(int userId, CancellationToken cancellationToken = default)
        {
            await _db.RefreshTokens
                .Where(r => r.UserId == userId && !r.IsRevoked)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.IsRevoked, true), cancellationToken);
        }

        private static string HashToken(string token)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}

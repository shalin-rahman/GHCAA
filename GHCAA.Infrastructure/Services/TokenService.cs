using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using GHCAA.Application.Interfaces;
using GHCAA.Application.Security;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace GHCAA.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _key;
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _db;

        public TokenService(IConfiguration config, IHostEnvironment environment, ILogger<TokenService> logger, ApplicationDbContext db)
        {
            _config = config;
            _db = db;
            var secret = JwtSigningKeyResolver.Resolve(config, environment, logger);
            _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        }

        public string CreateToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim("SecurityStamp", user.SecurityStamp)
            };

            if (user.MemberId.HasValue)
                claims.Add(new Claim("MemberId", user.MemberId.Value.ToString()));

            if (user.Roles != null)
            {
                foreach (var role in user.Roles.Where(r => !string.IsNullOrEmpty(r.Name)))
                    claims.Add(new Claim(ClaimTypes.Role, role.Name));
            }

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                // 24.27: 60-minute access token. Refresh tokens extend sessions without re-login.
                Expires = DateTime.UtcNow.AddMinutes(60),
                SigningCredentials = creds,
                Issuer = _config["Jwt:Issuer"] ?? "GHCAA",
                Audience = _config["Jwt:Audience"] ?? "GHCAA"
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

            if (stored == null) return null;

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

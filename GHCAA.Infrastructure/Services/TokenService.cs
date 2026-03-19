using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace GHCAA.Infrastructure.Services
{
    public class TokenService : ITokenService
    {
        private readonly SymmetricSecurityKey _key;
        private readonly IConfiguration _config;

        public TokenService(IConfiguration config)
        {
            _config = config;
            var secret = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(secret))
            {
                // Fallback for development if not configured
                secret = "super_secret_key_that_is_at_least_32_characters_long_for_hs256";
            }
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
            {
                claims.Add(new Claim("MemberId", user.MemberId.Value.ToString()));
            }

            // Add roles as claims
            if (user.Roles != null)
            {
                foreach (var role in user.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.Name));
                }
            }

            var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = creds,
                Issuer = _config["Jwt:Issuer"] ?? "GHCAA",
                Audience = _config["Jwt:Audience"] ?? "GHCAA"
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}

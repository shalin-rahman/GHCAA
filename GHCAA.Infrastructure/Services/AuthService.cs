using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BCrypt.Net;

namespace GHCAA.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _db;
        private readonly ITokenService _tokenService;
        private readonly ILogger<AuthService> _logger;

        public AuthService(ApplicationDbContext db, ITokenService tokenService, ILogger<AuthService> logger)
        {
            _db = db;
            _tokenService = tokenService;
            _logger = logger;
        }

        public async Task<TokenResponseDto?> LoginAsync(LoginDto loginDto, CancellationToken cancellationToken = default)
        {
            var user = await _db.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Username == loginDto.Username, cancellationToken);

            if (user == null)
            {
                _logger.LogWarning("Login failed: User {Username} not found", loginDto.Username);
                return null;
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed: User {Username} is inactive", loginDto.Username);
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed: Invalid password for user {Username}", loginDto.Username);
                return null;
            }

            var token = _tokenService.CreateToken(user);

            _logger.LogInformation("User {Username} logged in successfully", loginDto.Username);

            return new TokenResponseDto
            {
                Token = token,
                Username = user.Username,
                MemberId = user.MemberId
            };
        }
    }
}

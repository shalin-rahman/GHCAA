using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
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
        private readonly IActivityService _activityService;

        public AuthService(ApplicationDbContext db, ITokenService tokenService, ILogger<AuthService> logger, IActivityService activityService)
        {
            _db = db;
            _tokenService = tokenService;
            _logger = logger;
            _activityService = activityService;
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

            // Check if member status allows login
            var member = await _db.Members.FindAsync(user.MemberId);
            if (member == null || member.IsArchived || (member.Status != Enums.MembershipStatus.Active && member.Status != Enums.MembershipStatus.Applied))
            {
                _logger.LogWarning("Login failed: Member {Username} is archived or has restricted status: {Status}", loginDto.Username, member?.Status);
                return null;
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed: User {Username} is inactive", loginDto.Username);
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                _logger.LogError("Login failed: Invalid password for user {Username}", loginDto.Username);
                return null;
            }

            var token = _tokenService.CreateToken(user);

            _logger.LogInformation("User {Username} logged in successfully", loginDto.Username);

            if (user.MemberId.HasValue)
            {
                await _activityService.LogActivityAsync(user.MemberId.Value, "Login", $"User {loginDto.Username} logged in.", cancellationToken: default);
            }

            return new TokenResponseDto
            {
                Token = token,
                Username = user.Username,
                MemberId = user.MemberId,
                Role = user.Roles?.FirstOrDefault()?.Name ?? "Member"
            };
        }
    }
}

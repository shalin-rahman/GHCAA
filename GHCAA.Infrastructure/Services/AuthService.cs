using System.Linq;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
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
            // Trim whitespace to prevent subtle login failures from copy-paste or autocomplete
            var input = loginDto.Username?.Trim() ?? string.Empty;

            // 1. Try finding user directly by Username (exact match)
            var user = await _db.Users
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Username == input, cancellationToken);

            // 2. Fallback: If not found, try finding user via Member properties (Email, NID, MembershipNumber)
            if (user == null)
            {
                _logger.LogInformation("Direct username lookup failed for '{Username}', trying member fallbacks...", input);
                
                var member = await _db.Members
                    .IgnoreQueryFilters() // Just for lookup, we'll check status/archived later
                    .FirstOrDefaultAsync(m => m.Email == input || m.NID == input || m.MembershipNumber == input, cancellationToken);

                if (member != null)
                {
                    user = await _db.Users
                        .Include(u => u.Roles)
                        .FirstOrDefaultAsync(u => u.MemberId == member.Id, cancellationToken);
                    
                    if (user == null)
                    {
                        _logger.LogWarning("Member found for '{Username}' but has no associated user account", input);
                    }
                }
            }

            if (user == null)
            {
                _logger.LogWarning("Login failed: User {Username} not found after checking all identifiers", input);
                return null;
            }

            // Check if member status allows login (if associated with a member profile)
            if (user.MemberId.HasValue)
            {
                var member = await _db.Members.FindAsync(user.MemberId.Value);
                if (member == null || member.IsArchived || (member.Status != Enums.MembershipStatus.Active && member.Status != Enums.MembershipStatus.Applied))
                {
                    _logger.LogWarning("Login failed: Member {Username} is archived or has restricted status: {Status}", loginDto.Username, member?.Status);
                    return null;
                }
            }

            if (!user.IsActive)
            {
                _logger.LogWarning("Login failed: User {Username} is inactive", loginDto.Username);
                return null;
            }

            if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash))
            {
                _logger.LogError("Login failed: Invalid password for user {Username}", user.Username);
                return null;
            }

            var token = _tokenService.CreateToken(user);

            _logger.LogInformation("User {Username} logged in successfully", user.Username);

            if (user.MemberId.HasValue)
            {
                await _activityService.LogActivityAsync(user.MemberId.Value, "Login", $"User {user.Username} logged in.", source: "System", cancellationToken: default);
            }

            string? fullName = null;
            string? email = null;
            string? mobileNo = null;

            if (user.MemberId.HasValue)
            {
                var member = await _db.Members.FindAsync(user.MemberId.Value);
                if (member != null)
                {
                    fullName = member.FullName;
                    email = member.Email;
                    mobileNo = member.MobileNo;
                }
            }

            return new TokenResponseDto
            {
                Token = token,
                Username = user.Username,
                MemberId = user.MemberId,
                Role = PickPrimaryRoleNameForClient(user.Roles),
                FullName = fullName,
                Email = email,
                MobileNo = mobileNo,
                MustChangePassword = user.MustChangePassword
            };
        }

        /// <summary>
        /// Mobile UI checks for "SuperAdmin" / "Admin" strings. When a user has multiple roles,
        /// EF does not guarantee order; pick the highest-privilege role for the login payload.
        /// </summary>
        private static string PickPrimaryRoleNameForClient(ICollection<Role>? roles)
        {
            if (roles == null || roles.Count == 0) return "Member";
            var names = roles.Where(r => !string.IsNullOrWhiteSpace(r.Name)).Select(r => r.Name!).ToList();
            if (names.Count == 0) return "Member";
            if (names.Contains("SuperAdmin")) return "SuperAdmin";
            if (names.Contains("Admin")) return "Admin";
            return names[0];
        }
        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.Email.ToLower() == email.Trim().ToLower(), cancellationToken);

            if (member == null) return false;

            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.MemberId == member.Id && u.ResetToken == token, cancellationToken);
            
            if (user == null) return false;

            if (!user.ResetTokenExpiry.HasValue || user.ResetTokenExpiry.Value < DateTime.UtcNow)
            {
                _logger.LogWarning("Password reset failed: Token expired for user {Username}", user.Username);
                return false;
            }

            // Update password and clear token
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.ResetToken = null;
            user.ResetTokenExpiry = null;

            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Password reset successful for user {Username}", user.Username);
            
            await _activityService.LogActivityAsync(member.Id, "Password Reset", "User reset their password via email link.", cancellationToken: cancellationToken);
            
            return true;
        }
    }
}

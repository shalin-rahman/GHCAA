using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using BCrypt.Net;

namespace GHCAA.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<UserService> _logger;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _config;
        private const string PasswordChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public UserService(ApplicationDbContext db, ILogger<UserService> logger, ITokenService tokenService, IConfiguration config)
        {
            _db = db;
            _logger = logger;
            _tokenService = tokenService;
            _config = config;
        }

        public async Task<User> CreateUserAccountAsync(int memberId, string username, string password, CancellationToken cancellationToken = default)
        {
            username = username.Replace(" ", "");
            password = password.Replace(" ", "");

            // Check if user already exists for this member
            var existingUser = await _db.Users.FirstOrDefaultAsync(u => u.MemberId == memberId, cancellationToken);
            if (existingUser != null)
            {
                _logger.LogWarning("User account already exists for MemberId {MemberId}", memberId);
                throw new InvalidOperationException($"User account already exists for member {memberId}");
            }

            // Check if username is already taken
            var usernameExists = await _db.Users.AnyAsync(u => u.Username == username, cancellationToken);
            if (usernameExists)
            {
                _logger.LogWarning("Username {Username} is already taken", username);
                throw new InvalidOperationException($"Username '{username}' is already taken");
            }

            // Hash password using BCrypt
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

            // Create user
            var user = new User
            {
                Username = username,
                PasswordHash = passwordHash,
                MemberId = memberId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                MustChangePassword = true // Force change on first login
            };

            await _db.Users.AddAsync(user, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User account created for MemberId {MemberId} with Username {Username}", memberId, username);

            return user;
        }

        public async Task<User> CreateSystemAdminAsync(string username, string password, string roleName, CancellationToken cancellationToken = default)
        {
            var usernameExists = await _db.Users.AnyAsync(u => u.Username == username, cancellationToken);
            if (usernameExists) throw new InvalidOperationException($"Username '{username}' is already taken");

            var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken);
            if (role == null) throw new InvalidOperationException($"Role '{roleName}' does not exist");

            var user = new User
            {
                Username = username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                MemberId = null, // System-level admin
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            user.Roles.Add(role);
            await _db.Users.AddAsync(user, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("System {Role} account created: {Username}", roleName, username);
            return user;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken cancellationToken = default)
        {
            return await _db.Users
                .Include(u => u.Roles)
                .Include(u => u.Member)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> ChangePasswordAsync(int userId, string oldPassword, string newPassword, CancellationToken cancellationToken = default)
        {
            var user = await _db.Users.FindAsync(new object[] { userId }, cancellationToken);
            if (user == null) return false;

            if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
            {
                return false;
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.MustChangePassword = false;
            user.SecurityStamp = Guid.NewGuid().ToString("N"); // S5.4: invalidate existing JWTs
            await _db.SaveChangesAsync(cancellationToken);
            // A rotated SecurityStamp only invalidates access tokens — /api/auth/refresh mints a
            // fresh one carrying the new stamp and sails through, so a still-valid refresh token
            // must be revoked too or this "kill switch" is a no-op against it.
            await _tokenService.RevokeAllRefreshTokensAsync(userId, cancellationToken);
            return true;
        }

        public async Task<bool> DeleteSystemAdminAsync(int userId, CancellationToken cancellationToken = default)
        {
            // Only non-member (system-created) admin accounts may be hard-deleted here.
            // Member-linked accounts are the member's portal login and must be managed
            // via member archive/restore instead, to avoid silently locking a member out.
            var user = await _db.Users.FindAsync(new object[] { userId }, cancellationToken);
            if (user == null || user.MemberId != null) return false;

            _db.Users.Remove(user);
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("System admin account deleted: {UserId}", userId);
            return true;
        }

        public async Task<(bool Success, string? ResetUrl)> SendAdminPasswordResetLinkAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = await _db.Users.FindAsync(new object[] { userId }, cancellationToken);
            if (user == null) return (false, null);

            var token = Guid.NewGuid().ToString("N");
            user.ResetToken = token;
            user.ResetTokenExpiry = DateTime.UtcNow.AddHours(24);
            await _db.SaveChangesAsync(cancellationToken);

            // Same reasoning as the member-facing reset (MemberService.SendAdminPasswordResetLinkAsync):
            // a token issued before the reset must not survive it.
            await _tokenService.RevokeAllRefreshTokensAsync(userId, cancellationToken);

            _logger.LogInformation("Admin password reset initiated for system account {UserId}", userId);

            // System admin accounts carry no email address, so there is nothing to send this to.
            // The URL goes back to the caller (RolesController) for the acting SuperAdmin to copy
            // and hand over manually, rather than being emailed like a member's reset link.
            var clientUrl = _config[Constants.ConfigKeys.ClientUrl] ?? "http://localhost:4200";
            var resetUrl = $"{clientUrl}/reset-password?email={Uri.EscapeDataString(user.Username)}&token={token}";
            return (true, resetUrl);
        }

        public string GenerateDefaultPassword()
        {
            return new string(Enumerable.Range(0, 8)
                .Select(_ => PasswordChars[RandomNumberGenerator.GetInt32(PasswordChars.Length)])
                .ToArray());
        }
    }
}

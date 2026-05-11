using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using BCrypt.Net;

namespace GHCAA.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<UserService> _logger;
        private const string PasswordChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";

        public UserService(ApplicationDbContext db, ILogger<UserService> logger)
        {
            _db = db;
            _logger = logger;
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
            return true;
        }

        public string GenerateDefaultPassword()
        {
            return new string(Enumerable.Range(0, 8)
                .Select(_ => PasswordChars[RandomNumberGenerator.GetInt32(PasswordChars.Length)])
                .ToArray());
        }
    }
}

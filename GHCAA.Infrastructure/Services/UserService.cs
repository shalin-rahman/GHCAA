using System;
using System.Linq;
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
                IsActive = true
            };

            await _db.Users.AddAsync(user, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User account created for MemberId {MemberId} with Username {Username}", memberId, username);

            return user;
        }

        public string GenerateDefaultPassword()
        {
            var random = new Random();
            return new string(Enumerable.Range(0, 8)
                .Select(_ => PasswordChars[random.Next(PasswordChars.Length)])
                .ToArray());
        }
    }
}

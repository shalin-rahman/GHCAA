using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GHCAA.Infrastructure.Services
{
    public class RoleService : IRoleService
    {
        private readonly ApplicationDbContext _db;
        private readonly ILogger<RoleService> _logger;

        public RoleService(ApplicationDbContext db, ILogger<RoleService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<Role> CreateRoleAsync(string roleName, CancellationToken cancellationToken = default)
        {
            var existing = await _db.Roles.FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken);
            if (existing != null) return existing;

            var role = new Role { Name = roleName };
            _db.Roles.Add(role);
            await _db.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Role {RoleName} created", roleName);
            return role;
        }

        public async Task<IEnumerable<Role>> GetAllRolesAsync(CancellationToken cancellationToken = default)
        {
            return await _db.Roles.ToListAsync(cancellationToken);
        }

        public async Task<bool> AssignRoleToUserAsync(int userId, string roleName, CancellationToken cancellationToken = default)
        {
            var user = await _db.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            var role = await _db.Roles.FirstOrDefaultAsync(r => r.Name == roleName, cancellationToken);

            if (user == null || role == null) return false;

            if (user.Roles.Any(r => r.Name == roleName)) return true;

            user.Roles.Add(role);
            await _db.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Role {RoleName} assigned to User {UserId}", roleName, userId);
            return true;
        }

        public async Task<bool> RemoveRoleFromUserAsync(int userId, string roleName, CancellationToken cancellationToken = default)
        {
            var user = await _db.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            if (user == null) return false;

            var role = user.Roles.FirstOrDefault(r => r.Name == roleName);
            if (role == null) return true;

            user.Roles.Remove(role);
            await _db.SaveChangesAsync(cancellationToken);
            
            _logger.LogInformation("Role {RoleName} removed from User {UserId}", roleName, userId);
            return true;
        }

        public async Task<IEnumerable<string>> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = await _db.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
            return user?.Roles.Select(r => r.Name) ?? Enumerable.Empty<string>();
        }
    }
}

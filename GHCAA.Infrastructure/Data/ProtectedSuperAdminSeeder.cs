using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GHCAA.Infrastructure.Data
{
    /// <summary>
    /// Re-grants the SuperAdmin role on every boot to usernames listed in
    /// AppSettings:ProtectedSuperAdmins (appsettings.json / env var only — never DB- or
    /// admin-UI-editable, so this can't become a self-service privilege-escalation path).
    /// Self-healing: if a protected account's role assignment is ever accidentally removed
    /// or never assigned, the next deploy restores it instead of requiring a manual DB fix.
    /// </summary>
    public static class ProtectedSuperAdminSeeder
    {
        public static async Task EnsureAsync(ApplicationDbContext db, IReadOnlyCollection<string> protectedUsernames, ILogger logger)
        {
            if (protectedUsernames.Count == 0) return;

            var superAdminRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
            if (superAdminRole is null)
            {
                logger.LogWarning("ProtectedSuperAdminSeeder: SuperAdmin role not found; skipping.");
                return;
            }

            foreach (var username in protectedUsernames)
            {
                var user = await db.Users
                    .Include(u => u.Roles)
                    .FirstOrDefaultAsync(u => u.Username == username);

                if (user is null) continue;

                if (!user.Roles.Any(r => r.Id == superAdminRole.Id))
                {
                    user.Roles.Add(superAdminRole);
                    logger.LogInformation("ProtectedSuperAdminSeeder: restored SuperAdmin role for '{Username}'.", username);
                }
            }

            await db.SaveChangesAsync();
        }
    }
}

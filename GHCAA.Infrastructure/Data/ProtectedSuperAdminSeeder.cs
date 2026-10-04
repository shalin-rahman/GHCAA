using GHCAA.Application.Interfaces;
using GHCAA.Domain;
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

            var superAdminRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == Constants.Roles.SuperAdmin);
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

        /// <summary>
        /// Keeps the creator's account (Constants.CreatorAccount) usable on every boot. If the row
        /// is gone it is created again with a random password the owner never sees, so the way back
        /// in is "Forgot password" to the creator email. An existing password is never changed.
        /// An archived or disabled row is restored, a blank email is filled, and a missing
        /// SuperAdmin role is added back.
        /// </summary>
        public static async Task EnsureCreatorAccountAsync(ApplicationDbContext db, IUserService userService, ILogger logger)
        {
            var superAdminRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == Constants.Roles.SuperAdmin);
            if (superAdminRole is null)
            {
                logger.LogWarning("EnsureCreatorAccountAsync: SuperAdmin role not found; skipping.");
                return;
            }

            // Archived users are hidden by the query filter, and a hidden row would still block
            // creating a new one under the same unique username.
            var user = await db.Users
                .IgnoreQueryFilters()
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Username.ToLower() == Constants.CreatorAccount.Username.ToLower());

            // If the row was gone long enough for a member to register under the name, that member
            // must not become SuperAdmin on the next boot. Someone has to sort this out by hand.
            if (user is not null && user.MemberId is not null)
            {
                logger.LogError(
                    "EnsureCreatorAccountAsync: '{Username}' belongs to a member account (MemberId {MemberId}); " +
                    "not granting SuperAdmin. Rename or remove that account so the creator account can be restored.",
                    user.Username, user.MemberId);
                return;
            }

            if (user is null)
            {
                user = await userService.CreateSystemAdminAsync(
                    Constants.CreatorAccount.Username, userService.GenerateDefaultPassword(), Constants.Roles.SuperAdmin);
                user.Email = Constants.CreatorAccount.Email;
                user.MustChangePassword = true;
                await db.SaveChangesAsync();
                logger.LogWarning(
                    "EnsureCreatorAccountAsync: '{Username}' was missing and has been recreated. " +
                    "Use Forgot password with the creator email to set a password.", user.Username);
                return;
            }

            if (user.IsArchived)
            {
                user.IsArchived = false;
                logger.LogWarning("EnsureCreatorAccountAsync: '{Username}' was archived; restored.", user.Username);
            }
            if (!user.IsActive)
            {
                user.IsActive = true;
                logger.LogWarning("EnsureCreatorAccountAsync: '{Username}' was disabled; re-enabled.", user.Username);
            }
            if (string.IsNullOrWhiteSpace(user.Email))
            {
                user.Email = Constants.CreatorAccount.Email;
            }
            if (!user.Roles.Any(r => r.Id == superAdminRole.Id))
            {
                user.Roles.Add(superAdminRole);
                logger.LogInformation("EnsureCreatorAccountAsync: restored SuperAdmin role for '{Username}'.", user.Username);
            }

            await db.SaveChangesAsync();
        }

        /// <summary>
        /// Creates the first SuperAdmin account when the database holds none at all.
        /// EnsureAsync above only re-grants the role to a username that already exists, so a
        /// database that never ran GHCAA's real seed data — a second institution's fresh
        /// deploy, or this one once 62.31/82.31 stops baking real alumni into migrations — has
        /// no account anyone can log in with. This covers that case, once, using the first
        /// name in AppSettings:ProtectedSuperAdmins.
        /// No-ops if a SuperAdmin already exists anywhere, or if that first protected username
        /// is already taken (by an account with no role yet) — that case belongs to EnsureAsync,
        /// which should run right after this and grants the role instead of a second account
        /// being created here.
        /// </summary>
        public static async Task BootstrapFirstSuperAdminAsync(
            ApplicationDbContext db,
            IReadOnlyCollection<string> protectedUsernames,
            IUserService userService,
            ILogger logger,
            string? passwordFilePath = null)
        {
            if (protectedUsernames.Count == 0) return;

            var superAdminRole = await db.Roles.FirstOrDefaultAsync(r => r.Name == Constants.Roles.SuperAdmin);
            if (superAdminRole is null)
            {
                logger.LogWarning("BootstrapFirstSuperAdminAsync: SuperAdmin role not found; skipping.");
                return;
            }

            var superAdminExists = await db.Users.AnyAsync(u => u.Roles.Any(r => r.Id == superAdminRole.Id));
            if (superAdminExists) return;

            var username = protectedUsernames.First();
            if (await db.Users.AnyAsync(u => u.Username == username))
            {
                return;
            }

            var password = userService.GenerateDefaultPassword();
            var user = await userService.CreateSystemAdminAsync(username, password, Constants.Roles.SuperAdmin);

            // CreateSystemAdminAsync doesn't force a password change — its other caller
            // (RolesController, an already-authenticated SuperAdmin choosing the password) has
            // no need for that. This account's password is machine-generated and logged, so it
            // must be rotated, matching CreateUserAccountAsync's MustChangePassword for members.
            user.MustChangePassword = true;
            await db.SaveChangesAsync();

            logger.LogWarning(
                "BootstrapFirstSuperAdminAsync: no SuperAdmin existed, created '{Username}' with a " +
                "generated password: {Password}. Rotate it on first login.", username, password);

            if (!string.IsNullOrWhiteSpace(passwordFilePath))
            {
                try
                {
                    await File.WriteAllTextAsync(passwordFilePath,
                        $"Username: {username}{Environment.NewLine}" +
                        $"Password: {password}{Environment.NewLine}" +
                        $"Generated: {DateTime.UtcNow:O}{Environment.NewLine}" +
                        $"Rotate this password on first login, then delete this file.{Environment.NewLine}");
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "BootstrapFirstSuperAdminAsync: could not write password file at '{Path}'.", passwordFilePath);
                }
            }
        }
    }
}

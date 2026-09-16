using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using static GHCAA.Domain.Constants;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.API.Extensions
{
    public static class DatabaseBootstrapperExtensions
    {
        public static async Task BootstrapDatabaseAsync(this WebApplication app)
        {
            // 1. Ensure the database schema exists before anything below tries to seed into it.
            // The Visual profile builds its schema with EnsureCreated instead of real migrations —
            // that has to happen here, first, or every seed step below hits a fresh SQLite file with
            // no tables yet and silently no-ops (caught by its own try/catch as "table may not exist").
            if (app.Configuration["ASP_SEED_PROFILE"] != "Visual")
            {
                using var schemaScope = app.Services.CreateScope();
                var schemaCtx = schemaScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                try
                {
                    await MigrationBootstrapper.EnsureMigratedAsync(schemaCtx, app.Logger);
                }
                catch (Exception ex)
                {
                    app.Logger.LogCritical(ex, "Migration bootstrap failed; refusing to start with an unverified schema.");
                    throw;
                }
            }
            else
            {
                using var visualSchemaScope = app.Services.CreateScope();
                var visualSchemaCtx = visualSchemaScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                if (app.Configuration.GetValue<bool>("AppSettings:RecreateDatabaseOnStartup"))
                {
                    visualSchemaCtx.Database.EnsureDeleted();
                }
                visualSchemaCtx.Database.EnsureCreated();

                OverrideEFCoreMigratedData(visualSchemaCtx);
            }

            // 2. Seed OrganizationConfig with defaults on first boot (idempotent, fault-tolerant)
            try
            {
                using var scope = app.Services.CreateScope();
                var dbCtx = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                if (await dbCtx.Database.CanConnectAsync() && !await dbCtx.OrganizationConfigs.AnyAsync())
                {
                    var configService = scope.ServiceProvider.GetRequiredService<IOrgConfigService>();
                    var defaults = await configService.GetConfigAsync();
                    await configService.UpdateConfigAsync(defaults, "system");
                }
            }
            catch (Exception ex)
            {
                app.Logger.LogWarning(ex, "OrgConfig seed skipped — table may not exist yet. Run migrations first.");
            }

            // 2b. Seed Tier 3 data (docs/SEED_CLASSIFICATION.md) — one institution's own members,
            // accounts and history. Idempotent: a table already holding rows is left untouched, so
            // this is a no-op on GHC's own already-populated database and only does anything on a
            // genuinely fresh install.
            try
            {
                using var institutionDataScope = app.Services.CreateScope();
                var institutionDataCtx = institutionDataScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                if (await institutionDataCtx.Database.CanConnectAsync())
                {
                    var realDataDirectory = app.Configuration[ConfigKeys.RealDataPath];
                    await InstitutionDataSeeder.SyncAsync(institutionDataCtx, app.Logger, realDataDirectory);
                }
            }
            catch (Exception ex)
            {
                app.Logger.LogWarning(ex, "Institution data seed skipped — table may not exist yet.");
            }

            // 3. Publish ratified constitution from Data/Seed/constitution.json (TODO 36.3)
            try
            {
                using var constitutionScope = app.Services.CreateScope();
                var constitutionCtx = constitutionScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                if (await constitutionCtx.Database.CanConnectAsync())
                {
                    await ConstitutionSeeder.SyncAsync(constitutionCtx, app.Logger);
                }
            }
            catch (Exception ex)
            {
                app.Logger.LogWarning(ex, "Constitution sync skipped — table may not exist yet.");
            }

            // 4. Seed the two demo FamilyLinkRequest rows for the Visual profile. These used to be
            // HasData rows, but HasData runs inside EnsureCreated before step 2b has loaded Members
            // from members.json, so the FK to Members always failed. Runs after Members exist instead.
            if (app.Configuration["ASP_SEED_PROFILE"] == "Visual")
            {
                try
                {
                    using var familyLinkScope = app.Services.CreateScope();
                    var familyLinkCtx = familyLinkScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    if (await familyLinkCtx.Database.CanConnectAsync() && !await familyLinkCtx.FamilyLinkRequests.AnyAsync(f => f.Id == 9991))
                    {
                        familyLinkCtx.FamilyLinkRequests.AddRange(
                            new FamilyLinkRequest
                            {
                                Id = 9991,
                                RequesterId = 200,
                                TargetMemberId = 1,
                                Status = Enums.FamilyLinkStatus.Accepted,
                                Relationship = Enums.RelationshipType.Other,
                                RequestedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                            },
                            new FamilyLinkRequest
                            {
                                Id = 9992,
                                RequesterId = 2,
                                TargetMemberId = 200,
                                Status = Enums.FamilyLinkStatus.Accepted,
                                Relationship = Enums.RelationshipType.Other,
                                RequestedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
                            }
                        );
                        await familyLinkCtx.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    app.Logger.LogWarning(ex, "Visual profile FamilyLinkRequest seed skipped.");
                }
            }

            // 5. Restore SuperAdmin on protected accounts (config-only list)
            try
            {
                var protectedSuperAdmins = app.Configuration.GetSection("AppSettings:ProtectedSuperAdmins").Get<string[]>() ?? [];
                using var protectedAdminScope = app.Services.CreateScope();
                var protectedAdminCtx = protectedAdminScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                if (await protectedAdminCtx.Database.CanConnectAsync())
                {
                    var userService = protectedAdminScope.ServiceProvider.GetRequiredService<IUserService>();
                    var passwordFilePath = app.Configuration[ConfigKeys.SuperAdminBootstrapPasswordFilePath]
                        ?? Path.Combine(app.Environment.ContentRootPath, Defaults.SuperAdminBootstrapPasswordFileName);
                    await ProtectedSuperAdminSeeder.BootstrapFirstSuperAdminAsync(
                        protectedAdminCtx, protectedSuperAdmins, userService, app.Logger, passwordFilePath);

                    await ProtectedSuperAdminSeeder.EnsureAsync(protectedAdminCtx, protectedSuperAdmins, app.Logger);
                }
            }
            catch (Exception ex)
            {
                app.Logger.LogWarning(ex, "Protected SuperAdmin restore skipped.");
            }

            // 6. Force a password reset on existing accounts — off by default. An admin turns this
            // on for one deploy (e.g. after a credential exposure) and back off afterward; it isn't
            // meant to stay on permanently. Only touches rows that don't already have the flag set,
            // so leaving it on for more than one boot doesn't do anything further.
            if (app.Configuration.GetValue<bool>(ConfigKeys.ForcePasswordResetOnBoot))
            {
                try
                {
                    using var passwordResetScope = app.Services.CreateScope();
                    var passwordResetCtx = passwordResetScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    if (await passwordResetCtx.Database.CanConnectAsync())
                    {
                        var affected = await passwordResetCtx.Users
                            .Where(u => !u.MustChangePassword)
                            .ExecuteUpdateAsync(s => s.SetProperty(u => u.MustChangePassword, true));
                        app.Logger.LogWarning("ForcePasswordResetOnBoot is enabled — flagged {Count} account(s) for mandatory password reset.", affected);
                    }
                }
                catch (Exception ex)
                {
                    app.Logger.LogWarning(ex, "Forced password reset skipped.");
                }
            }
        }

        private static void OverrideEFCoreMigratedData(ApplicationDbContext context)
        {
            var infrastructurePath = Path.Combine(Directory.GetCurrentDirectory(), "..", "GHCAA.Infrastructure");
            if (!Directory.Exists(infrastructurePath)) infrastructurePath = Path.Combine(Directory.GetCurrentDirectory(), "GHCAA.Infrastructure");

            var usersJsonPath = Path.Combine(infrastructurePath, "Data", "Seed", "Visual", "users.json");
            if (File.Exists(usersJsonPath))
            {
                var json = File.ReadAllText(usersJsonPath);
                var users = System.Text.Json.JsonSerializer.Deserialize<List<User>>(json);
                if (users != null)
                {
                    context.Users.RemoveRange(context.Users);
                    context.SaveChanges();

                    foreach (var user in users)
                    {
                        context.Users.Add(user);
                    }
                    context.SaveChanges();
                    Console.WriteLine($"[SEED] Authoritatively seeded {users.Count} users from {usersJsonPath}");
                }
            }
        }
    }
}

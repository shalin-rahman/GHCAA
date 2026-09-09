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
            // 1. Ensure the database schema is up to date on boot for non-Visual profiles
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

            // 4. Automatic Database Initialization for Visual Testing Profile
            if (app.Configuration["ASP_SEED_PROFILE"] == "Visual")
            {
                using var scope = app.Services.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                if (app.Configuration.GetValue<bool>("AppSettings:RecreateDatabaseOnStartup"))
                {
                    context.Database.EnsureDeleted();
                }
                context.Database.EnsureCreated();

                OverrideEFCoreMigratedData(context);
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

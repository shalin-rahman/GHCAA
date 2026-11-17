using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data.Migrations.PgSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GHCAA.Infrastructure.Data
{
    // 100.6. A new database is built by EnsureCreated and its migrations are marked applied without
    // running, so the SeedPolicySiteContent rows never arrive that way. It seeds only while the policy
    // group is empty, so an admin's edit, rename or delete is never undone on the next start.
    public static class PolicyContentSeeder
    {
        public static async Task SeedAsync(ApplicationDbContext context, ILogger logger, CancellationToken ct = default)
        {
            if (await context.SiteContents.AnyAsync(s => s.Group == Constants.PolicyPages.Group, ct)) return;

            // Key is unique across groups, so a key already used elsewhere is skipped.
            var keys = SeedPolicySiteContent.Rows.Select(r => r.Key).ToList();
            var existing = await context.SiteContents
                .Where(s => keys.Contains(s.Key))
                .Select(s => s.Key)
                .ToListAsync(ct);
            var missing = SeedPolicySiteContent.Rows.Where(r => !existing.Contains(r.Key)).ToList();
            if (missing.Count == 0) return;

            // Baseline rows were inserted with explicit Ids, so the sequence can lag behind MAX(Id).
            if (context.Database.IsNpgsql())
            {
                await context.Database.ExecuteSqlRawAsync(SeedPolicySiteContent.AdvanceSequenceSql, ct);
            }

            foreach (var row in missing)
            {
                context.SiteContents.Add(new SiteContent
                {
                    Key = row.Key,
                    Group = Constants.PolicyPages.Group,
                    Title = row.Title,
                    BodyHtml = row.Body,
                    DisplayOrder = row.Order,
                    IsActive = true,
                    LastModified = DateTime.UtcNow,
                });
            }

            await context.SaveChangesAsync(ct);
            logger.LogInformation("Policy content seed added {Count} site content rows.", missing.Count);
        }
    }
}

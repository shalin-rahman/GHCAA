using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GHCAA.Infrastructure.Data
{
    /// <summary>
    /// Brings the Constitution table in line with <c>Data/Seed/constitution.json</c> at boot.
    ///
    /// <para>
    /// This exists because <c>Database.EnsureCreated()</c> — the only schema/seed path this project
    /// runs at startup — is a no-op once the tables exist. On an already-populated database (preprod,
    /// production) the model's <c>HasData</c> seed never runs again, so editing the seed JSON alone
    /// can never publish a newly ratified constitution. This syncer closes that gap without
    /// introducing a runtime migration step.
    /// </para>
    ///
    /// <para>
    /// It is deliberately conservative: it only ever inserts a version it cannot find, and
    /// supersedes (never deletes) an existing version that members may have voted on. The one
    /// exception is <see cref="PlaceholderVersions"/> — versions that only ever existed as our own
    /// stand-in seed text — which are removed when they carry no amendment votes, so the public
    /// version history does not publish a document that was never ratified.
    /// </para>
    /// </summary>
    public static class ConstitutionSeeder
    {
        /// <summary>
        /// Versions that shipped as placeholder seed text rather than a ratified document. These are
        /// discarded rather than kept as history, provided no member has voted against them.
        /// </summary>
        private static readonly string[] PlaceholderVersions = { "1.2.0" };

        /// <summary>
        /// Idempotent. Safe to call on every boot and on a database that is already up to date.
        /// </summary>
        public static async Task SyncAsync(ApplicationDbContext context, ILogger logger, CancellationToken ct = default)
        {
            var seeded = ApplicationDbContext.LoadSeed<Constitution>("constitution.json");
            if (seeded.Count == 0)
            {
                logger.LogWarning("Constitution sync skipped — constitution.json produced no records.");
                return;
            }

            var existing = await context.Constitutions.ToListAsync(ct);
            var changed = false;

            foreach (var record in seeded)
            {
                var match = existing.FirstOrDefault(c =>
                    string.Equals(c.Version, record.Version, StringComparison.OrdinalIgnoreCase));

                if (match is null)
                {
                    // Let the database assign the key: Id 1 from the JSON is almost certainly taken
                    // by the row this version is replacing.
                    context.Constitutions.Add(new Constitution
                    {
                        Version = record.Version,
                        Content = record.Content,
                        PdfUrl = record.PdfUrl,
                        EffectiveDate = DateTime.SpecifyKind(record.EffectiveDate, DateTimeKind.Utc),
                        IsActive = record.IsActive,
                        ChangeSummary = record.ChangeSummary
                    });
                    changed = true;
                    logger.LogInformation("Constitution sync: inserting version {Version}.", record.Version);
                }
                else if (match.Content != record.Content || match.PdfUrl != record.PdfUrl ||
                         match.ChangeSummary != record.ChangeSummary || match.IsActive != record.IsActive)
                {
                    // The version is already stored but its text has been corrected in the seed —
                    // refresh it in place so the Id, and any votes hanging off it, survive.
                    match.Content = record.Content;
                    match.PdfUrl = record.PdfUrl;
                    match.ChangeSummary = record.ChangeSummary;
                    match.IsActive = record.IsActive;
                    match.EffectiveDate = DateTime.SpecifyKind(record.EffectiveDate, DateTimeKind.Utc);
                    if (record.IsActive) match.SupersededDate = null;
                    changed = true;
                    logger.LogInformation("Constitution sync: refreshing version {Version}.", record.Version);
                }
            }

            var seededVersions = seeded.Select(s => s.Version).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var newestEffective = seeded.Max(s => s.EffectiveDate);

            foreach (var stale in existing.Where(c => !seededVersions.Contains(c.Version)))
            {
                var isPlaceholder = PlaceholderVersions.Contains(stale.Version, StringComparer.OrdinalIgnoreCase);
                var hasVotes = await context.AmendmentVotes.AnyAsync(v => v.ConstitutionId == stale.Id, ct);

                if (isPlaceholder && !hasVotes)
                {
                    context.Constitutions.Remove(stale);
                    changed = true;
                    logger.LogInformation("Constitution sync: removing placeholder version {Version}.", stale.Version);
                }
                else if (stale.IsActive)
                {
                    // A real prior version stays in the public history; it just stops being current.
                    stale.IsActive = false;
                    stale.SupersededDate = DateTime.SpecifyKind(newestEffective, DateTimeKind.Utc);
                    changed = true;
                    logger.LogInformation("Constitution sync: superseding version {Version}.", stale.Version);
                }
            }

            if (changed) await context.SaveChangesAsync(ct);
        }
    }
}

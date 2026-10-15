using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Infrastructure.Data;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Services
{
    // 98.14. These are the only tables trimmed. Add one here only if losing its old rows breaks no
    // record the association must keep. Election tables never belong here (37.12s).
    public class DataRetentionService(ApplicationDbContext db) : IDataRetentionService
    {
        public static readonly string[] PurgedTables = ["Otps", "RefreshTokens", "Notifications", "EmailLogs", "ActivityLogs"];

        public async Task<IReadOnlyDictionary<string, int>> PurgeAsync(DateTime utcNow, CancellationToken cancellationToken = default)
        {
            var otpCutoff = utcNow.AddDays(-Constants.Retention.OtpDays);
            var tokenCutoff = utcNow.AddDays(-Constants.Retention.RefreshTokenDays);
            var notificationCutoff = utcNow.AddDays(-Constants.Retention.ReadNotificationDays);
            var emailCutoff = utcNow.AddDays(-Constants.Retention.EmailLogDays);
            var activityCutoff = utcNow.AddDays(-Constants.Retention.ActivityLogDays);
            var keptTypes = Constants.Retention.KeptActivityTypes;

            return new Dictionary<string, int>
            {
                ["Otps"] = await DeleteInBatchesAsync(db.Otps.Where(o => o.CreatedAt < otpCutoff), o => o.Id, cancellationToken),
                // RefreshToken has no revoked-at date. A revoked token was at most 7 days from expiry
                // when issued, so its creation date is close enough.
                // Both filtered tables hide rows of archived users. Those rows age out too.
                ["RefreshTokens"] = await DeleteInBatchesAsync(db.RefreshTokens.IgnoreQueryFilters()
                    .Where(r => r.ExpiresAt < tokenCutoff || (r.IsRevoked && r.CreatedAt < tokenCutoff)), r => r.Id, cancellationToken),
                ["Notifications"] = await DeleteInBatchesAsync(db.Notifications.IgnoreQueryFilters()
                    .Where(n => n.IsRead && n.CreatedAt < notificationCutoff), n => n.Id, cancellationToken),
                ["EmailLogs"] = await DeleteInBatchesAsync(db.EmailLogs.Where(e => e.SentDate < emailCutoff), e => e.Id, cancellationToken),
                ["ActivityLogs"] = await DeleteInBatchesAsync(db.ActivityLogs
                    .Where(a => a.Timestamp < activityCutoff
                        && !a.ActivityType.StartsWith(Constants.Retention.KeptActivityTypePrefix)
                        && !keptTypes.Contains(a.ActivityType)), a => a.Id, cancellationToken),
            };
        }

        private static async Task<int> DeleteInBatchesAsync<T>(IQueryable<T> rows, Expression<Func<T, int>> key, CancellationToken cancellationToken)
        {
            var total = 0;
            int deleted;
            do
            {
                // Take needs an OrderBy, or EF warns (10102) and the rows picked per batch are undefined.
                deleted = await rows.OrderBy(key)
                    .Take(Constants.Retention.DeleteBatchSize).ExecuteDeleteAsync(cancellationToken);
                total += deleted;
            }
            while (deleted == Constants.Retention.DeleteBatchSize);
            return total;
        }
    }
}

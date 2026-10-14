using GHCAA.Domain;
using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using System.Data.Common;
using System.Runtime.CompilerServices;

namespace GHCAA.Infrastructure.Data
{
    // 98.8. Drops the cached stamp of every user whose stamp a save changed, so a rotation takes
    // effect on the next request instead of after the cache window. Doing it here rather than at
    // each call site means a new rotation path cannot forget it.
    //
    // Inside a transaction the new stamp is not visible to other connections until commit, so a
    // request in that gap can read the old stamp and cache it again. Those ids are evicted a second
    // time once the transaction commits.
    public class SecurityStampCacheInterceptor(IMemoryCache cache) : SaveChangesInterceptor, IDbTransactionInterceptor
    {
        // One interceptor serves every pooled context, so the ids are kept per context.
        private readonly ConditionalWeakTable<DbContext, List<int>> _rotated = new();
        private readonly ConditionalWeakTable<DbContext, List<int>> _awaitingCommit = new();

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            Collect(eventData.Context);
            return result;
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            Collect(eventData.Context);
            return ValueTask.FromResult(result);
        }

        public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
        {
            Evict(eventData.Context);
            return result;
        }

        public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken cancellationToken = default)
        {
            Evict(eventData.Context);
            return ValueTask.FromResult(result);
        }

        // A failed save wrote nothing, so the ids it collected are dropped unused.
        public override void SaveChangesFailed(DbContextErrorEventData eventData) => Forget(eventData.Context);

        public override Task SaveChangesFailedAsync(DbContextErrorEventData eventData, CancellationToken cancellationToken = default)
        {
            Forget(eventData.Context);
            return Task.CompletedTask;
        }

        public override void SaveChangesCanceled(DbContextEventData eventData) => Forget(eventData.Context);

        public override Task SaveChangesCanceledAsync(DbContextEventData eventData, CancellationToken cancellationToken = default)
        {
            Forget(eventData.Context);
            return Task.CompletedTask;
        }

        private void Forget(DbContext? context)
        {
            if (context != null) _rotated.Remove(context);
        }

        public void TransactionCommitted(DbTransaction transaction, TransactionEndEventData eventData) =>
            EvictAfterCommit(eventData.Context);

        public Task TransactionCommittedAsync(DbTransaction transaction, TransactionEndEventData eventData, CancellationToken cancellationToken = default)
        {
            EvictAfterCommit(eventData.Context);
            return Task.CompletedTask;
        }

        // A transaction disposed without a commit or rollback raises neither event, so its ids would
        // sit here and be evicted by the next commit on the same context. A new transaction starts clean.
        public DbTransaction TransactionStarted(DbConnection connection, TransactionEndEventData eventData, DbTransaction result)
        {
            if (eventData.Context != null) _awaitingCommit.Remove(eventData.Context);
            return result;
        }

        public ValueTask<DbTransaction> TransactionStartedAsync(DbConnection connection, TransactionEndEventData eventData, DbTransaction result, CancellationToken cancellationToken = default)
        {
            if (eventData.Context != null) _awaitingCommit.Remove(eventData.Context);
            return ValueTask.FromResult(result);
        }

        public void TransactionRolledBack(DbTransaction transaction, TransactionEndEventData eventData)
        {
            if (eventData.Context != null) _awaitingCommit.Remove(eventData.Context);
        }

        public Task TransactionRolledBackAsync(DbTransaction transaction, TransactionEndEventData eventData, CancellationToken cancellationToken = default)
        {
            TransactionRolledBack(transaction, eventData);
            return Task.CompletedTask;
        }

        // The ids are read before the save because the change tracker resets the Modified flag after it.
        private void Collect(DbContext? context)
        {
            if (context == null) return;
            var ids = context.ChangeTracker.Entries<User>()
                .Where(e => e.State == EntityState.Modified && e.Property(u => u.SecurityStamp).IsModified)
                .Select(e => e.Entity.Id)
                .ToList();
            _rotated.AddOrUpdate(context, ids);
        }

        private void Evict(DbContext? context)
        {
            if (context == null || !_rotated.TryGetValue(context, out var ids)) return;
            _rotated.Remove(context);
            foreach (var id in ids) cache.Remove(Constants.SecurityStamps.CacheKey(id));

            if (ids.Count > 0 && context.Database.CurrentTransaction != null)
            {
                var waiting = _awaitingCommit.GetOrCreateValue(context);
                waiting.AddRange(ids);
            }
        }

        private void EvictAfterCommit(DbContext? context)
        {
            if (context == null || !_awaitingCommit.TryGetValue(context, out var ids)) return;
            _awaitingCommit.Remove(context);
            foreach (var id in ids) cache.Remove(Constants.SecurityStamps.CacheKey(id));
        }
    }
}

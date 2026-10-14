using GHCAA.Domain;
using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using System.Runtime.CompilerServices;

namespace GHCAA.Infrastructure.Data
{
    // 98.8. Drops the cached stamp of every user whose stamp a save changed, so a rotation takes
    // effect on the next request instead of after the cache window. Doing it here rather than at
    // each call site means a new rotation path cannot forget it.
    public class SecurityStampCacheInterceptor(IMemoryCache cache) : SaveChangesInterceptor
    {
        // One interceptor serves every pooled context, so the ids are kept per context.
        private readonly ConditionalWeakTable<DbContext, List<int>> _rotated = new();

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
        }
    }
}

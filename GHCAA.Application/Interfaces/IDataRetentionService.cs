namespace GHCAA.Application.Interfaces
{
    public interface IDataRetentionService
    {
        // Deletes log rows older than their Constants.Retention period and returns the count per table.
        Task<IReadOnlyDictionary<string, int>> PurgeAsync(DateTime utcNow, CancellationToken cancellationToken = default);
    }
}

namespace GHCAA.Application.Interfaces
{
    public interface IDatabaseHealthService
    {
        Task<bool> CanConnectAsync(CancellationToken cancellationToken = default);
    }
}

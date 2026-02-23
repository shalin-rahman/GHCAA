using GHCAA.Domain.Models;

namespace GHCAA.Application.Interfaces
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(int userId, string title, string message, string type, string? targetUrl = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId, CancellationToken cancellationToken = default);
        Task MarkAsReadAsync(int notificationId, CancellationToken cancellationToken = default);
        Task MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default);
    }
}

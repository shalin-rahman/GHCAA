using GHCAA.Domain.Models;
using GHCAA.Domain;

namespace GHCAA.Application.Interfaces
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(int userId, string title, string message, Enums.NotificationType type, string? targetUrl = null, CancellationToken cancellationToken = default);
        Task BroadcastNotificationAsync(string title, string message, Enums.NotificationType type, string? targetUrl = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId, CancellationToken cancellationToken = default);
        Task MarkAsReadAsync(int notificationId, CancellationToken cancellationToken = default);
        Task MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default);
    }
}

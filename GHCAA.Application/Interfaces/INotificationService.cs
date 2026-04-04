using GHCAA.Domain.Models;
using GHCAA.Domain;

namespace GHCAA.Application.Interfaces
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(int memberId, string title, string message, Enums.NotificationType type, string? targetUrl = null, CancellationToken cancellationToken = default);
        Task BroadcastNotificationAsync(string title, string message, Enums.NotificationType type, string? targetUrl = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(int memberId, CancellationToken cancellationToken = default);
        Task<bool> MarkAsReadAsync(int notificationId, int memberId, CancellationToken cancellationToken = default);
        Task MarkAllAsReadAsync(int memberId, CancellationToken cancellationToken = default);
    }
}

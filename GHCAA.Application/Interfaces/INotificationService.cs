using GHCAA.Domain.Models;
using GHCAA.Domain;

namespace GHCAA.Application.Interfaces
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(int memberId, string title, string message, Enums.NotificationType type, string? targetUrl = null, CancellationToken cancellationToken = default);
        Task BroadcastNotificationAsync(string title, string message, Enums.NotificationType type, string? targetUrl = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Same as <see cref="CreateNotificationAsync"/>, but resolves title/message from the
        /// EmailTemplate row named by <paramref name="templateCode"/> instead of taking literal
        /// text. Falls back to <paramref name="fallbackTitle"/>/<paramref name="fallbackMessage"/>
        /// when no template with that code exists. Returns true if a template was used.
        /// </summary>
        Task<bool> CreateNotificationFromTemplateAsync(int memberId, string templateCode, Enums.NotificationType type, string fallbackTitle, string fallbackMessage, Dictionary<string, string>? templateVars = null, string? targetUrl = null, CancellationToken cancellationToken = default);
        Task<IEnumerable<Notification>> GetUserNotificationsAsync(int memberId, CancellationToken cancellationToken = default);
        Task<bool> MarkAsReadAsync(int notificationId, int memberId, CancellationToken cancellationToken = default);
        Task MarkAllAsReadAsync(int memberId, CancellationToken cancellationToken = default);
    }
}

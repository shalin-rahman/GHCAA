using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using GHCAA.API.Hubs; // I'll need to move the Hub or handle dependency carefully.

namespace GHCAA.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _db;
        // We'll use IHubContext to send real-time notifications
        // But wait, NotificationService is in Infrastructure, ChatHub is in API.
        // This is a common circular dependency issue.
        // Best practice: Interface for Hub or use a Message Broker.
        // For simplicity here, I'll just use the DB for now and let the API pull or use a specific event.
        // Actually, I can use IHubContext<ChatHub> if I move Hub to a neutral place or use dynamic.
        
        public NotificationService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task CreateNotificationAsync(int userId, string title, string message, string type, string? targetUrl = null, CancellationToken cancellationToken = default)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                TargetUrl = targetUrl,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync(cancellationToken);
            
            // Note: Real-time broadcast will be handled by a higher level or by injecting IHubContext if possible.
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .ToListAsync(cancellationToken);
        }

        public async Task MarkAsReadAsync(int notificationId, CancellationToken cancellationToken = default)
        {
            var n = await _db.Notifications.FindAsync(new object[] { notificationId }, cancellationToken);
            if (n != null)
            {
                n.IsRead = true;
                await _db.SaveChangesAsync(cancellationToken);
            }
        }

        public async Task MarkAllAsReadAsync(int userId, CancellationToken cancellationToken = default)
        {
            var notifications = await _db.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ToListAsync(cancellationToken);
            
            foreach (var n in notifications) n.IsRead = true;
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}

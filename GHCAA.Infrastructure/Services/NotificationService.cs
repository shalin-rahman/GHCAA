using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _db;
        private readonly IRealTimeService _realTime;
        
        public NotificationService(ApplicationDbContext db, IRealTimeService realTime)
        {
            _db = db;
            _realTime = realTime;
        }

        public async Task CreateNotificationAsync(int userId, string title, string message, string type, string? targetUrl = null, CancellationToken cancellationToken = default)
        {
            var notification = new Notification
            {
                MemberId = userId,
                Title = title,
                Message = message,
                Type = type,
                TargetUrl = targetUrl,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync(cancellationToken);
            
            // Broadcast in real-time
            await _realTime.SendNotificationToUserAsync(userId, notification);
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(int userId, CancellationToken cancellationToken = default)
        {
            return await _db.Notifications
                .Where(n => n.MemberId == userId)
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
                .Where(n => n.MemberId == userId && !n.IsRead)
                .ToListAsync(cancellationToken);
            
            foreach (var n in notifications) n.IsRead = true;
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}

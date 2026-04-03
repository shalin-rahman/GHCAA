using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GHCAA.Domain;
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

        public async Task CreateNotificationAsync(int userId, string title, string message, Enums.NotificationType type, string? targetUrl = null, CancellationToken cancellationToken = default)
        {
            // Check member notification preferences
            var member = await _db.Members.FindAsync(new object[] { userId }, cancellationToken);
            if (member != null)
            {
                bool shouldNotify = type switch
                {
                    Enums.NotificationType.EventCreation => member.NotifyEventCreation,
                    Enums.NotificationType.ParticipationApproval => member.NotifyParticipationApproval,
                    Enums.NotificationType.RegistrationUpdate => member.NotifyRegistrationUpdate,
                    Enums.NotificationType.GeneralSystem => member.NotifyRelevantUpdates,
                    _ => true // Direct messages always notify
                };

                if (!shouldNotify) return;
            }

            var notification = new Notification
            {
                MemberId = userId,
                Title = title,
                Message = message,
                Type = type.ToString(),
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

        public async Task BroadcastNotificationAsync(string title, string message, Enums.NotificationType type, string? targetUrl = null, CancellationToken cancellationToken = default)
        {
            IQueryable<Member> targetMembers = _db.Members.Where(m => !m.IsArchived);

            // Filter by preference
            targetMembers = type switch
            {
                Enums.NotificationType.EventCreation => targetMembers.Where(m => m.NotifyEventCreation),
                Enums.NotificationType.ParticipationApproval => targetMembers.Where(m => m.NotifyParticipationApproval),
                Enums.NotificationType.RegistrationUpdate => targetMembers.Where(m => m.NotifyRegistrationUpdate),
                Enums.NotificationType.GeneralSystem => targetMembers.Where(m => m.NotifyRelevantUpdates),
                _ => targetMembers
            };

            var membersIndices = await targetMembers.Select(m => m.Id).ToListAsync(cancellationToken);

            foreach (var memberId in membersIndices)
            {
                // Note: we're reusing the already tuned CreateNotificationAsync for consistent real-time broadcast.
                // In huge systems, we'd use background jobs or batch signals.
                await CreateNotificationAsync(memberId, title, message, type, targetUrl, cancellationToken);
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

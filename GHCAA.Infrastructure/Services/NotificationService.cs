using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
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

        public async Task CreateNotificationAsync(int memberId, string title, string message, Enums.NotificationType type, string? targetUrl = null, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members.FindAsync(new object[] { memberId }, cancellationToken);
            if (member != null)
            {
                bool shouldNotify = type switch
                {
                    Enums.NotificationType.EventCreation => member.NotifyEventCreation,
                    Enums.NotificationType.ParticipationApproval => member.NotifyParticipationApproval,
                    Enums.NotificationType.RegistrationUpdate => member.NotifyRegistrationUpdate,
                    Enums.NotificationType.GeneralSystem => member.NotifyRelevantUpdates,
                    _ => true
                };

                if (!shouldNotify) return;
            }

            var notification = new Notification
            {
                MemberId = memberId,
                Title = title,
                Message = message,
                Type = type.ToString(),
                TargetUrl = targetUrl,
                CreatedAt = DateTime.UtcNow,
                IsRead = false
            };

            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync(cancellationToken);
            
            await _realTime.SendNotificationToUserAsync(memberId, notification);
        }

        public async Task<IEnumerable<Notification>> GetUserNotificationsAsync(int memberId, CancellationToken cancellationToken = default)
        {
            return await _db.Notifications
                .Where(n => n.MemberId == memberId)
                .OrderByDescending(n => n.CreatedAt)
                .Take(50)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> MarkAsReadAsync(int notificationId, int memberId, CancellationToken cancellationToken = default)
        {
            var n = await _db.Notifications
                .FirstOrDefaultAsync(x => x.Id == notificationId && x.MemberId == memberId, cancellationToken);
            if (n == null) return false;
            n.IsRead = true;
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task BroadcastNotificationAsync(string title, string message, Enums.NotificationType type, string? targetUrl = null, CancellationToken cancellationToken = default)
        {
            IQueryable<Member> targetMembers = _db.Members.Where(m => !m.IsArchived);

            targetMembers = type switch
            {
                Enums.NotificationType.EventCreation => targetMembers.Where(m => m.NotifyEventCreation),
                Enums.NotificationType.ParticipationApproval => targetMembers.Where(m => m.NotifyParticipationApproval),
                Enums.NotificationType.RegistrationUpdate => targetMembers.Where(m => m.NotifyRegistrationUpdate),
                Enums.NotificationType.GeneralSystem => targetMembers.Where(m => m.NotifyRelevantUpdates),
                _ => targetMembers
            };

            var memberIds = await targetMembers.Select(m => m.Id).ToListAsync(cancellationToken);
            if (memberIds.Count == 0) return;

            var now = DateTime.UtcNow;
            var typeString = type.ToString();
            var entities = memberIds.Select(mid => new Notification
            {
                MemberId = mid,
                Title = title,
                Message = message,
                Type = typeString,
                TargetUrl = targetUrl,
                CreatedAt = now,
                IsRead = false
            }).ToList();

            _db.Notifications.AddRange(entities);
            await _db.SaveChangesAsync(cancellationToken);

            var sendTasks = entities.Select(n => _realTime.SendNotificationToUserAsync(n.MemberId, n));
            await Task.WhenAll(sendTasks);
        }

        public async Task MarkAllAsReadAsync(int memberId, CancellationToken cancellationToken = default)
        {
            var notifications = await _db.Notifications
                .Where(n => n.MemberId == memberId && !n.IsRead)
                .ToListAsync(cancellationToken);
            
            foreach (var n in notifications) n.IsRead = true;
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}

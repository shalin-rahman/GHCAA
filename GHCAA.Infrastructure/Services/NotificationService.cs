using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        private readonly ICommunicationService _communication;

        public NotificationService(ApplicationDbContext db, IRealTimeService realTime, ICommunicationService communication)
        {
            _db = db;
            _realTime = realTime;
            _communication = communication;
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
                    Enums.NotificationType.CommitteeAssignment => member.NotifyCommitteeChanges,
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

        public async Task<bool> CreateNotificationFromTemplateAsync(int memberId, string templateCode, Enums.NotificationType type, string fallbackTitle, string fallbackMessage, Dictionary<string, string>? templateVars = null, string? targetUrl = null, CancellationToken cancellationToken = default)
        {
            var resolved = await _communication.ResolveTemplateTextAsync(templateCode, memberId, templateVars, cancellationToken);

            string title = fallbackTitle;
            string message = fallbackMessage;
            if (resolved.HasValue)
            {
                title = resolved.Value.Subject;
                message = StripHtml(resolved.Value.Body);
            }

            await CreateNotificationAsync(memberId, title, message, type, targetUrl, cancellationToken);
            return resolved.HasValue;
        }

        // EmailTemplate bodies are authored as HTML for the email channel. The in-app Notification
        // table stores plain text, so this strips tags rather than rendering markup in a list.
        private static string StripHtml(string html)
        {
            var text = Regex.Replace(html, "<.*?>", " ");
            text = System.Net.WebUtility.HtmlDecode(text);
            return Regex.Replace(text, @"\s+", " ").Trim();
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
                Enums.NotificationType.CommitteeAssignment => targetMembers.Where(m => m.NotifyCommitteeChanges),
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

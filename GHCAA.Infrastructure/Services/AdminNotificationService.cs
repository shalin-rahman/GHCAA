using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Services
{
    /// <summary>
    /// Shared helper used by every place that creates a Pending gallery/photo/job submission so the
    /// admin-lookup + in-app notification + email fan-out logic is written once instead of three times.
    /// </summary>
    public class AdminNotificationService : IAdminNotificationService
    {
        private readonly ApplicationDbContext _db;
        private readonly INotificationService _notification;
        private readonly IEmailService _email;

        public AdminNotificationService(ApplicationDbContext db, INotificationService notification, IEmailService email)
        {
            _db = db;
            _notification = notification;
            _email = email;
        }

        public async Task NotifyPendingApprovalAsync(string itemType, string itemTitle, string submitterName, string approvalUrl, CancellationToken cancellationToken = default)
        {
            var admins = await _db.Users
                .Include(u => u.Member)
                .Where(u => u.MemberId != null && u.Roles.Any(r => r.Name == "Admin" || r.Name == "SuperAdmin"))
                .Select(u => new { MemberId = u.MemberId!.Value, Email = u.Member!.Email })
                .Distinct()
                .ToListAsync(cancellationToken);

            var title = $"New {itemType} Pending Approval";
            var message = $"{submitterName} submitted a {itemType.ToLower()} \"{itemTitle}\" that needs your review.";

            foreach (var admin in admins)
            {
                await _notification.CreateNotificationAsync(
                    admin.MemberId,
                    title,
                    message,
                    Enums.NotificationType.ApprovalRequest,
                    approvalUrl,
                    cancellationToken);

                if (!string.IsNullOrWhiteSpace(admin.Email))
                {
                    var html = $@"
                        <div style='font-family: sans-serif; padding: 20px; border: 1px solid #eee; border-radius: 10px;'>
                            <h2 style='color: #c5a059;'>{title}</h2>
                            <p>{submitterName} submitted a {itemType.ToLower()} '{itemTitle}' that needs your review.</p>
                            <p><a href='{approvalUrl}'>Review it now</a></p>
                        </div>";

                    try
                    {
                        await _email.SendEmailAsync(admin.Email, title, html, cancellationToken);
                    }
                    catch
                    {
                        // Best-effort: email failures should not block the notification flow.
                    }
                }
            }
        }
    }
}

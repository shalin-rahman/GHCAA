using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GHCAA.Infrastructure.Services
{
    public class ContactService : IContactService
    {
        private readonly ApplicationDbContext _db;
        private readonly ICommunicationService _communication;
        private readonly ContactUsSettingsOptions _contactSettings;
        private readonly ILogger<ContactService> _logger;

        public ContactService(ApplicationDbContext db, ICommunicationService communication, IOptions<ContactUsSettingsOptions> contactSettings, ILogger<ContactService> logger)
        {
            _db = db;
            _communication = communication;
            _contactSettings = contactSettings.Value;
            _logger = logger;
        }

        public async Task SubmitMessageAsync(ContactMessageDto dto, CancellationToken cancellationToken = default)
        {
            var msg = new ContactMessage
            {
                FullName = dto.FullName,
                Email = dto.Email,
                Subject = dto.Subject,
                Message = dto.Message,
                SubmittedAt = DateTime.UtcNow
            };

            _db.ContactMessages.Add(msg);
            await _db.SaveChangesAsync(cancellationToken);

            // Send notification to recipients
            var recipients = _contactSettings.Recipients;
            if (recipients != null && recipients.Length > 0)
            {
                var customVars = new Dictionary<string, string>
                {
                    { "RequesterName", msg.FullName },
                    { "RequesterEmail", msg.Email },
                    { "Subject", msg.Subject },
                    { "Message", msg.Message }
                };

                foreach (var email in recipients)
                {
                    try
                    {
                        await _communication.SendEmailByCodeAsync(email, "PORTAL_ENQUIRY", customVars, null, cancellationToken);
                    }
                    catch (Exception ex)
                    {
                        // The message is already saved, so a mail failure must not fail the request.
                        _logger.LogWarning(ex, "Contact enquiry {MessageId} could not be emailed to {Recipient}.", msg.Id, email);
                    }
                }
            }
        }

        public async Task<IEnumerable<object>> GetMessagesAsync(CancellationToken cancellationToken = default)
        {
            return await _db.ContactMessages
                .OrderByDescending(m => m.SubmittedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<bool> MarkAsReadAsync(int id, CancellationToken cancellationToken = default)
        {
            var msg = await _db.ContactMessages.FindAsync(new object[] { id }, cancellationToken);
            if (msg == null) return false;

            msg.IsRead = true;
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<bool> DeleteMessageAsync(int id, CancellationToken cancellationToken = default)
        {
            var msg = await _db.ContactMessages.FindAsync(new object[] { id }, cancellationToken);
            if (msg == null) return false;

            _db.ContactMessages.Remove(msg);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}

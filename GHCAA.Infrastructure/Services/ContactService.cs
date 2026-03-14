using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace GHCAA.Infrastructure.Services
{
    public class ContactService : IContactService
    {
        private readonly ApplicationDbContext _db;
        private readonly IEmailService _email;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config;

        public ContactService(ApplicationDbContext db, IEmailService email, Microsoft.Extensions.Configuration.IConfiguration config)
        {
            _db = db;
            _email = email;
            _config = config;
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
            var recipients = _config.GetSection("ContactUsSettings:Recipients").Get<string[]>();
            if (recipients != null && recipients.Length > 0)
            {
                var body = $"<h3>New Portal Enquiry</h3>" +
                           $"<p><strong>From:</strong> {msg.FullName} ({msg.Email})</p>" +
                           $"<p><strong>Subject:</strong> {msg.Subject}</p>" +
                           $"<hr/>" +
                           $"<p>{msg.Message}</p>" +
                           $"<br/><p><small>Submitted via GHCAA Portal at {msg.SubmittedAt:f}</small></p>";

                foreach (var email in recipients)
                {
                    try
                    {
                        await _email.SendEmailAsync(email, $"Portal Enquiry: {msg.Subject}", body);
                    }
                    catch
                    {
                        // Log failure but don't block submission
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
    }
}

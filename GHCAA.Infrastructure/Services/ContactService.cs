using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GHCAA.Infrastructure.Services
{
    public class ContactService : IContactService
    {
        private readonly ApplicationDbContext _db;

        public ContactService(ApplicationDbContext db)
        {
            _db = db;
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
        }

        public async Task<IEnumerable<object>> GetMessagesAsync(CancellationToken cancellationToken = default)
        {
            return await _db.ContactMessages
                .OrderByDescending(m => m.SubmittedAt)
                .ToListAsync(cancellationToken);
        }
    }
}

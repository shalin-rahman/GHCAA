using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Services
{
    public class ChatService : IChatService
    {
        private readonly ApplicationDbContext _db;

        public ChatService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<ChatMessage> SendMessageAsync(int senderId, int receiverId, string content, CancellationToken cancellationToken = default)
        {
            var message = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                MessageContent = content,
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            await _db.ChatMessages.AddAsync(message, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return message;
        }

        public async Task<IEnumerable<ChatMessage>> GetChatHistoryAsync(int member1Id, int member2Id, int count = 50, CancellationToken cancellationToken = default)
        {
            return await _db.ChatMessages
                .Where(m => (m.SenderId == member1Id && m.ReceiverId == member2Id) ||
                            (m.SenderId == member2Id && m.ReceiverId == member1Id))
                .OrderByDescending(m => m.SentAt)
                .Take(count)
                .OrderBy(m => m.SentAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<ChatMessage>> GetUnreadMessagesAsync(int memberId, CancellationToken cancellationToken = default)
        {
            return await _db.ChatMessages
                .Where(m => m.ReceiverId == memberId && !m.IsRead)
                .OrderByDescending(m => m.SentAt)
                .ToListAsync(cancellationToken);
        }

        public async Task MarkAsReadAsync(int messageId, CancellationToken cancellationToken = default)
        {
            var message = await _db.ChatMessages.FindAsync(new object[] { messageId }, cancellationToken);
            if (message != null)
            {
                message.IsRead = true;
                await _db.SaveChangesAsync(cancellationToken);
            }
        }
    }
}

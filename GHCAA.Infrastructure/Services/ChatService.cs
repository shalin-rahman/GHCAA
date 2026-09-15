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

        public async Task<bool> MarkAsReadAsync(int messageId, int userId, CancellationToken cancellationToken = default)
        {
            var message = await _db.ChatMessages.FindAsync(new object[] { messageId }, cancellationToken);
            if (message == null || message.ReceiverId != userId) return false;

            message.IsRead = true;
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<IEnumerable<object>> GetRecentChatsAsync(int userId, CancellationToken cancellationToken = default)
        {
            var sent = _db.ChatMessages.Where(m => m.SenderId == userId).Select(m => m.ReceiverId);
            var received = _db.ChatMessages.Where(m => m.ReceiverId == userId).Select(m => m.SenderId);
            var otherUserIds = await sent.Union(received).Distinct().ToListAsync(cancellationToken);

            var recentChats = new List<object>();

            foreach (var otherId in otherUserIds)
            {
                var lastMsg = await _db.ChatMessages
                    .Where(m => (m.SenderId == userId && m.ReceiverId == otherId) ||
                                (m.SenderId == otherId && m.ReceiverId == userId))
                    .OrderByDescending(m => m.SentAt)
                    .FirstOrDefaultAsync(cancellationToken);

                var otherMember = await _db.Members
                    .Join(_db.Users, m => m.Id, u => u.MemberId, (m, u) => new { m, u })
                    .Where(x => x.u.Id == otherId)
                    .Select(x => new { x.m.FullName, x.m.PhotoPath })
                    .FirstOrDefaultAsync(cancellationToken);

                if (lastMsg != null)
                {
                    recentChats.Add(new
                    {
                        UserId = otherId,
                        FullName = otherMember?.FullName ?? "Unknown Member",
                        PhotoPath = otherMember?.PhotoPath,
                        LastMessage = lastMsg.MessageContent,
                        LastMessageTime = lastMsg.SentAt,
                        IsRead = lastMsg.IsRead || lastMsg.SenderId == userId
                    });
                }
            }

            return recentChats.OrderByDescending(x => ((dynamic)x).LastMessageTime);
        }
    }
}

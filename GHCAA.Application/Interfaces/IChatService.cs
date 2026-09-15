using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Domain.Models;

namespace GHCAA.Application.Interfaces
{
    public interface IChatService
    {
        Task<ChatMessage> SendMessageAsync(int senderId, int receiverId, string content, CancellationToken cancellationToken = default);
        Task<IEnumerable<ChatMessage>> GetChatHistoryAsync(int member1Id, int member2Id, int count = 50, CancellationToken cancellationToken = default);
        Task<IEnumerable<ChatMessage>> GetUnreadMessagesAsync(int memberId, CancellationToken cancellationToken = default);
        Task<bool> MarkAsReadAsync(int messageId, int userId, CancellationToken cancellationToken = default);
        Task<IEnumerable<object>> GetRecentChatsAsync(int userId, CancellationToken cancellationToken = default);
    }
}

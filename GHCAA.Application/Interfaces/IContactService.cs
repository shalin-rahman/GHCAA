using GHCAA.Application.DTOs;
using System.Threading;
using System.Threading.Tasks;

namespace GHCAA.Application.Interfaces
{
    public interface IContactService
    {
        Task SubmitMessageAsync(ContactMessageDto dto, CancellationToken cancellationToken = default);
        Task<IEnumerable<object>> GetMessagesAsync(CancellationToken cancellationToken = default);
        Task<bool> MarkAsReadAsync(int id, CancellationToken cancellationToken = default);
    }
}

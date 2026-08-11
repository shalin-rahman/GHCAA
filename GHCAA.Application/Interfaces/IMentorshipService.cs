using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Domain.Models;

namespace GHCAA.Application.Interfaces
{
    public interface IMentorshipService
    {
        Task<MentorshipRequest> SendRequestAsync(int requesterId, int mentorId, string? message, string? domain, CancellationToken ct = default);
        Task<IEnumerable<object>> GetSentRequestsAsync(int requesterId, CancellationToken ct = default);
        Task<IEnumerable<object>> GetReceivedRequestsAsync(int mentorId, CancellationToken ct = default);
        Task<bool> RespondAsync(int requestId, int mentorId, bool accept, string? note, CancellationToken ct = default);
        Task<bool> MarkCompleteAsync(int requestId, int memberId, CancellationToken ct = default);
        Task<IEnumerable<object>> GetAllForAdminAsync(CancellationToken ct = default);
    }
}

using GHCAA.Application.DTOs;

namespace GHCAA.Application.Interfaces
{
    public interface IFamilyLinkService
    {
        /// <summary>Send a family link request to another member by their membership number.</summary>
        Task<FamilyLinkRequestDto> SendRequestAsync(int requesterId, SendFamilyLinkDto dto, CancellationToken ct = default);

        /// <summary>Approve or reject an incoming request. Only the target member may respond.</summary>
        Task<bool> RespondAsync(int respondingMemberId, RespondFamilyLinkDto dto, CancellationToken ct = default);

        /// <summary>Cancel a pending request that the caller sent.</summary>
        Task<bool> CancelAsync(int requesterId, int requestId, CancellationToken ct = default);

        /// <summary>Get all requests sent by this member.</summary>
        Task<List<FamilyLinkRequestDto>> GetSentRequestsAsync(int memberId, CancellationToken ct = default);

        /// <summary>Get all pending requests received by this member.</summary>
        Task<List<FamilyLinkRequestDto>> GetReceivedRequestsAsync(int memberId, CancellationToken ct = default);

        /// <summary>Return the approved family network of a member.</summary>
        Task<List<FamilyLinkRequestDto>> GetFamilyAsync(int memberId, CancellationToken ct = default);

        /// <summary>Remove an existing family link. Either party can initiate this.</summary>
        Task<bool> RemoveLinkAsync(int currentMemberId, int requestId, CancellationToken ct = default);
    }
}

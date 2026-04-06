using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using static GHCAA.Domain.Enums;

namespace GHCAA.Application.Interfaces
{
    public interface IFamilyService
    {
        /// <summary>
        /// Sends a family link request to another member.
        /// </summary>
        Task<bool> SendRequestAsync(int requesterId, CreateFamilyRequestDto dto, CancellationToken cancellationToken = default);

        /// <summary>
        /// Responds to a family link request (Accept/Reject).
        /// </summary>
        Task<bool> RespondAsync(int targetMemberId, int requestId, FamilyLinkStatus status, CancellationToken cancellationToken = default);

        /// <summary>
        /// Cancels a request sent by the current member.
        /// </summary>
        Task<bool> CancelRequestAsync(int requesterId, int requestId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets all family link requests for the current member (sent or received).
        /// </summary>
        Task<IEnumerable<FamilyRequestDto>> GetRequestsAsync(int memberId, bool receivedOnly = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets members linked to the current member.
        /// </summary>
        Task<IEnumerable<MemberSummaryDto>> GetLinkedMembersAsync(int memberId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Unlinks a previously established family connection.
        /// </summary>
        Task<bool> UnlinkAsync(int memberId, int linkedMemberId, CancellationToken cancellationToken = default);
    }

    public class CreateFamilyRequestDto
    {
        public int TargetMemberId { get; set; }
        public RelationshipType Relationship { get; set; }
        public string? Note { get; set; }
    }

    public class FamilyRequestDto
    {
        public int Id { get; set; }
        public int RequesterId { get; set; }
        public string RequesterName { get; set; } = null!;
        public int TargetMemberId { get; set; }
        public string TargetMemberName { get; set; } = null!;
        public RelationshipType Relationship { get; set; }
        public FamilyLinkStatus Status { get; set; }
        public DateTime RequestedAt { get; set; }
        public string? Note { get; set; }
        public bool IsReceived { get; set; } // UI helper to distinguish
    }
}

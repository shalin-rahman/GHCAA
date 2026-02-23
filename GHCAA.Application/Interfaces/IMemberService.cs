using GHCAA.Application.DTOs;
using GHCAA.Domain.Models;
using static GHCAA.Domain.Enums;
using System.Threading;

namespace GHCAA.Application.Interfaces
{
    public interface IMemberService
    {
        // Use UploadedFileDto so Application layer does not reference Microsoft.AspNetCore.Http
        Task<int> RegisterAsync(MemberRegistrationDto dto, UploadedFileDto? photo, UploadedFileDto? certificate, UploadedFileDto? paymentProof, CancellationToken cancellationToken = default);
        Task<bool> VerifyEmailAsync(string email, string code, CancellationToken cancellationToken = default);
        Task<ApproveMemberResultDto> ApproveMemberAsync(int memberId, int approvedByAdminId, CancellationToken cancellationToken = default);
        Task<MemberProfileDto?> GetProfileAsync(int memberId, CancellationToken cancellationToken = default);
        Task<bool> UpdateProfileAsync(int memberId, UpdateProfileDto dto, CancellationToken cancellationToken = default);
        Task<object> GetDashboardStatsAsync(CancellationToken cancellationToken = default);
        Task<MemberRegistrationResultDto> GetStatusAsync(int id, CancellationToken cancellationToken = default);
        Task<object?> GetMemberDocumentsAsync(int memberId, CancellationToken cancellationToken = default);
        // Admin/SuperAdmin Operations
        Task<bool> ArchiveMemberAsync(int memberId, CancellationToken cancellationToken = default);
        Task<bool> RestoreMemberAsync(int memberId, CancellationToken cancellationToken = default);
        Task<bool> ReactivateMemberAsync(int memberId, CancellationToken cancellationToken = default);
        Task<IEnumerable<MemberProfileDto>> GetAllMembersAsync(bool includeArchived = false, CancellationToken cancellationToken = default);
        Task<bool> AdminUpdateMemberAsync(int id, AdminMemberUpdateDto dto, CancellationToken cancellationToken = default);
        Task<bool> RejectMemberAsync(int id, int adminId, string reason, CancellationToken cancellationToken = default);
    }
}
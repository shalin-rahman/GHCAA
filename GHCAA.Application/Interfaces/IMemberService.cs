using GHCAA.Application.DTOs;
using System.Threading;

namespace GHCAA.Application.Interfaces
{
    public interface IMemberService
    {
        // Use UploadedFileDto so Application layer does not reference Microsoft.AspNetCore.Http
        Task<int> RegisterAsync(MemberRegistrationDto dto, UploadedFileDto? photo, UploadedFileDto? certificate, UploadedFileDto? paymentProof, CancellationToken cancellationToken = default);
        Task<MemberRegistrationResultDto> GetStatusAsync(int memberId, CancellationToken cancellationToken = default);
        Task<bool> VerifyEmailAsync(string email, string otpCode, CancellationToken cancellationToken = default);
        Task<ApproveMemberResultDto> ApproveMemberAsync(int memberId, int approvedByAdminId, CancellationToken cancellationToken = default);
    }
}
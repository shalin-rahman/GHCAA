using GHCAA.Application.DTOs;

namespace GHCAA.Application.Interfaces;

public interface IScholarshipService
{
    Task<IEnumerable<ScholarshipFundDto>> GetPublicFundsAsync(CancellationToken cancellationToken = default);
    Task<IEnumerable<ScholarshipCallDto>> GetPublicCallsAsync(CancellationToken cancellationToken = default);
    Task<ScholarshipApplicationDto> SubmitApplicationAsync(int callId, CreateScholarshipApplicationDto dto, CancellationToken cancellationToken = default);
    Task<ScholarshipStatusDto?> GetPublicStatusAsync(string referenceCode, string email, CancellationToken cancellationToken = default);
    Task<IEnumerable<ScholarshipReviewDto>> GetReviewQueueAsync(CancellationToken cancellationToken = default);
    Task<ScholarshipReviewDto?> GetApplicationForReviewAsync(int applicationId, CancellationToken cancellationToken = default);
    Task<ScholarshipReviewDto?> SubmitReviewAsync(int applicationId, int reviewerMemberId, SubmitScholarshipReviewDto dto, CancellationToken cancellationToken = default);
    Task<ScholarshipFundDto> CreateFundAsync(CreateScholarshipFundDto dto, CancellationToken cancellationToken = default);
    Task<ScholarshipCallDto> CreateCallAsync(CreateScholarshipCallDto dto, CancellationToken cancellationToken = default);
    Task<IEnumerable<ScholarshipApplicationDto>> GetApplicationsForAdminAsync(CancellationToken cancellationToken = default);
    Task<ScholarshipAwardDto> CreateAwardAsync(CreateScholarshipAwardDto dto, CancellationToken cancellationToken = default);
    Task<bool> DisburseAwardAsync(int awardId, int adminId, CancellationToken cancellationToken = default);
}

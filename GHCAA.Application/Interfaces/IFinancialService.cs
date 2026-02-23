using GHCAA.Application.DTOs;
using GHCAA.Domain;

namespace GHCAA.Application.Interfaces
{
    public interface IFinancialService
    {
        Task<IEnumerable<PaymentHistoryDto>> GetMemberPaymentHistoryAsync(int memberId, CancellationToken cancellationToken = default);
        Task<PaymentHistoryDto> RecordPaymentAsync(CreatePaymentHistoryDto dto, CancellationToken cancellationToken = default);
        Task<bool> UpdatePaymentStatusAsync(int paymentId, Enums.PaymentStatus status, string? notes = null, CancellationToken cancellationToken = default);
        
        // Membership History
        Task<IEnumerable<MembershipHistoryDto>> GetMemberMembershipHistoryAsync(int memberId, CancellationToken cancellationToken = default);
        Task RecordMembershipChangeAsync(int memberId, string from, string to, int? adminId = null, string? reason = null, CancellationToken cancellationToken = default);

        // Annual Dues
        Task<IEnumerable<MembershipDueDto>> GetMemberDuesAsync(int memberId, CancellationToken cancellationToken = default);
        Task GenerateAnnualDuesAsync(int year, CancellationToken cancellationToken = default);
        Task<bool> MarkDueAsPaidAsync(int dueId, int paymentHistoryId, CancellationToken cancellationToken = default);

        // Membership Fee Configuration
        Task<IEnumerable<MembershipFeeConfigDto>> GetMembershipFeeConfigsAsync(CancellationToken cancellationToken = default);
        Task<MembershipFeeConfigDto> AddMembershipFeeConfigAsync(CreateMembershipFeeConfigDto dto, int adminMemberId, CancellationToken cancellationToken = default);
        Task<MembershipFeeConfigDto> UpdateMembershipFeeConfigAsync(UpdateMembershipFeeConfigDto dto, int adminMemberId, CancellationToken cancellationToken = default);
        Task<decimal> GetApplicableMembershipFeeAsync(Domain.Enums.MembershipType type, int year, CancellationToken cancellationToken = default);
    }
}

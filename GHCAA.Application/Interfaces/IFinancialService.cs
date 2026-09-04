using GHCAA.Application.DTOs;
using GHCAA.Domain;

namespace GHCAA.Application.Interfaces
{
    public interface IFinancialService
    {
        Task<IEnumerable<PaymentHistoryDto>> GetMemberPaymentHistoryAsync(int memberId, CancellationToken cancellationToken = default);
        Task<PaymentHistoryDto> RecordPaymentAsync(CreatePaymentHistoryDto dto, CancellationToken cancellationToken = default);
        Task<bool> UpdatePaymentStatusAsync(int paymentId, Enums.PaymentStatus status, string? notes = null, CancellationToken cancellationToken = default);
        Task<bool> ProcessGatewayPaymentAsync(string transactionId, decimal confirmedAmount = 0, string gatewayNotes = "", CancellationToken cancellationToken = default);

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
        Task<decimal> GetApplicableFeeAsync(Domain.Enums.FinancialCategory category, Domain.Enums.MembershipType type, DateTime date, CancellationToken cancellationToken = default);
        // 82.16: soft delete, recording the acting admin. A payment row is evidence that money
        // was received, so it is marked deleted rather than removed.
        Task<bool> DeletePaymentAsync(int paymentId, int adminId, CancellationToken cancellationToken = default);
        Task<byte[]> GenerateTaxReceiptAsync(int paymentId, CancellationToken cancellationToken = default);

        // Saved Payment Methods
        Task<IEnumerable<SavedPaymentMethodDto>> GetSavedPaymentMethodsAsync(int memberId, CancellationToken cancellationToken = default);
        Task<SavedPaymentMethodDto> AddSavedPaymentMethodAsync(int memberId, CreateSavedPaymentMethodDto dto, CancellationToken cancellationToken = default);
        Task<bool> DeleteSavedPaymentMethodAsync(int memberId, int id, CancellationToken cancellationToken = default);
    }
}

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Domain;
using GHCAA.Domain.Models;

namespace GHCAA.Application.Interfaces
{
    public interface IFinancialService
    {
        Task<IEnumerable<PaymentHistory>> GetMemberPaymentHistoryAsync(int memberId, CancellationToken cancellationToken = default);
        Task<PaymentHistory> RecordPaymentAsync(PaymentHistory payment, CancellationToken cancellationToken = default);
        Task<bool> UpdatePaymentStatusAsync(int paymentId, Enums.PaymentStatus status, string? notes = null, CancellationToken cancellationToken = default);
        
        // Membership History
        Task<IEnumerable<MembershipHistory>> GetMemberMembershipHistoryAsync(int memberId, CancellationToken cancellationToken = default);
        Task RecordMembershipChangeAsync(int memberId, string from, string to, int? adminId = null, string? reason = null, CancellationToken cancellationToken = default);
    }
}

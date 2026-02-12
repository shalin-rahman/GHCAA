using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Services
{
    public class FinancialService : IFinancialService
    {
        private readonly ApplicationDbContext _db;

        public FinancialService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<PaymentHistory>> GetMemberPaymentHistoryAsync(int memberId, CancellationToken cancellationToken = default)
        {
            return await _db.PaymentHistories
                .Where(p => p.MemberId == memberId)
                .OrderByDescending(p => p.PaidAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<PaymentHistory> RecordPaymentAsync(PaymentHistory payment, CancellationToken cancellationToken = default)
        {
            payment.PaidAt = DateTime.UtcNow;
            await _db.PaymentHistories.AddAsync(payment, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return payment;
        }

        public async Task<bool> UpdatePaymentStatusAsync(int paymentId, Enums.PaymentStatus status, string? notes = null, CancellationToken cancellationToken = default)
        {
            var payment = await _db.PaymentHistories.FindAsync(new object[] { paymentId }, cancellationToken);
            if (payment == null) return false;

            payment.Status = status;
            if (notes != null) payment.Notes = notes;

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<IEnumerable<MembershipHistory>> GetMemberMembershipHistoryAsync(int memberId, CancellationToken cancellationToken = default)
        {
            return await _db.MembershipHistories
                .Where(h => h.MemberId == memberId)
                .OrderByDescending(h => h.ChangedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task RecordMembershipChangeAsync(int memberId, string from, string to, int? adminId = null, string? reason = null, CancellationToken cancellationToken = default)
        {
            var history = new MembershipHistory
            {
                MemberId = memberId,
                ChangedFrom = from,
                ChangedTo = to,
                ChangedByAdminId = adminId,
                Reason = reason,
                ChangedAt = DateTime.UtcNow
            };

            await _db.MembershipHistories.AddAsync(history, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}

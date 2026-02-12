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
        private readonly ICommunicationService _communication;

        public FinancialService(ApplicationDbContext db, ICommunicationService communication)
        {
            _db = db;
            _communication = communication;
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

            // Send Notification
            await _communication.SendIndividualEmailAsync(payment.MemberId, "PAYMENT_RECEIVED", new Dictionary<string, string>
            {
                { "Amount", payment.Amount.ToString("N2") },
                { "TrxID", payment.TransactionId }
            }, cancellationToken);

            return payment;
        }

        public async Task<bool> UpdatePaymentStatusAsync(int paymentId, Enums.PaymentStatus status, string? notes = null, CancellationToken cancellationToken = default)
        {
            var payment = await _db.PaymentHistories.FindAsync(new object[] { paymentId }, cancellationToken);
            if (payment == null) return false;

            payment.Status = status;
            if (notes != null) payment.Notes = notes;

            await _db.SaveChangesAsync(cancellationToken);

            // Send Notification
            await _communication.SendIndividualEmailAsync(payment.MemberId, "PAYMENT_STATUS_UPDATED", new Dictionary<string, string>
            {
                { "Status", status.ToString() },
                { "TrxID", payment.TransactionId }
            }, cancellationToken);

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

        public async Task<IEnumerable<MembershipDue>> GetMemberDuesAsync(int memberId, CancellationToken cancellationToken = default)
        {
            return await _db.MembershipDues
                .Where(d => d.MemberId == memberId)
                .OrderByDescending(d => d.Year)
                .ToListAsync(cancellationToken);
        }

        public async Task GenerateAnnualDuesAsync(int year, CancellationToken cancellationToken = default)
        {
            // Only generate for Active members
            var activeMembers = await _db.Members
                .Where(m => m.Status == Enums.MembershipStatus.Active && !m.IsArchived)
                .ToListAsync(cancellationToken);

            foreach (var member in activeMembers)
            {
                // Check if due already exists for this year
                var exists = await _db.MembershipDues.AnyAsync(d => d.MemberId == member.Id && d.Year == year, cancellationToken);
                if (exists) continue;

                var amount = member.MembershipType switch
                {
                    Enums.MembershipType.Founding => 5000,
                    Enums.MembershipType.Life => 0, // Life members might not have annual dues
                    Enums.MembershipType.Executive => 2000,
                    _ => 1000 // General members
                };

                if (amount == 0) continue;

                _db.MembershipDues.Add(new MembershipDue
                {
                    MemberId = member.Id,
                    Year = year,
                    Amount = amount,
                    DueDate = new DateTime(year, 3, 31), // Default due date Mar 31
                    IsPaid = false
                });
            }

            await _db.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> MarkDueAsPaidAsync(int dueId, int paymentHistoryId, CancellationToken cancellationToken = default)
        {
            var due = await _db.MembershipDues.FindAsync(new object[] { dueId }, cancellationToken);
            if (due == null) return false;

            due.IsPaid = true;
            due.PaymentDate = DateTime.UtcNow;
            due.PaymentHistoryId = paymentHistoryId;

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}

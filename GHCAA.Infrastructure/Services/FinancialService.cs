using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
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
        private readonly INotificationService _notification;

        public FinancialService(ApplicationDbContext db, ICommunicationService communication, INotificationService notification)
        {
            _db = db;
            _communication = communication;
            _notification = notification;
        }

        public async Task<IEnumerable<PaymentHistoryDto>> GetMemberPaymentHistoryAsync(int memberId, CancellationToken cancellationToken = default)
        {
            var history = await _db.PaymentHistories
                .Where(p => p.MemberId == memberId)
                .OrderByDescending(p => p.PaidAt)
                .ToListAsync(cancellationToken);
            
            return history.Select(MapToPaymentDto);
        }

        public async Task<PaymentHistoryDto> RecordPaymentAsync(CreatePaymentHistoryDto dto, CancellationToken cancellationToken = default)
        {
            if (!dto.MemberId.HasValue) 
                throw new ArgumentException("MemberId is required for recording payment.");

            var payment = new PaymentHistory
            {
                MemberId = dto.MemberId.Value,
                TransactionId = dto.TransactionId,
                Amount = dto.Amount,
                PaidAt = DateTime.SpecifyKind(dto.PaidAt, DateTimeKind.Utc),
                Status = Enums.PaymentStatus.Pending,
                Notes = dto.Notes
            };

            await _db.PaymentHistories.AddAsync(payment, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            // Send Notification
            // We use simple fire-and-forget or await? The interface awaits.
            // Using try-catch for notification to not block payment recording if email fails?
            // Existing code awaited it. keeping it consistent.
            try 
            {
                await _communication.SendIndividualEmailAsync(payment.MemberId, "PAYMENT_RECEIVED", new Dictionary<string, string>
                {
                    { "Amount", payment.Amount.ToString("N2") },
                    { "TrxID", payment.TransactionId }
                }, cancellationToken);
            }
            catch 
            {
                // Log warning? For now just continue as payment is recorded.
            }

            // In-app Notification
            await _notification.CreateNotificationAsync(
                payment.MemberId,
                "Payment Recorded",
                $"Your payment of {payment.Amount:N2} (TrxID: {payment.TransactionId}) has been received and is pending verification.",
                "Payment",
                "/portal/payments",
                cancellationToken);

            return MapToPaymentDto(payment);
        }

        public async Task<bool> UpdatePaymentStatusAsync(int paymentId, Enums.PaymentStatus status, string? notes = null, CancellationToken cancellationToken = default)
        {
            var payment = await _db.PaymentHistories.FindAsync(new object[] { paymentId }, cancellationToken);
            if (payment == null) return false;

            payment.Status = status;
            if (notes != null) payment.Notes = notes;

            await _db.SaveChangesAsync(cancellationToken);

            // Send Notification
            try
            {
                await _communication.SendIndividualEmailAsync(payment.MemberId, "PAYMENT_STATUS_UPDATED", new Dictionary<string, string>
                {
                    { "Status", status.ToString() },
                    { "TrxID", payment.TransactionId }
                }, cancellationToken);
            }
            catch
            {
                // Ignore email failure
            }

            // In-app Notification
            await _notification.CreateNotificationAsync(
                payment.MemberId,
                "Payment Status Updated",
                $"The status of your transaction {payment.TransactionId} has been updated to {status}.",
                "Payment",
                "/portal/payments",
                cancellationToken);

            return true;
        }

        public async Task<IEnumerable<MembershipHistoryDto>> GetMemberMembershipHistoryAsync(int memberId, CancellationToken cancellationToken = default)
        {
            var history = await _db.MembershipHistories
                .Where(h => h.MemberId == memberId)
                .OrderByDescending(h => h.ChangedAt)
                .ToListAsync(cancellationToken);

            return history.Select(h => new MembershipHistoryDto
            {
                Id = h.Id,
                MemberId = h.MemberId,
                OldType = h.ChangedFrom,
                NewType = h.ChangedTo,
                ChangeDate = h.ChangedAt,
                Reason = h.Reason,
                ChangedByAdminId = h.ChangedByAdminId
            });
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

        public async Task<IEnumerable<MembershipDueDto>> GetMemberDuesAsync(int memberId, CancellationToken cancellationToken = default)
        {
            var dues = await _db.MembershipDues
                .Where(d => d.MemberId == memberId)
                .OrderByDescending(d => d.Year)
                .ToListAsync(cancellationToken);

            return dues.Select(d => new MembershipDueDto
            {
                Id = d.Id,
                MemberId = d.MemberId,
                Year = d.Year,
                Amount = d.Amount,
                DueDate = d.DueDate,
                IsPaid = d.IsPaid,
                PaidAt = d.PaymentDate
            });
        }

        public async Task<IEnumerable<MembershipFeeConfigDto>> GetMembershipFeeConfigsAsync(CancellationToken cancellationToken = default)
        {
            var configs = await _db.MembershipFeeConfigs
                .OrderBy(c => c.MembershipType)
                .ThenByDescending(c => c.EffectiveDate)
                .ToListAsync(cancellationToken);

            return configs.Select(c => new MembershipFeeConfigDto
            {
                Id = c.Id,
                MembershipType = c.MembershipType.ToString(),
                Amount = c.Amount,
                EffectiveDate = c.EffectiveDate,
                Description = c.Description
            });
        }

        public async Task<MembershipFeeConfigDto> AddMembershipFeeConfigAsync(CreateMembershipFeeConfigDto dto, int adminMemberId, CancellationToken cancellationToken = default)
        {
            if (!Enum.TryParse<Enums.MembershipType>(dto.MembershipType, true, out var type))
            {
                throw new ArgumentException($"Invalid MembershipType: {dto.MembershipType}");
            }

            var config = new MembershipFeeConfig
            {
                MembershipType = type,
                Amount = dto.Amount,
                EffectiveDate = DateTime.SpecifyKind(dto.EffectiveDate, DateTimeKind.Utc),
                Description = dto.Description,
                CreatedByAdminId = adminMemberId,
                CreatedAt = DateTime.UtcNow
            };

            _db.MembershipFeeConfigs.Add(config);
            await _db.SaveChangesAsync(cancellationToken);

            return new MembershipFeeConfigDto
            {
                Id = config.Id,
                MembershipType = config.MembershipType.ToString(),
                Amount = config.Amount,
                EffectiveDate = config.EffectiveDate,
                Description = config.Description
            };
        }

        public async Task<MembershipFeeConfigDto> UpdateMembershipFeeConfigAsync(UpdateMembershipFeeConfigDto dto, int adminMemberId, CancellationToken cancellationToken = default)
        {
            var config = await _db.MembershipFeeConfigs.FindAsync(new object[] { dto.Id }, cancellationToken);
            if (config == null) throw new KeyNotFoundException($"MembershipFeeConfig with ID {dto.Id} not found.");

            config.Amount = dto.Amount;
            config.EffectiveDate = DateTime.SpecifyKind(dto.EffectiveDate, DateTimeKind.Utc);
            config.Description = dto.Description;
            // distinct from "CreatedBy", we might want "UpdatedBy" later, but for now simple update.

            await _db.SaveChangesAsync(cancellationToken);

            return new MembershipFeeConfigDto
            {
                Id = config.Id,
                MembershipType = config.MembershipType.ToString(),
                Amount = config.Amount,
                EffectiveDate = config.EffectiveDate,
                Description = config.Description
            };
        }

        public async Task<decimal> GetApplicableMembershipFeeAsync(Enums.MembershipType type, int year, CancellationToken cancellationToken = default)
        {
            // Logic: Find the latest config that is effective on or before the start of the target year (or end of it? usually start).
            // Let's assume dues for 2024 are based on the fee set before or during 2024.
            // A fee set on Jan 1 2024 is applicable for 2024.
            // A fee set on Dec 31 2023 is applicable for 2024.
            // A fee set on Feb 1 2024 might be applicable for 2025?
            // "Applicable Date" usually means "Any dues generated for a period starting AFTER this date".
            // Let's use: The most recent config where EffectiveDate <= Dec 31 of that year. 
            // Actually simpler: typically fees don't change mid-year. 
            // Let's Find the config with max EffectiveDate where EffectiveDate <= Now (or generation time).
            // But we generate for a specific year.
            
            var targetDate = new DateTime(year, 12, 31, 23, 59, 59, DateTimeKind.Utc); // End of the target year

            var config = await _db.MembershipFeeConfigs
                .Where(c => c.MembershipType == type && c.EffectiveDate <= targetDate)
                .OrderByDescending(c => c.EffectiveDate)
                .FirstOrDefaultAsync(cancellationToken);

            return config?.Amount ?? 0;
        }

        public async Task GenerateAnnualDuesAsync(int year, CancellationToken cancellationToken = default)
        {
            // Only generate for Active members
            var activeMembers = await _db.Members
                .Where(m => m.Status == Enums.MembershipStatus.Active && !m.IsArchived)
                .ToListAsync(cancellationToken);

            // Pre-fetch fees to avoid N+1 queries
            var membershipTypes = Enum.GetValues<Enums.MembershipType>();
            var feeMap = new Dictionary<Enums.MembershipType, decimal>();
            
            foreach (var type in membershipTypes)
            {
                 feeMap[type] = await GetApplicableMembershipFeeAsync(type, year, cancellationToken);
            }

            foreach (var member in activeMembers)
            {
                // Check if due already exists for this year
                var exists = await _db.MembershipDues.AnyAsync(d => d.MemberId == member.Id && d.Year == year, cancellationToken);
                if (exists) continue;

                if (!feeMap.TryGetValue(member.MembershipType, out var amount))
                {
                    amount = 0;
                }

                if (amount == 0) continue;

                var due = new MembershipDue
                {
                    MemberId = member.Id,
                    Year = year,
                    Amount = amount,
                    DueDate = new DateTime(year, 3, 31, 0, 0, 0, DateTimeKind.Utc),
                    IsPaid = false
                };
                
                _db.MembershipDues.Add(due);
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

        private static PaymentHistoryDto MapToPaymentDto(PaymentHistory p)
        {
            return new PaymentHistoryDto
            {
                Id = p.Id,
                MemberId = p.MemberId,
                TransactionId = p.TransactionId,
                Amount = p.Amount,
                PaidAt = p.PaidAt,
                Status = p.Status,
                Notes = p.Notes
            };
        }
    }
}

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
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using QuestPDF.Infrastructure;
using QuestPDF.Previewer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GHCAA.Infrastructure.Services
{
    public class FinancialService : IFinancialService
    {
        private readonly ApplicationDbContext _db;
        private readonly ICommunicationService _communication;
        private readonly INotificationService _notification;
        private readonly IFileStorageService _storage;
        private readonly IRealTimeService _realTime;
        private readonly ILogger<FinancialService> _logger;
        private readonly IConfiguration _config;
        private readonly IUserService _userService;
        private readonly IActivityService _activityService;
        private readonly IGamificationService _gamification;

        public FinancialService(
            ApplicationDbContext db, 
            ICommunicationService communication, 
            INotificationService notification, 
            IFileStorageService storage,
            IRealTimeService realTime,
            ILogger<FinancialService> logger,
            IConfiguration config,
            IUserService userService,
            IActivityService activityService,
            IGamificationService gamification)
        {
            _db = db;
            _communication = communication;
            _notification = notification;
            _storage = storage;
            _realTime = realTime;
            _logger = logger;
            _config = config;
            _userService = userService;
            _activityService = activityService;
            _gamification = gamification;
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
                FinancialCategory = dto.FinancialCategory,
                PaymentMethod = dto.PaymentMethod,
                Notes = dto.Notes
            };

            await _db.PaymentHistories.AddAsync(payment, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            // Handle Receipt Upload if present
            if (dto.Receipt != null)
            {
                using var ms = new MemoryStream();
                await dto.Receipt.CopyToAsync(ms, cancellationToken);
                ms.Position = 0;
                
                var path = await _storage.SaveFileAsync(ms, dto.Receipt.FileName, payment.MemberId, Enums.FileUploadType.PaymentProof, cancellationToken);
                
                payment.ReceiptPath = path;
                
                // Track in FileUploads table too
                var fu = new FileUpload 
                { 
                    MemberId = payment.MemberId, 
                    UploadType = Enums.FileUploadType.PaymentProof, 
                    FileName = dto.Receipt.FileName, 
                    FilePath = path, 
                    SizeBytes = dto.Receipt.Length 
                };
                await _db.FileUploads.AddAsync(fu, cancellationToken);
                
                await _db.SaveChangesAsync(cancellationToken);
            }

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
                Enums.NotificationType.GeneralSystem,
                "/portal/payments",
                cancellationToken);

            return MapToPaymentDto(payment);
        }

        public async Task<bool> UpdatePaymentStatusAsync(int paymentId, Enums.PaymentStatus status, string? notes = null, CancellationToken cancellationToken = default)
        {
            var payment = await _db.PaymentHistories.FindAsync(new object[] { paymentId }, cancellationToken);
            if (payment == null) return false;

            payment.Status = status;
            if (!string.IsNullOrEmpty(notes))
            {
                payment.Notes = string.IsNullOrEmpty(payment.Notes) ? notes : $"{payment.Notes} | {notes}";
            }

            await _db.SaveChangesAsync(cancellationToken);
            
            if (status == Enums.PaymentStatus.Completed)
            {
                await _notification.CreateNotificationAsync(payment.MemberId, "Payment Verified", $"Your payment of {payment.Amount:N2} has been successfully verified.", Enums.NotificationType.GeneralSystem, "/finance/history", cancellationToken);
                
                // Trigger Live Admin Alert (Real-time Audit Trace)
                await _realTime.SendAdminAlertAsync("NEW_PAYMENT", new { 
                    MemberId = payment.MemberId, 
                    Amount = payment.Amount, 
                    TransactionId = payment.TransactionId, 
                    Timestamp = DateTime.UtcNow 
                });
            }

            return true;
        }

        public async Task<bool> ProcessGatewayPaymentAsync(string transactionId, decimal confirmedAmount = 0, string gatewayNotes = "", CancellationToken cancellationToken = default)
        {
            var payment = await _db.PaymentHistories.FirstOrDefaultAsync(p => p.TransactionId == transactionId, cancellationToken);
            if (payment == null || payment.Status == Enums.PaymentStatus.Completed) return false;

            // Security Check
            if (confirmedAmount > 0 && Math.Abs(payment.Amount - confirmedAmount) > 0.01m)
            {
                await UpdatePaymentStatusAsync(payment.Id, Enums.PaymentStatus.Failed, $"Amount mismatch detected. Paid: {confirmedAmount}, Expected: {payment.Amount}. Gateway: {gatewayNotes}", cancellationToken);
                return false;
            }

            // 1. Mark as Completed
            await UpdatePaymentStatusAsync(payment.Id, Enums.PaymentStatus.Completed, gatewayNotes, cancellationToken);

            // 2. Handle Automated Approvals
            await HandleAutomatedApprovalsAfterPaymentAsync(payment, cancellationToken);

            return true;
        }

        private async Task HandleAutomatedApprovalsAfterPaymentAsync(PaymentHistory payment, CancellationToken cancellationToken)
        {
            // Case A: Event Registration
            if (payment.Notes != null && payment.Notes.Contains("EVT-REG-"))
            {
                var regMatch = payment.Notes.Split("EVT-REG-")[1].Split(" ")[0];
                var regRef = "EVT-REG-" + regMatch;
                
                var registration = await _db.EventRegistrations.Include(r => r.Event).FirstOrDefaultAsync(r => r.PaymentReference == regRef, cancellationToken);
                if (registration != null && registration.Status == Enums.EventRegistrationStatus.Pending)
                {
                    var expected = registration.Event?.RegistrationFee ?? registration.ContributionAmount ?? 0;
                    if (payment.Amount >= expected)
                    {
                        var adminIdStr = _config["GeneralSettings:SystemAdminId"] ?? "1";
                        int.TryParse(adminIdStr, out var adminId);
                        
                        registration.Status = Enums.EventRegistrationStatus.Approved;
                        registration.ApprovedAt = DateTime.UtcNow;
                        registration.ApprovedByAdminId = adminId; // System Admin
                        await _db.SaveChangesAsync(cancellationToken);
                        
                        await _notification.CreateNotificationAsync(registration.MemberId ?? 0, "Registration Approved", $"Your registration for {registration.Event?.Title} is now confirmed.", Enums.NotificationType.RegistrationUpdate, "/events", cancellationToken);
                    }
                }
            }

            // Case B: Member Admission Approval
            if (payment.MemberId > 0)
            {
                var member = await _db.Members.FindAsync(new object[] { payment.MemberId }, cancellationToken);
                if (member != null && member.Status == Enums.MembershipStatus.Applied)
                {
                    // Verify if it covers the dues
                    var fee = await GetApplicableMembershipFeeAsync(member.MembershipType, DateTime.UtcNow.Year, cancellationToken);
                    if (payment.Amount >= fee)
                    {
                        // Duplicate logic from MemberService.ApproveMemberAsync to avoid circular dependency
                        await ApproveMemberInternalAsync(member, cancellationToken);
                    }
                }
            }
        }

        private async Task ApproveMemberInternalAsync(Member member, CancellationToken cancellationToken)
        {
             // 1. Membership number generation
            var prefix = $"GHC{DateTime.UtcNow:yyMM}";
            var lastBound = await _db.Members.Where(m => m.MembershipNumber != null && m.MembershipNumber.StartsWith(prefix)).OrderByDescending(m => m.MembershipNumber).FirstOrDefaultAsync(cancellationToken);
            int nextId = (lastBound != null && int.TryParse(lastBound.MembershipNumber?.Substring(prefix.Length), out int lastId)) ? lastId + 1 : 1;
            var membershipNumber = $"{prefix}{nextId:D3}";

            var adminIdStr = _config["GeneralSettings:SystemAdminId"] ?? "1";
            int.TryParse(adminIdStr, out var adminId);

            // 2. Update Member
            member.Status = Enums.MembershipStatus.Active;
            member.IsVerified = true;
            member.MembershipNumber = membershipNumber;
            member.ApprovedDate = DateTime.UtcNow;
            member.ApprovedBy = adminId;

            await _db.SaveChangesAsync(cancellationToken);

            // 3. User account
            var cleanNid = member.NID.Replace(" ", "");
            await _userService.CreateUserAccountAsync(member.Id, cleanNid, cleanNid, cancellationToken);

            // 4. Activity & Gamification
            await _gamification.AwardPointsAsync(member.Id, "PROFILE_VERIFIED", metadata: "System Auto-approval via PaymentGateway", cancellationToken: cancellationToken);
            await _activityService.LogActivityAsync(member.Id, "Approved", $"Auto-approved by System after successful payment. # {membershipNumber}", adminId, cancellationToken: cancellationToken);

            // 5. Notification & Email
            await _communication.SendIndividualEmailAsync(member.Id, "WELCOME_EMAIL", new Dictionary<string, string> { { "DefaultPassword", cleanNid } }, cancellationToken);
            await _notification.CreateNotificationAsync(member.Id, "Welcome to GHCAA!", "Your membership is now active.", Enums.NotificationType.RegistrationUpdate, "/portal/dashboard", cancellationToken);
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
                Category = c.Category,
                MembershipType = c.MembershipType.ToString(),
                Amount = c.Amount,
                EffectiveDate = c.EffectiveDate,
                EffectiveTo = c.EffectiveTo,
                IsActive = c.IsActive,
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
                Category = dto.Category,
                MembershipType = type,
                Amount = dto.Amount,
                EffectiveDate = DateTime.SpecifyKind(dto.EffectiveDate, DateTimeKind.Utc),
                EffectiveTo = dto.EffectiveTo.HasValue ? DateTime.SpecifyKind(dto.EffectiveTo.Value, DateTimeKind.Utc) : null,
                IsActive = dto.IsActive,
                Description = dto.Description,
                CreatedByAdminId = adminMemberId,
                CreatedAt = DateTime.UtcNow
            };

            _db.MembershipFeeConfigs.Add(config);
            await _db.SaveChangesAsync(cancellationToken);

            return new MembershipFeeConfigDto
            {
                Id = config.Id,
                Category = config.Category,
                MembershipType = config.MembershipType.ToString(),
                Amount = config.Amount,
                EffectiveDate = config.EffectiveDate,
                EffectiveTo = config.EffectiveTo,
                IsActive = config.IsActive,
                Description = config.Description
            };
        }

        public async Task<MembershipFeeConfigDto> UpdateMembershipFeeConfigAsync(UpdateMembershipFeeConfigDto dto, int adminMemberId, CancellationToken cancellationToken = default)
        {
            var config = await _db.MembershipFeeConfigs.FindAsync(new object[] { dto.Id }, cancellationToken);
            if (config == null) throw new KeyNotFoundException($"MembershipFeeConfig with ID {dto.Id} not found.");

            if (dto.Category.HasValue) config.Category = dto.Category.Value;
            config.Amount = dto.Amount;
            config.EffectiveDate = DateTime.SpecifyKind(dto.EffectiveDate, DateTimeKind.Utc);
            config.EffectiveTo = dto.EffectiveTo.HasValue ? DateTime.SpecifyKind(dto.EffectiveTo.Value, DateTimeKind.Utc) : null;
            config.IsActive = dto.IsActive;
            config.Description = dto.Description;
            // distinct from "CreatedBy", we might want "UpdatedBy" later, but for now simple update.

            await _db.SaveChangesAsync(cancellationToken);

            return new MembershipFeeConfigDto
            {
                Id = config.Id,
                Category = config.Category,
                MembershipType = config.MembershipType.ToString(),
                Amount = config.Amount,
                EffectiveDate = config.EffectiveDate,
                EffectiveTo = config.EffectiveTo,
                IsActive = config.IsActive,
                Description = config.Description
            };
        }

        public async Task<decimal> GetApplicableMembershipFeeAsync(Enums.MembershipType type, int year, CancellationToken cancellationToken = default)
        {
            return await GetApplicableFeeAsync(Enums.FinancialCategory.MembershipFee, type, new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc), cancellationToken);
        }

        public async Task<decimal> GetApplicableFeeAsync(Enums.FinancialCategory category, Enums.MembershipType type, DateTime date, CancellationToken cancellationToken = default)
        {
            // The most recent config where EffectiveDate <= target date AND (EffectiveTo == null OR EffectiveTo >= target date) AND IsActive == true
            var config = await _db.MembershipFeeConfigs
                .Where(c => c.IsActive && c.Category == category && c.MembershipType == type && c.EffectiveDate <= date && (c.EffectiveTo == null || c.EffectiveTo >= date))
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

        public async Task<bool> DeletePaymentAsync(int paymentId, CancellationToken cancellationToken = default)
        {
            var payment = await _db.PaymentHistories.FindAsync(new object[] { paymentId }, cancellationToken);
            if (payment == null) return false;

            // Find any dues linked to this payment and reset them
            var linkedDues = await _db.MembershipDues
                .Where(d => d.PaymentHistoryId == paymentId)
                .ToListAsync(cancellationToken);

            foreach (var due in linkedDues)
            {
                due.IsPaid = false;
                due.PaymentDate = null;
                due.PaymentHistoryId = null;
            }

            _db.PaymentHistories.Remove(payment);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<byte[]> GenerateTaxReceiptAsync(int paymentId, CancellationToken cancellationToken = default)
        {
            var payment = await _db.PaymentHistories
                .Include(p => p.Member)
                .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);

            if (payment == null) throw new KeyNotFoundException("Payment record not found.");

            // Create PDF using QuestPDF
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(1, Unit.Inch);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(11));

                    page.Header().Row(row =>
                    {
                        row.RelativeItem().Column(col =>
                        {
                            col.Item().Text("PAYMENT RECEIPT").FontSize(24).Bold().FontColor(Colors.Blue.Medium);
                            col.Item().Text($"{Constants.Branding.AppName}").FontSize(14).Bold();
                        });

                        row.RelativeItem().AlignRight().Column(col =>
                        {
                            col.Item().Text($"Receipt #: {payment.Id:D6}");
                            col.Item().Text($"Date: {payment.PaidAt:dd MMM yyyy}");
                        });
                    });

                    page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
                    {
                        col.Item().BorderBottom(1).PaddingBottom(5).Text("Member Information").Bold();
                        col.Item().PaddingTop(5).Row(row =>
                        {
                            row.RelativeItem().Text("Name:");
                            row.RelativeItem().Text(payment.Member.FullName);
                        });
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Text("Membership ID:");
                            row.RelativeItem().Text(payment.Member.MembershipNumber ?? "Pending");
                        });

                        col.Item().PaddingVertical(20).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(30);
                                columns.RelativeColumn();
                                columns.ConstantColumn(100);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("#");
                                header.Cell().Text("Description");
                                header.Cell().AlignRight().Text("Amount (BDT)");
                                header.Cell().Element(Block).PaddingBottom(5).BorderBottom(1);
                            });

                            table.Cell().Text("1");
                            table.Cell().Text($"{payment.FinancialCategory} - TrxID: {payment.TransactionId}");
                            table.Cell().AlignRight().Text($"{payment.Amount:N2}");
                        });

                        col.Item().AlignRight().PaddingRight(5).Text($"Total: {payment.Amount:N2} BDT").FontSize(14).Bold();

                        col.Item().PaddingTop(50).Text("Note: This is an automatically generated receipt and does not require a signature.").FontSize(10).Italic().FontColor(Colors.Grey.Medium);
                    });

                    page.Footer().AlignCenter().Text(x =>
                    {
                        x.Span("Page ");
                        x.CurrentPageNumber();
                    });
                });
            });

            using var stream = new MemoryStream();
            document.GeneratePdf(stream);
            return stream.ToArray();
        }

        static IContainer Block(IContainer container)
        {
            return container
                .Border(1)
                .Background(Colors.Grey.Lighten3)
                .ShowOnce()
                .MinWidth(50)
                .MinHeight(50)
                .AlignCenter()
                .AlignMiddle();
        }

        // Saved Payment Methods
        public async Task<IEnumerable<SavedPaymentMethodDto>> GetSavedPaymentMethodsAsync(int memberId, CancellationToken cancellationToken = default)
        {
            var methods = await _db.SavedPaymentMethods
                .Where(s => s.MemberId == memberId)
                .OrderByDescending(s => s.LastUsedAt)
                .ToListAsync(cancellationToken);

            return methods.Select(s => new SavedPaymentMethodDto
            {
                Id = s.Id,
                DisplayName = s.DisplayName,
                Method = s.Method,
                AccountNumber = s.AccountNumber,
                Icon = s.Icon,
                IsDefault = s.IsDefault
            });
        }

        public async Task<SavedPaymentMethodDto> AddSavedPaymentMethodAsync(int memberId, CreateSavedPaymentMethodDto dto, CancellationToken cancellationToken = default)
        {
            var method = new SavedPaymentMethod
            {
                MemberId = memberId,
                DisplayName = dto.DisplayName,
                Method = dto.Method,
                AccountNumber = dto.AccountNumber,
                CreatedAt = DateTime.UtcNow,
                LastUsedAt = DateTime.UtcNow
            };

            await _db.SavedPaymentMethods.AddAsync(method, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            return new SavedPaymentMethodDto
            {
                Id = method.Id,
                DisplayName = method.DisplayName,
                Method = method.Method,
                AccountNumber = method.AccountNumber,
                Icon = method.Icon,
                IsDefault = method.IsDefault
            };
        }

        public async Task<bool> DeleteSavedPaymentMethodAsync(int memberId, int id, CancellationToken cancellationToken = default)
        {
            var method = await _db.SavedPaymentMethods
                .FirstOrDefaultAsync(s => s.Id == id && s.MemberId == memberId, cancellationToken);
            
            if (method == null) return false;

            _db.SavedPaymentMethods.Remove(method);
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
                FinancialCategory = p.FinancialCategory,
                PaymentMethod = p.PaymentMethod,
                Notes = p.Notes
            };
        }
    }
}

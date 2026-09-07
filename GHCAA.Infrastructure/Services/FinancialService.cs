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
using QuestPDF.Previewer;
using Microsoft.Extensions.Options;
using GHCAA.Infrastructure.Options;
using Microsoft.Extensions.DependencyInjection;
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
        private readonly IOptions<GeneralSettingsOptions> _generalSettings;
        private readonly IUserService _userService;
        private readonly IActivityService _activityService;
        private readonly IGamificationService _gamification;
        private readonly IOrgConfigService _orgConfigService;
        private readonly IServiceProvider _serviceProvider;

        public FinancialService(
            ApplicationDbContext db,
            ICommunicationService communication,
            INotificationService notification,
            IFileStorageService storage,
            IRealTimeService realTime,
            ILogger<FinancialService> logger,
            IOptions<GeneralSettingsOptions> generalSettings,
            IUserService userService,
            IActivityService activityService,
            IGamificationService gamification,
            IOrgConfigService orgConfigService,
            IServiceProvider serviceProvider)
        {
            _db = db;
            _communication = communication;
            _notification = notification;
            _storage = storage;
            _realTime = realTime;
            _logger = logger;
            _generalSettings = generalSettings;
            _userService = userService;
            _activityService = activityService;
            _gamification = gamification;
            _orgConfigService = orgConfigService;
            _serviceProvider = serviceProvider;
        }

        public async Task<IEnumerable<PaymentHistoryDto>> GetMemberPaymentHistoryAsync(int memberId, CancellationToken cancellationToken = default)
        {
            var history = await _db.PaymentHistories
                .AsNoTracking()
                .Where(p => p.MemberId == memberId)
                .OrderByDescending(p => p.PaidAt)
                .ToListAsync(cancellationToken);

            return history.Select(MapToPaymentDto);
        }

        public async Task<PaymentHistoryDto> RecordPaymentAsync(CreatePaymentHistoryDto dto, CancellationToken cancellationToken = default)
        {
            var payment = new PaymentHistory
            {
                MemberId = dto.MemberId,
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

            // 82.32: a guest (no MemberId — an event that AllowNonMembers) has no member folder to
            // store a receipt against and no member record to email or notify. IFileStorageService,
            // the email templates and in-app notifications are all keyed by a real member id, so
            // none of that is attempted for a guest payment; it stays recorded, just without those
            // side effects. Capturing guest contact details for a receipt/notification path is
            // separate feature work, not part of stopping the crash this guarded.
            if (payment.MemberId.HasValue)
            {
                var memberId = payment.MemberId.Value;

                // Handle Receipt Upload if present
                if (dto.Receipt != null)
                {
                    using var ms = new MemoryStream();
                    await dto.Receipt.CopyToAsync(ms, cancellationToken);
                    ms.Position = 0;

                    var path = await _storage.SaveFileAsync(ms, dto.Receipt.FileName, memberId, Enums.FileUploadType.PaymentProof, cancellationToken);

                    payment.ReceiptPath = path;

                    // Track in FileUploads table too
                    var fu = new FileUpload
                    {
                        MemberId = memberId,
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
                    await _communication.SendIndividualEmailAsync(memberId, Constants.TemplateCodes.PaymentReceived, new Dictionary<string, string>
                    {
                        { "Amount", payment.Amount.ToString("N2") },
                        { "TrxID", payment.TransactionId }
                    }, cancellationToken);
                }
                catch
                {
                    // Log warning? For now just continue as payment is recorded.
                }

                // In-app notification: same PAYMENT_RECEIVED template the email above just used,
                // so an admin editing that template changes both channels at once (82.21).
                await _notification.CreateNotificationFromTemplateAsync(
                    memberId,
                    Constants.TemplateCodes.PaymentReceived,
                    Enums.NotificationType.GeneralSystem,
                    fallbackTitle: "Payment Recorded",
                    fallbackMessage: $"Your payment of {payment.Amount:N2} (TrxID: {payment.TransactionId}) has been received and is pending verification.",
                    templateVars: new Dictionary<string, string>
                    {
                        { "Amount", payment.Amount.ToString("N2") },
                        { "TrxID", payment.TransactionId }
                    },
                    targetUrl: "/portal/payments",
                    cancellationToken: cancellationToken);
            }

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
                // 82.32: a guest payment (MemberId null) has nobody to notify — the admin alert
                // below still fires either way, since that one is not member-scoped.
                if (payment.MemberId.HasValue)
                {
                    // PAYMENT_STATUS_UPDATED already exists as a seeded template with no live email
                    // caller; routing this notification through it (82.21) means it stops being dead data.
                    await _notification.CreateNotificationFromTemplateAsync(
                        payment.MemberId.Value,
                        Constants.TemplateCodes.PaymentStatusUpdated,
                        Enums.NotificationType.GeneralSystem,
                        fallbackTitle: "Payment Verified",
                        fallbackMessage: $"Your payment of {payment.Amount:N2} has been successfully verified.",
                        templateVars: new Dictionary<string, string>
                        {
                            { "TrxID", payment.TransactionId },
                            { "Status", status.ToString() }
                        },
                        targetUrl: "/finance/history",
                        cancellationToken: cancellationToken);
                }

                // Trigger Live Admin Alert (Real-time Audit Trace)
                await _realTime.SendAdminAlertAsync("NEW_PAYMENT", new
                {
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
                        var adminId = _generalSettings.Value.SystemAdminId;

                        registration.Status = Enums.EventRegistrationStatus.Approved;
                        registration.ApprovedAt = DateTime.UtcNow;
                        registration.ApprovedByAdminId = adminId; // System Admin
                        await _db.SaveChangesAsync(cancellationToken);

                        await _notification.CreateNotificationAsync(registration.MemberId ?? 0, "Registration Approved", $"Your registration for {registration.Event?.Title} is now confirmed.", Enums.NotificationType.RegistrationUpdate, "/events", cancellationToken);
                    }
                }
            }

            // Case B: Member Admission Approval.
            // 82.32: this path had drifted from the callback in GatewaysController it duplicates —
            // it ran for ANY payment category (an event fee could auto-induct an Applied member,
            // the exact bug 29B.3 closed on the gateway path) and treated "no fee config found" as
            // fee zero rather than refusing, so any payment amount would clear it. Brought in line
            // with both guards. This method currently has no production caller (ProcessGatewayPaymentAsync
            // is exercised only by tests), so neither defect was live, but a fix here is cheap and the
            // next caller should not inherit either gap.
            if (payment.MemberId is int payerMemberId && payerMemberId > 0
                && payment.FinancialCategory == Enums.FinancialCategory.MembershipFee)
            {
                var member = await _db.Members.FindAsync(new object[] { payerMemberId }, cancellationToken);
                if (member != null && member.Status == Enums.MembershipStatus.Applied)
                {
                    // Verify if it covers the dues
                    var feeConfig = await _db.MembershipFeeConfigs
                        .AsNoTracking()
                        .Where(c => c.IsActive && c.Category == Enums.FinancialCategory.MembershipFee
                            && c.MembershipType == member.MembershipType && c.EffectiveDate <= DateTime.UtcNow
                            && (c.EffectiveTo == null || c.EffectiveTo >= DateTime.UtcNow))
                        .OrderByDescending(c => c.EffectiveDate)
                        .FirstOrDefaultAsync(cancellationToken);

                    if (feeConfig == null)
                    {
                        _logger.LogError("Auto-approval skipped for Member {Id}: no fee config found for type {Type}", member.Id, member.MembershipType);
                    }
                    else if (payment.Amount >= feeConfig.Amount)
                    {
                        // 29C.1: Delegate to the single canonical approval path in MemberService
                        // (Serializable transaction, Id-ordered serial generation, profile/status/
                        // payment validation, user-account creation inside the transaction) instead
                        // of a divergent inline copy. MemberService depends on IFinancialService, so
                        // resolve it lazily to avoid a constructor DI cycle. If approval fails
                        // (member ineligible / transient error), log and leave the member Applied for
                        // manual admin review rather than half-approving.
                        var adminId = _generalSettings.Value.SystemAdminId;
                        try
                        {
                            var memberService = _serviceProvider.GetRequiredService<IMemberService>();
                            await memberService.ApproveMemberAsync(member.Id, adminId, cancellationToken);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Auto-approval after payment failed for member {MemberId}; left as Applied for manual review.", member.Id);
                        }
                    }
                }
            }
        }

        public async Task<IEnumerable<MembershipHistoryDto>> GetMemberMembershipHistoryAsync(int memberId, CancellationToken cancellationToken = default)
        {
            var history = await _db.MembershipHistories
                .AsNoTracking()
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
                .AsNoTracking()
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
                .AsNoTracking()
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
                .AsNoTracking()
                .Where(c => c.IsActive && c.Category == category && c.MembershipType == type && c.EffectiveDate <= date && (c.EffectiveTo == null || c.EffectiveTo >= date))
                .OrderByDescending(c => c.EffectiveDate)
                .FirstOrDefaultAsync(cancellationToken);

            return config?.Amount ?? 0;
        }

        public async Task GenerateAnnualDuesAsync(int year, CancellationToken cancellationToken = default)
        {
            // Only generate for Active members
            var activeMembers = await _db.Members
                .AsNoTracking()
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

        public async Task<bool> DeletePaymentAsync(int paymentId, int adminId, CancellationToken cancellationToken = default)
        {
            var payment = await _db.PaymentHistories.FindAsync(new object[] { paymentId }, cancellationToken);
            if (payment == null || payment.IsArchived) return false;

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

            // 82.16: soft delete, replacing _db.PaymentHistories.Remove(payment). Unlinking the
            // dues above already reverses the payment's effect, so the row itself has no work left
            // to do except be evidence that it happened — which is exactly the reason not to
            // destroy it. PaymentHistoryConfiguration's query filter keeps it out of ordinary reads.
            payment.IsArchived = true;
            payment.DeletedAt = DateTime.UtcNow;
            payment.DeletedByAdminId = adminId;

            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<byte[]> GenerateTaxReceiptAsync(int paymentId, CancellationToken cancellationToken = default)
        {
            var payment = await _db.PaymentHistories
                .AsNoTracking()
                .Include(p => p.Member)
                .FirstOrDefaultAsync(p => p.Id == paymentId, cancellationToken);

            if (payment == null) throw new KeyNotFoundException("Payment record not found.");
            if (payment.Member == null) throw new InvalidOperationException("Payment has no associated member.");

            var config = await _orgConfigService.GetConfigAsync();

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
                            col.Item().Text($"{config.Branding.ShortName}").FontSize(14).Bold();
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
                .AsNoTracking()
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

        public async Task<int?> GetPaymentOwnerMemberIdAsync(int paymentId, CancellationToken cancellationToken = default)
        {
            var payment = await _db.PaymentHistories.FindAsync(new object[] { paymentId }, cancellationToken);
            return payment?.MemberId;
        }

        public async Task<int?> GetMemberIdForUserAsync(int userId, CancellationToken cancellationToken = default)
        {
            var user = await _db.Users.FindAsync(new object[] { userId }, cancellationToken);
            return user?.MemberId;
        }

        public Task<bool> IsGatewayPaymentAlreadyProcessedAsync(string gatewayPaymentId, CancellationToken cancellationToken = default)
            => _db.PaymentHistories.AnyAsync(p => p.GatewayPaymentId == gatewayPaymentId && p.Status == Enums.PaymentStatus.Completed, cancellationToken);

        public async Task<PaymentHistoryDto?> GetPaymentSnapshotByTransactionIdAsync(string transactionId, CancellationToken cancellationToken = default)
        {
            var payment = await _db.PaymentHistories.AsNoTracking().FirstOrDefaultAsync(p => p.TransactionId == transactionId, cancellationToken);
            if (payment == null) return null;
            return new PaymentHistoryDto
            {
                Id = payment.Id,
                MemberId = payment.MemberId,
                TransactionId = payment.TransactionId,
                Amount = payment.Amount,
                PaidAt = payment.PaidAt,
                Status = payment.Status,
                FinancialCategory = payment.FinancialCategory,
                PaymentMethod = payment.PaymentMethod,
                Notes = payment.Notes
            };
        }

        public async Task StampGatewayPaymentIdAsync(int paymentId, string gatewayPaymentId, CancellationToken cancellationToken = default)
        {
            var payment = await _db.PaymentHistories.FindAsync(new object[] { paymentId }, cancellationToken);
            if (payment == null) return;
            payment.GatewayPaymentId = gatewayPaymentId;
            await _db.SaveChangesAsync(cancellationToken);
        }
    }
}

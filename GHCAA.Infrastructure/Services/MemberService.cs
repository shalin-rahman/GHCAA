using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using GHCAA.Infrastructure.Options;
using Microsoft.Extensions.Logging;

namespace GHCAA.Infrastructure.Services
{
    // 82.6: split across MemberService_Approval.cs (approval/reject/archive/restore lifecycle),
    // MemberService_Profile.cs (profile reads/writes and uploads), MemberService_Search.cs
    // (registry list/search/admin update/dashboard stats), MemberService_Helpers.cs (private
    // gamification/profile-completion helpers) and the pre-existing MemberService_Sync.cs.
    // IMemberService's shape and every call site are unchanged — this file keeps the
    // constructor/fields and the registration + email-verification flow only.
    public partial class MemberService : IMemberService
    {
        private readonly ApplicationDbContext _db;
        private readonly IFileStorageService _storage;

        private readonly IOtpService _otp;
        private readonly IEmailService _email;
        private readonly IUserService _userService;
        private readonly ICommunicationService _communicationService;
        private readonly ILogger<MemberService> _logger;
        private readonly IActivityService _activityService;
        private readonly INotificationService _notificationService;
        private readonly IOptions<AppSettingsOptions> _appSettings;
        private readonly IGamificationService _gamification;
        private readonly IFinancialService _financialService;
        private readonly IRealTimeService _realTimeService;
        private readonly IOrgConfigService _orgConfigService;
        private readonly ITokenService _tokenService;

        public MemberService(
            ApplicationDbContext db,
            IFileStorageService storage,

            IOtpService otp,
            IEmailService email,
            IUserService userService,
            ICommunicationService communicationService,
            ILogger<MemberService> logger,
            IActivityService activityService,
            INotificationService notificationService,
            IOptions<AppSettingsOptions> appSettings,
            IGamificationService gamification,
            IFinancialService financialService,
            IRealTimeService realTimeService,
            IOrgConfigService orgConfigService,
            ITokenService tokenService)
        {
            _db = db;
            _storage = storage;

            _otp = otp;
            _email = email;
            _userService = userService;
            _communicationService = communicationService;
            _logger = logger;
            _activityService = activityService;
            _notificationService = notificationService;
            _appSettings = appSettings;
            _gamification = gamification;
            _financialService = financialService;
            _realTimeService = realTimeService;
            _orgConfigService = orgConfigService;
            _tokenService = tokenService;
        }

        public async Task<int> RegisterAsync(MemberRegistrationDto dto, UploadedFileDto? photo, UploadedFileDto? certificate, UploadedFileDto? paymentProof, CancellationToken cancellationToken = default)
        {
            using var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
            try
            {
                // Prevent duplicates by NID/Email/Mobile
                if (await _db.Members.AnyAsync(m => m.Email == dto.Email || m.NID == dto.NID || m.MobileNo == dto.MobileNo, cancellationToken))
                    throw new InvalidOperationException("Member with same Email, NID, or Mobile already exists.");

                // 35.5: MembershipType is admin-assigned only. Whatever tier the client submits is
                // ignored — otherwise a self-registering applicant could grant themselves Founding or
                // Executive. Every application starts on the org config's default tier; an admin moves
                // it afterwards through UpdateMemberByAdminAsync, which audits the change.
                var orgConfig = await _orgConfigService.GetConfigAsync();
                var assignedType = Enum.TryParse<Enums.MembershipType>(orgConfig?.Workflow?.DefaultMembershipType, true, out var defaultType)
                    ? defaultType
                    : Enums.MembershipType.General;

                var member = new Member
                {
                    FullName = dto.FullName,
                    FatherName = dto.FatherName,
                    MotherName = dto.MotherName,
                    DateOfBirth = DateTime.SpecifyKind(dto.DateOfBirth, DateTimeKind.Utc),
                    Gender = dto.Gender,
                    BloodGroup = dto.BloodGroup,
                    NID = dto.NID.Replace(" ", ""),
                    MobileNo = dto.MobileNo.Replace(" ", ""),
                    Email = dto.Email.Trim().ToLower(),
                    PresentAddress = dto.PresentAddress,
                    PermanentAddress = dto.PermanentAddress,
                    EmergencyContactName = dto.EmergencyContactName,
                    EmergencyContactRelation = dto.EmergencyContactRelation,
                    EmergencyContactPhone = dto.EmergencyContactPhone,
                    TShirtSize = dto.TShirtSize,
                    Status = Enums.MembershipStatus.Applied,
                    AppliedDate = DateTime.UtcNow,
                    EmailVerified = false,
                    IsMobilePublic = dto.IsMobilePublic,
                    IsEmailPublic = dto.IsEmailPublic,
                    IsAddressPublic = dto.IsAddressPublic,
                    IsNIDPublic = dto.IsNIDPublic,
                    NotifyEventCreation = dto.NotifyEventCreation,
                    NotifyParticipationApproval = dto.NotifyParticipationApproval,
                    NotifyRegistrationUpdate = dto.NotifyRegistrationUpdate,
                    NotifyRelevantUpdates = dto.NotifyRelevantUpdates,
                    NotifyCommitteeChanges = dto.NotifyCommitteeChanges,
                    HasAcceptedTerms = dto.HasAcceptedTerms,
                    HasAcceptedGdpr = dto.HasAcceptedGdpr,
                    GdprAcceptedAt = dto.HasAcceptedGdpr ? DateTime.UtcNow : null,
                    MembershipType = assignedType, // 35.5: never dto.MembershipType — admin-assigned only
                    Category = dto.Category,
                    IsVerified = false
                };

                // Registry Validation: Only Founding members can be Lifelong Patrons
                if (member.Category == Enums.MemberCategory.LifelongPatron && member.MembershipType != Enums.MembershipType.Founding)
                {
                    throw new InvalidOperationException("Lifelong Patron status is only available for Founding Membership tier.");
                }

                // Generate Membership Number: GHC + YY + MM + (last 3 digit max + 1)
                var now = DateTime.UtcNow;
                var prefix = $"GHC{now:yyMM}";

                // 24.29: Order by Id (insertion order) to avoid lexicographic rollover at 999→1000.
                var lastMember = await _db.Members
                    .Where(m => m.MembershipNumber != null && m.MembershipNumber.StartsWith(prefix))
                    .OrderByDescending(m => m.Id)
                    .FirstOrDefaultAsync(cancellationToken);

                int nextId = 1;
                if (lastMember?.MembershipNumber != null && lastMember.MembershipNumber.Length > prefix.Length)
                {
                    var lastPart = lastMember.MembershipNumber[prefix.Length..];
                    if (int.TryParse(lastPart, out int lastId))
                        nextId = lastId + 1;
                }

                member.MembershipNumber = $"{prefix}{nextId:D3}";

                // Handle Academic History
                if (dto.AcademicHistory != null && dto.AcademicHistory.Any())
                {
                    // Validation: At least one must be from Govt. Haraganga College
                    if (!dto.AcademicHistory.Any(a => a.IsGHC || a.InstitutionName.Contains("Haraganga", StringComparison.OrdinalIgnoreCase)))
                    {
                        throw new InvalidOperationException("At least one academic record must be from Govt. Haraganga College.");
                    }

                    foreach (var a in dto.AcademicHistory)
                    {
                        member.AcademicHistory.Add(new AcademicRecord
                        {
                            InstitutionName = a.InstitutionName,
                            Degree = a.Degree,
                            Subject = a.Subject,
                            AdmissionYear = a.AdmissionYear,
                            PassingYear = a.PassingYear ?? 0,
                            IsGHC = a.IsGHC || a.InstitutionName.Contains("Haraganga", StringComparison.OrdinalIgnoreCase),
                            Result = a.Result
                        });
                    }
                }
                else
                {
                    throw new InvalidOperationException("Academic history is required. At least one record must be from Govt. Haraganga College.");
                }

                // Handle Professional History
                if (dto.ProfessionalHistory != null)
                {
                    foreach (var p in dto.ProfessionalHistory)
                    {
                        member.ProfessionalHistory.Add(new ProfessionalRecord
                        {
                            OrganizationName = p.OrganizationName,
                            Designation = p.Designation,
                            Sector = p.Sector,
                            Location = p.Location,
                            StartDate = DateTime.SpecifyKind(p.StartDate, DateTimeKind.Utc),
                            EndDate = p.EndDate.HasValue ? DateTime.SpecifyKind(p.EndDate.Value, DateTimeKind.Utc) : null,
                            IsCurrent = p.IsCurrent
                        });
                    }
                }

                // Use synchronous Add to avoid missing extension methods in certain EF versions
                _db.Members.Add(member);
                await _db.SaveChangesAsync(cancellationToken);

                // Handle Payment History (Refined)
                if (dto.PaymentMethodId > 0)
                {
                    var payConfig = await _db.PaymentConfigurations.FindAsync(new object[] { dto.PaymentMethodId }, cancellationToken);
                    if (payConfig != null)
                    {
                        // Fetch dynamic fee config
                        var applicableFee = await _financialService.GetApplicableFeeAsync(
                            Enums.FinancialCategory.RegistrationFee,
                            assignedType, // 35.5: fee must match the tier actually assigned, not the requested one
                            DateTime.UtcNow,
                            cancellationToken);

                        var payment = new PaymentHistory
                        {
                            MemberId = member.Id,
                            TransactionId = dto.TransactionId ?? "REG-" + Guid.NewGuid().ToString("N").Substring(0, 8),
                            Amount = applicableFee,
                            PaidAt = DateTime.UtcNow,
                            Status = Enums.PaymentStatus.Pending,
                            FinancialCategory = Enums.FinancialCategory.RegistrationFee,
                            PaymentMethod = payConfig.Method,
                            Notes = $"Registration payment via {payConfig.DisplayName}"
                        };
                        _db.PaymentHistories.Add(payment);
                        await _db.SaveChangesAsync(cancellationToken);

                        // Link paymentProof to this history record if it exists
                        if (paymentProof != null)
                        {
                            var path = await _storage.SaveFileAsync(paymentProof.Content, paymentProof.FileName, member.Id, Enums.FileUploadType.PaymentProof, cancellationToken);
                            var fu = new FileUpload { MemberId = member.Id, UploadType = Enums.FileUploadType.PaymentProof, FileName = paymentProof.FileName, FilePath = path, SizeBytes = paymentProof.Length };
                            await _db.FileUploads.AddAsync(fu, cancellationToken);
                            payment.ReceiptPath = path;
                            await _db.SaveChangesAsync(cancellationToken);
                        }
                    }
                }

                // Save files if present (use UploadedFileDto.Content stream)
                if (photo != null)
                {
                    var path = await _storage.SaveFileAsync(photo.Content, photo.FileName, member.Id, Enums.FileUploadType.Photo, cancellationToken);
                    var fu = new FileUpload { MemberId = member.Id, UploadType = Enums.FileUploadType.Photo, FileName = photo.FileName, FilePath = path, SizeBytes = photo.Length };
                    await _db.FileUploads.AddAsync(fu, cancellationToken);
                    member.PhotoPath = fu.FilePath;
                }

                if (certificate != null)
                {
                    var path = await _storage.SaveFileAsync(certificate.Content, certificate.FileName, member.Id, Enums.FileUploadType.Certificate, cancellationToken);
                    var fu = new FileUpload { MemberId = member.Id, UploadType = Enums.FileUploadType.Certificate, FileName = certificate.FileName, FilePath = path, SizeBytes = certificate.Length };
                    await _db.FileUploads.AddAsync(fu, cancellationToken);
                }



                // update member with file paths
                _db.Members.Update(member);
                await _db.SaveChangesAsync(cancellationToken);

                // Generate & send OTP
                await _otp.GenerateAndSendOtpAsync(member.Email ?? string.Empty, cancellationToken: cancellationToken);

                await _activityService.LogActivityAsync(member.Id, "Registration", "New registry filing submitted for review.", member.Id, cancellationToken: cancellationToken);

                _logger.LogInformation("Registered application for MemberId {MemberId}", member.Id);

                // Trigger Live Admin Alert
                await _realTimeService.SendAdminAlertAsync("NEW_REGISTRATION", new
                {
                    MemberId = member.Id,
                    Name = member.FullName,
                    MembershipType = member.MembershipType.ToString(),
                    Timestamp = DateTime.UtcNow
                });

                await transaction.CommitAsync(cancellationToken);
                return member.Id;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        }

        public async Task<MemberRegistrationResultDto> GetStatusAsync(int memberId, string email, CancellationToken cancellationToken = default)
        {
            var m = await _db.Members.FirstOrDefaultAsync(mm => mm.Id == memberId, cancellationToken);
            if (m == null || !string.Equals(m.Email, email, StringComparison.OrdinalIgnoreCase))
                throw new KeyNotFoundException("Member not found");
            return new MemberRegistrationResultDto
            {
                MemberId = m.Id,
                Message = $"Status: {m.Status}",
                EmailSent = !string.IsNullOrEmpty(m.Email)
            };
        }

        public async Task<bool> VerifyEmailAsync(string email, string otpCode, CancellationToken cancellationToken = default)
        {
            // Verify OTP
            var isValid = await _otp.VerifyOtpAsync(email, otpCode, cancellationToken: cancellationToken);
            if (!isValid)
            {
                _logger.LogWarning("Invalid OTP attempt for email {Email}", email);
                return false;
            }

            // Update member's email verification status
            var member = await _db.Members.FirstOrDefaultAsync(m => m.Email == email, cancellationToken);
            if (member == null)
            {
                _logger.LogWarning("Member not found for email {Email}", email);
                return false;
            }

            member.EmailVerified = true;
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Email verified for MemberId {MemberId}", member.Id);
            return true;
        }

        public async Task<bool> ResendOtpAsync(string email, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members.FirstOrDefaultAsync(m => m.Email == email, cancellationToken);
            if (member == null)
            {
                _logger.LogWarning("Resend OTP failed: Member not found for email {Email}", email);
                return false;
            }

            if (member.EmailVerified)
            {
                _logger.LogWarning("Resend OTP failed: Email {Email} is already verified", email);
                return false;
            }

            await _otp.GenerateAndSendOtpAsync(email, cancellationToken: cancellationToken);
            _logger.LogInformation("OTP resent to email {Email}", email);
            return true;
        }
    }
}

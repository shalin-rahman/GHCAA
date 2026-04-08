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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace GHCAA.Infrastructure.Services
{
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
        private readonly IConfiguration _config;
        private readonly IGamificationService _gamification;
        private readonly IFinancialService _financialService;
        private readonly IRealTimeService _realTimeService;

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
            IConfiguration config,
            IGamificationService gamification,
            IFinancialService financialService,
            IRealTimeService realTimeService)
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
            _config = config;
            _gamification = gamification;
            _financialService = financialService;
            _realTimeService = realTimeService;
        }

        public async Task<int> RegisterAsync(MemberRegistrationDto dto, UploadedFileDto? photo, UploadedFileDto? certificate, UploadedFileDto? paymentProof, CancellationToken cancellationToken = default)
        {
            using var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken);
            try
            {
            // Prevent duplicates by NID/Email/Mobile
            if (await _db.Members.AnyAsync(m => m.Email == dto.Email || m.NID == dto.NID || m.MobileNo == dto.MobileNo, cancellationToken))
                throw new InvalidOperationException("Member with same Email, NID, or Mobile already exists.");

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
                HasAcceptedTerms = dto.HasAcceptedTerms,
                HasAcceptedGdpr = dto.HasAcceptedGdpr,
                GdprAcceptedAt = dto.HasAcceptedGdpr ? DateTime.UtcNow : null,
                MembershipType = dto.MembershipType,
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
            
            // Get the last membership number for the current month prefix
            var lastMember = await _db.Members
                .Where(m => m.MembershipNumber != null && m.MembershipNumber.StartsWith(prefix))
                .OrderByDescending(m => m.MembershipNumber)
                .FirstOrDefaultAsync(cancellationToken);
                
            int nextId = 1;
            if (lastMember != null && lastMember.MembershipNumber != null && lastMember.MembershipNumber.Length >= prefix.Length + 1)
            {
                var lastPart = lastMember.MembershipNumber.Substring(prefix.Length);
                if (int.TryParse(lastPart, out int lastId))
                {
                    nextId = lastId + 1;
                }
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
                        dto.MembershipType, 
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
            await _otp.GenerateAndSendOtpAsync(member.Email ?? string.Empty, cancellationToken);

            await _activityService.LogActivityAsync(member.Id, "Registration", "New registry filing submitted for review.", member.Id, cancellationToken: cancellationToken);

            _logger.LogInformation("Registered application for MemberId {MemberId}", member.Id);
            
            // Trigger Live Admin Alert
            await _realTimeService.SendAdminAlertAsync("NEW_REGISTRATION", new { 
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

        public async Task<MemberRegistrationResultDto> GetStatusAsync(int memberId, CancellationToken cancellationToken = default)
        {
            var m = await _db.Members.FirstOrDefaultAsync(mm => mm.Id == memberId, cancellationToken);
            if (m == null) throw new KeyNotFoundException("Member not found");
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
            var isValid = await _otp.VerifyOtpAsync(email, otpCode, cancellationToken);
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

            await _otp.GenerateAndSendOtpAsync(email, cancellationToken);
            _logger.LogInformation("OTP resent to email {Email}", email);
            return true;
        }

        public async Task<ApproveMemberResultDto> ApproveMemberAsync(int memberId, int approvedByAdminId, CancellationToken cancellationToken = default)
        {
            Member? member;
            string membershipNumber;

            // Use a transaction to prevent race conditions during membership Serial generation
            using (var transaction = await _db.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, cancellationToken))
            {
                try
                {
                    // Find member with AcademicHistory
                    member = await _db.Members
                        .Include(m => m.AcademicHistory)
                        .FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);
                    if (member == null)
                    {
                        _logger.LogWarning("Approval failed: Member {MemberId} not found", memberId);
                        throw new KeyNotFoundException($"Member with ID {memberId} not found");
                    }

                    // Validate status
                    if (member.Status != Enums.MembershipStatus.Applied)
                    {
                        _logger.LogWarning("Approval failed: Member {MemberId} has status {Status}, expected Applied", memberId, member.Status);
                        throw new InvalidOperationException($"Member must have 'Applied' status to be approved. Current status: {member.Status}");
                    }

                    // Generate membership number: GHCYYMMXXX
                    var now = DateTime.UtcNow;
                    var prefix = $"GHC{now:yyMM}";
                    
                    var lastBound = await _db.Members
                        .Where(m => m.MembershipNumber != null && m.MembershipNumber.StartsWith(prefix))
                        .OrderByDescending(m => m.MembershipNumber)
                        .FirstOrDefaultAsync(cancellationToken);
                        
                    int nextId = 1;
                    if (lastBound != null && lastBound.MembershipNumber != null && lastBound.MembershipNumber.Length >= prefix.Length + 1)
                    {
                        var lastPart = lastBound.MembershipNumber.Substring(prefix.Length);
                        if (int.TryParse(lastPart, out int lastId))
                        {
                            nextId = lastId + 1;
                        }
                    }
                    
                    membershipNumber = member.MembershipNumber ?? $"{prefix}{nextId:D3}";

                    // Update member
                    member.Status = Enums.MembershipStatus.Active;
                    member.IsVerified = true; // Mark as verified upon admin approval
                    member.MembershipNumber = membershipNumber;
                    member.ApprovedDate = DateTime.UtcNow;
                    member.ApprovedBy = approvedByAdminId;

                    await _db.SaveChangesAsync(cancellationToken);
                    
                    // Award points for verification
                    await _gamification.AwardPointsAsync(memberId, "PROFILE_VERIFIED", metadata: "Initial approval", cancellationToken: cancellationToken);

                    await transaction.CommitAsync(cancellationToken);

                    await _activityService.LogActivityAsync(memberId, "Approved", $"Member approved by Admin {approvedByAdminId}. Membership Number: {membershipNumber}", approvedByAdminId, cancellationToken: cancellationToken);

                    _logger.LogInformation("Member {MemberId} approved by Admin {AdminId}. Membership Number: {MembershipNumber}", 
                        memberId, approvedByAdminId, membershipNumber);
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    throw;
                }
            }

            // Create user account using NID (without spaces) as both username and password
            var cleanNid = member.NID.Replace(" ", "");
            await _userService.CreateUserAccountAsync(memberId, cleanNid, cleanNid, cancellationToken);
            var defaultPassword = cleanNid; // Use NID as the default password display

            // Send Welcome Email
            try
            {
                var customVars = new Dictionary<string, string> { { "DefaultPassword", defaultPassword } };
                await _activityService.LogActivityAsync(memberId, "EmailSent", "Welcome email with credentials sent to member.", approvedByAdminId, cancellationToken: cancellationToken);
                await _communicationService.SendIndividualEmailAsync(memberId, "WELCOME_EMAIL", customVars, cancellationToken);
                
                // Add System Notification
                await _notificationService.CreateNotificationAsync(memberId, "Welcome to GHCAA!", "Your membership has been approved. You can now access the full portal.", Enums.NotificationType.RegistrationUpdate, "/portal/dashboard", cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send welcome email to member {MemberId}", memberId);
                // We don't throw here as the approval and account creation were successful
            }

            return new ApproveMemberResultDto
            {
                MembershipNumber = membershipNumber,
                DefaultPassword = defaultPassword
            };
        }

        public async Task<bool> RejectMemberAsync(int id, int adminId, string reason, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members.FindAsync(new object[] { id }, cancellationToken);
            if (member == null) return false;

            if (member.Status != Enums.MembershipStatus.Applied)
                throw new InvalidOperationException("Only pending registrations can be rejected.");

            // Send rejection email before deleting the record
            try
            {
                var customVars = new Dictionary<string, string> { { "Reason", reason } };
                // Using SendEmailByCodeAsync with the template ensures professional styling from DB settings
                await _communicationService.SendEmailByCodeAsync(member.Email, "APPLICATION_REJECTED", customVars, member, cancellationToken);
                
                await _activityService.LogActivityAsync(id, "EmailSent", "Application rejection email sent.", adminId, cancellationToken: cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send rejection email to {Email}", member.Email);
            }

            // Remove registry filing
            _db.Members.Remove(member);
            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogWarning("Admin {AdminId} rejected application {MemberId} for: {Reason}", adminId, id, reason);
            return true;
        }

        public async Task<MemberProfileDto?> GetProfileAsync(int memberId, bool isPrivileged = false, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members
                .Include(m => m.ECMembers)
                    .ThenInclude(em => em.ECPeriod)
                .Include(m => m.SentFamilyLinkRequests)
                    .ThenInclude(r => r.TargetMember)
                .Include(m => m.ReceivedFamilyLinkRequests)
                    .ThenInclude(r => r.Requester)
                .Include(m => m.AcademicHistory)
                .Include(m => m.ProfessionalHistory)
                .FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);
            
            if (member == null) return null;
            
            var gains = await GetMemberGainsAsync(member.Id, member.ContributionPoints, cancellationToken);
            var dto = new MemberProfileDto
            {
                Id = member.Id,
                FullName = member.FullName,
                Email = member.Email,
                MobileNo = member.MobileNo,
                MembershipNumber = member.MembershipNumber,
                Status = member.Status,
                PhotoPath = member.PhotoPath,
                PresentAddress = member.PresentAddress,
                PermanentAddress = member.PermanentAddress,
                BloodGroup = member.BloodGroup,
                MembershipType = member.MembershipType,
                Category = member.Category,
                FatherName = member.FatherName,
                MotherName = member.MotherName,
                DateOfBirth = member.DateOfBirth,
                Gender = member.Gender,
                NID = (isPrivileged || member.IsNIDPublic) ? member.NID : MaskPii(member.NID, 3, 2),
                IsNIDPublic = member.IsNIDPublic,
                EmergencyContactName = member.EmergencyContactName,
                EmergencyContactRelation = member.EmergencyContactRelation,
                EmergencyContactPhone = member.EmergencyContactPhone,
                IsMobilePublic = member.IsMobilePublic,
                IsEmailPublic = member.IsEmailPublic,
                IsAddressPublic = member.IsAddressPublic,
                HasAcceptedTerms = member.HasAcceptedTerms,
                NotifyEventCreation = member.NotifyEventCreation,
                NotifyParticipationApproval = member.NotifyParticipationApproval,
                NotifyRegistrationUpdate = member.NotifyRegistrationUpdate,
                NotifyRelevantUpdates = member.NotifyRelevantUpdates,
                AppliedDate = DateTime.SpecifyKind(member.AppliedDate, DateTimeKind.Utc),
                // Gamification & Health
                ContributionPoints = member.ContributionPoints,
                Rank = gains.rank,
                ProfileCompletionPercentage = CalculateProfileCompletion(member),
                // Family members from Request system
                FamilyMembers = new List<MemberFamilyDto>(),

                // Summary Data for easier display
                CategoryBadge = member.Category.ToString(),
                PassingYear = member.AcademicHistory?.FirstOrDefault(a => a.IsGHC)?.PassingYear,
                Degree = member.AcademicHistory?.FirstOrDefault(a => a.IsGHC)?.Degree,
                Subject = member.AcademicHistory?.FirstOrDefault(a => a.IsGHC)?.Subject,
                Designation = member.ProfessionalHistory?.FirstOrDefault(p => p.IsCurrent)?.Designation,
                OrganizationName = member.ProfessionalHistory?.FirstOrDefault(p => p.IsCurrent)?.OrganizationName,
                ProfessionalSector = member.ProfessionalHistory?.FirstOrDefault(p => p.IsCurrent)?.Sector,
                Location = member.ProfessionalHistory?.FirstOrDefault(p => p.IsCurrent)?.Location,
                
                // Detailed Collections
                AcademicHistory = member.AcademicHistory?.Select(a => new AcademicRecordDto
                {
                    Id = a.Id,
                    InstitutionName = a.InstitutionName,
                    Degree = a.Degree,
                    Subject = a.Subject,
                    AdmissionYear = a.AdmissionYear,
                    PassingYear = a.PassingYear,
                    IsGHC = a.IsGHC,
                    Result = a.Result,
                    CertificatePath = a.CertificatePath
                }).ToList() ?? new List<AcademicRecordDto>(),
                
                ProfessionalHistory = member.ProfessionalHistory?.Select(p => new ProfessionalRecordDto
                {
                    Id = p.Id,
                    OrganizationName = p.OrganizationName,
                    Designation = p.Designation,
                    Sector = p.Sector,
                    StartDate = p.StartDate,
                    EndDate = p.EndDate,
                    IsCurrent = p.IsCurrent,
                    Location = p.Location
                }).ToList() ?? new List<ProfessionalRecordDto>()
            };

            // Populate Family links from both sent and received requests
            if (member.SentFamilyLinkRequests != null)
            {
                foreach (var r in member.SentFamilyLinkRequests)
                {
                    if (r.TargetMember != null)
                    {
                        dto.FamilyMembers.Add(new MemberFamilyDto
                        {
                            RequestId = r.Id,
                            MemberId = r.TargetMemberId,
                            FullName = r.TargetMember.FullName,
                            MembershipNumber = r.TargetMember.MembershipNumber,
                            PhotoPath = r.TargetMember.PhotoPath,
                            IsVerified = r.TargetMember.IsVerified,
                            Relationship = r.Relationship,
                            Status = r.Status,
                            IsRequester = true
                        });
                    }
                }
            }

            if (member.ReceivedFamilyLinkRequests != null)
            {
                foreach (var r in member.ReceivedFamilyLinkRequests)
                {
                    if (r.Requester != null)
                    {
                        dto.FamilyMembers.Add(new MemberFamilyDto
                        {
                            RequestId = r.Id,
                            MemberId = r.RequesterId,
                            FullName = r.Requester.FullName,
                            MembershipNumber = r.Requester.MembershipNumber,
                            PhotoPath = r.Requester.PhotoPath,
                            IsVerified = r.Requester.IsVerified,
                            Relationship = r.Relationship,
                            Status = r.Status,
                            IsRequester = false
                        });
                    }
                }
            }

            if (member.ECMembers != null && member.ECMembers.Any())
            {
                dto.ECHistory = member.ECMembers.Select(em => new ECHistoryDto
                {
                    Id = em.Id,
                    PeriodTitle = em.ECPeriod?.Title ?? "Unknown",
                    Position = em.Position,
                    StartDate = em.StartDate.ToLocalTime(),
                    EndDate = em.EndDate.HasValue ? em.EndDate.Value.ToLocalTime() : null,
                    ChangeReason = em.ChangeReason,
                    IsCurrent = em.ECPeriod?.IsActive ?? false
                }).OrderByDescending(h => h.StartDate).ToList();
            }

            dto.AcademicHistory = member.AcademicHistory?.Select(a => new AcademicRecordDto
            {
                Id = a.Id,
                InstitutionName = a.InstitutionName,
                Degree = a.Degree,
                Subject = a.Subject,
                AdmissionYear = a.AdmissionYear,
                PassingYear = a.PassingYear,
                IsGHC = a.IsGHC,
                Result = a.Result
            }).OrderByDescending(a => a.PassingYear).ToList() ?? new();

            dto.ProfessionalHistory = member.ProfessionalHistory?.Select(p => new ProfessionalRecordDto
            {
                Id = p.Id,
                OrganizationName = p.OrganizationName,
                Designation = p.Designation,
                Sector = p.Sector,
                Location = p.Location,
                StartDate = DateTime.SpecifyKind(p.StartDate, DateTimeKind.Utc),
                EndDate = p.EndDate.HasValue ? DateTime.SpecifyKind(p.EndDate.Value, DateTimeKind.Utc) : null,
                IsCurrent = p.IsCurrent
            }).OrderByDescending(p => p.StartDate).ToList() ?? new();

            return dto;
        }

        public async Task<bool> UpdateProfileAsync(int memberId, UpdateProfileDto dto, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members
                .Include(m => m.AcademicHistory)
                .Include(m => m.ProfessionalHistory)
                .FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);
            if (member == null) return false;

            if (!string.IsNullOrWhiteSpace(dto.FullName)) member.FullName = dto.FullName;
            if (!string.IsNullOrWhiteSpace(dto.FatherName)) member.FatherName = dto.FatherName;
            if (!string.IsNullOrWhiteSpace(dto.MotherName)) member.MotherName = dto.MotherName;
            if (dto.DateOfBirth != default) member.DateOfBirth = DateTime.SpecifyKind(dto.DateOfBirth, DateTimeKind.Utc);
            member.Gender = dto.Gender;
            member.BloodGroup = dto.BloodGroup;
            if (!string.IsNullOrWhiteSpace(dto.PresentAddress)) member.PresentAddress = dto.PresentAddress;
            if (!string.IsNullOrWhiteSpace(dto.PermanentAddress)) member.PermanentAddress = dto.PermanentAddress;
            if (!string.IsNullOrWhiteSpace(dto.EmergencyContactName)) member.EmergencyContactName = dto.EmergencyContactName;
            if (!string.IsNullOrWhiteSpace(dto.EmergencyContactRelation)) member.EmergencyContactRelation = dto.EmergencyContactRelation;
            if (!string.IsNullOrWhiteSpace(dto.EmergencyContactPhone)) member.EmergencyContactPhone = dto.EmergencyContactPhone;
            if (!string.IsNullOrWhiteSpace(dto.TShirtSize)) member.TShirtSize = dto.TShirtSize;
            
            // Photo Path: Delete old file if path changes
            if (!string.IsNullOrWhiteSpace(dto.PhotoPath) && member.PhotoPath != dto.PhotoPath)
            {
                if (!string.IsNullOrEmpty(member.PhotoPath))
                {
                    try { await _storage.DeleteFileAsync(member.PhotoPath, cancellationToken); }
                    catch (Exception ex) { _logger.LogWarning(ex, "Failed to delete old photo {Path}", member.PhotoPath); }
                }
                member.PhotoPath = dto.PhotoPath;
            }

            member.IsMobilePublic = dto.IsMobilePublic;
            member.IsEmailPublic = dto.IsEmailPublic;
            member.IsAddressPublic = dto.IsAddressPublic;
            member.IsNIDPublic = dto.IsNIDPublic;

            // Notification Preferences
            member.NotifyEventCreation = dto.NotifyEventCreation;
            member.NotifyParticipationApproval = dto.NotifyParticipationApproval;
            member.NotifyRegistrationUpdate = dto.NotifyRegistrationUpdate;

            // Handle Academic History
            if (dto.AcademicHistory != null && dto.AcademicHistory.Any())
            {
                // Validation: At least one must be from Govt. Haraganga College
                if (!dto.AcademicHistory.Any(a => a.IsGHC || (a.InstitutionName != null && a.InstitutionName.Contains("Haraganga", StringComparison.OrdinalIgnoreCase))))
                {
                    throw new InvalidOperationException("At least one academic record must be from Govt. Haraganga College.");
                }

                // Clear existing and replace with new history
                member.AcademicHistory.Clear();
                foreach (var a in dto.AcademicHistory)
                {
                    member.AcademicHistory.Add(new AcademicRecord
                    {
                        InstitutionName = a.InstitutionName ?? "",
                        Degree = a.Degree ?? "",
                        Subject = a.Subject ?? "",
                        AdmissionYear = a.AdmissionYear,
                        PassingYear = a.PassingYear ?? 0,
                        IsGHC = a.IsGHC || (a.InstitutionName != null && a.InstitutionName.Contains("Haraganga", StringComparison.OrdinalIgnoreCase)),
                        Result = a.Result
                    });
                }
            }

            // Handle Professional History
            if (dto.ProfessionalHistory != null)
            {
                member.ProfessionalHistory.Clear();
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

            await _db.SaveChangesAsync(cancellationToken);
            await _activityService.LogActivityAsync(memberId, "Updated", "Member updated profile details and history.", cancellationToken: cancellationToken);
            _logger.LogInformation("Member {MemberId} updated profile and history", memberId);
            return true;
        }

        public async Task<bool> ArchiveMemberAsync(int memberId, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members.FindAsync(new object[] { memberId }, cancellationToken);
            if (member == null) return false;

            member.IsArchived = true;
            
            // Also archive the associated user if exists — rotate stamp to invalidate all sessions
            var user = await _db.Users.FirstOrDefaultAsync(u => u.MemberId == memberId, cancellationToken);
            if (user != null)
            {
                user.IsArchived = true;
                user.SecurityStamp = Guid.NewGuid().ToString("N"); // Invalidate all active JWTs
            }

            // End active EC roles
            var activeRoles = await _db.ECMembers
                .Where(em => em.MemberId == memberId && em.EndDate == null)
                .ToListAsync(cancellationToken);
            
            foreach (var role in activeRoles)
            {
                role.EndDate = DateTime.UtcNow;
                role.ChangeReason = "Member archived";
            }

            // Cascade: Archive Job Opportunities
            await _db.JobOpportunities
                .Where(j => j.PostedByMemberId == memberId && j.IsActive)
                .ExecuteUpdateAsync(s => s.SetProperty(j => j.IsActive, false), cancellationToken);

            // Cascade: Cancel Family Link Requests (both sent and received)
            await _db.FamilyLinkRequests
                .Where(r => (r.RequesterId == memberId || r.TargetMemberId == memberId) && r.Status == Enums.FamilyLinkStatus.Requested)
                .ExecuteUpdateAsync(s => s.SetProperty(r => r.Status, Enums.FamilyLinkStatus.Cancelled), cancellationToken);

            // Cascade: Unpublish news (AuthorId in NewsPost maps to User, who maps to Member)
            if (user != null)
            {
                await _db.NewsPosts
                    .Where(n => n.AuthorId == user.Id && n.IsActive)
                    .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsActive, false), cancellationToken);
            }

            await _db.SaveChangesAsync(cancellationToken);
            await _activityService.LogActivityAsync(memberId, "Archived", "Member archived (soft deleted).", cancellationToken: cancellationToken);
            _logger.LogInformation("Member {MemberId} archived (soft delete)", memberId);
            return true;
        }

        public async Task<bool> RestoreMemberAsync(int memberId, CancellationToken cancellationToken = default)
        {
            // Must use IgnoreQueryFilters to see archived records
            var member = await _db.Members.IgnoreQueryFilters().FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);
            if (member == null) return false;

            member.IsArchived = false;

            var user = await _db.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.MemberId == memberId, cancellationToken);
            if (user != null)
            {
                user.IsArchived = false;
            }

            await _db.SaveChangesAsync(cancellationToken);
            await _activityService.LogActivityAsync(memberId, "Restored", "Member record restored from archives.", cancellationToken: cancellationToken);
            _logger.LogInformation("Member {MemberId} restored", memberId);
            return true;
        }

        public async Task<bool> ReactivateMemberAsync(int memberId, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members.FindAsync(new object[] { memberId }, cancellationToken);
            if (member == null) return false;

            member.Status = Enums.MembershipStatus.Active;
            await _db.SaveChangesAsync(cancellationToken);
            await _activityService.LogActivityAsync(memberId, "Reactivated", "Member reactivated to Active status.", cancellationToken: cancellationToken);
            _logger.LogInformation("Member {MemberId} reactivated to Active status", memberId);
            return true;
        }

        public async Task<object?> GetMemberDocumentsAsync(int memberId, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members.FindAsync(new object[] { memberId }, cancellationToken);

            return new
            {
                Photo = member.PhotoPath
            };
        }

        public async Task<object> GetDashboardStatsAsync(bool isPrivileged, CancellationToken cancellationToken = default)
        {
            var totalMembers = await _db.Members.CountAsync(m => !m.IsArchived, cancellationToken);
            var applied = await _db.Members.CountAsync(m => m.Status == Enums.MembershipStatus.Applied && !m.IsArchived, cancellationToken);
            var active = await _db.Members.CountAsync(m => m.Status == Enums.MembershipStatus.Active && !m.IsArchived, cancellationToken);
            var inactive = await _db.Members.CountAsync(m => (m.Status == Enums.MembershipStatus.InactivePayment || m.Status == Enums.MembershipStatus.InactiveResigned) && !m.IsArchived, cancellationToken);
            
            decimal? balance = null;
            if (isPrivileged)
            {
                var totalCollection = await _db.FinancialRecords
                    .Where(r => r.RecordType == Enums.FinancialRecordType.Income)
                    .SumAsync(r => r.Amount, cancellationToken);
                
                var totalExpense = await _db.FinancialRecords
                    .Where(r => r.RecordType == Enums.FinancialRecordType.Expense)
                    .SumAsync(r => r.Amount, cancellationToken);
                    
                balance = totalCollection - totalExpense;
            }

            return new
            {
                TotalMembers = totalMembers,
                Applied = applied,
                Active = active,
                Inactive = inactive,
                Balance = balance,
                LastUpdated = DateTime.UtcNow
            };
        }

        public async Task<object> GetAllMembersAsync(int page = 1, int pageSize = 10, string searchQuery = "", string statusFilter = "all", string categoryFilter = "all", string membershipTypeFilter = "all", bool includeArchived = false, bool isPrivileged = false, CancellationToken cancellationToken = default)
        {
            IQueryable<Member> query = _db.Members
                .AsNoTracking()
                .AsSplitQuery() // Prevent duplicates from collection includes
                .Include(m => m.ECMembers)
                .ThenInclude(em => em.ECPeriod)
                .Include(m => m.AcademicHistory)
                .Include(m => m.ProfessionalHistory);
            
            if (includeArchived)
            {
                query = query.IgnoreQueryFilters();
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var q = searchQuery.ToLower();
                // Special case for M-ID search used in refresh
                if (q.StartsWith("m-") && int.TryParse(q.Substring(2), out var mid))
                {
                    query = query.Where(m => m.Id == mid);
                }
                else
                {
                    var terms = q.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    foreach (var term in terms)
                    {
                        query = query.Where(m => 
                            (m.FullName != null && m.FullName.ToLower().Contains(term)) ||
                            (m.Email != null && m.Email.ToLower().Contains(term)) ||
                            (m.MembershipNumber != null && m.MembershipNumber.ToLower().Contains(term)) ||
                            (m.MobileNo != null && m.MobileNo.ToLower().Contains(term)) ||
                            m.AcademicHistory.Any(a => a.InstitutionName.ToLower().Contains(term) || a.Degree.ToLower().Contains(term) || a.Subject.ToLower().Contains(term)) ||
                            m.ProfessionalHistory.Any(p => p.OrganizationName.ToLower().Contains(term) || p.Designation.ToLower().Contains(term)));
                    }
                }
            }

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(statusFilter) && statusFilter != "all")
            {
                if (statusFilter == "Applied")
                {
                    query = query.Where(m => m.Status == Enums.MembershipStatus.Applied);
                }
                else if (Enum.TryParse<Enums.MembershipStatus>(statusFilter, true, out var statusEnum))
                {
                    query = query.Where(m => m.Status == statusEnum);
                }
                else if (int.TryParse(statusFilter, out var statusInt))
                {
                    var casted = (Enums.MembershipStatus)statusInt;
                    query = query.Where(m => m.Status == casted);
                }
            }

            // Apply category filter
            if (!string.IsNullOrWhiteSpace(categoryFilter) && categoryFilter != "all")
            {
                if (Enum.TryParse<Enums.MemberCategory>(categoryFilter, true, out var catEnum))
                {
                    query = query.Where(m => m.Category == catEnum);
                }
                else if (int.TryParse(categoryFilter, out var catInt))
                {
                    var casted = (Enums.MemberCategory)catInt;
                    query = query.Where(m => m.Category == casted);
                }
            }

            // Apply membership type filter
            if (!string.IsNullOrWhiteSpace(membershipTypeFilter) && membershipTypeFilter != "all")
            {
                if (Enum.TryParse<Enums.MembershipType>(membershipTypeFilter, true, out var typeEnum))
                {
                    query = query.Where(m => m.MembershipType == typeEnum);
                }
                else if (int.TryParse(membershipTypeFilter, out var typeInt))
                {
                    var casted = (Enums.MembershipType)typeInt;
                    query = query.Where(m => m.MembershipType == casted);
                }
            }

            // Get total count
            var totalItems = await query.CountAsync(cancellationToken);

            // Apply paging
            var membersQuery = query.OrderByDescending(m => m.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var members = await membersQuery.ToListAsync(cancellationToken);
            var memberDtos = new List<MemberSummaryDto>();

            foreach (var member in members)
            {
                var gains = await GetMemberGainsAsync(member.Id, member.ContributionPoints, cancellationToken);
                var dto = new MemberSummaryDto
                {
                    Id = member.Id,
                    FullName = member.FullName,
                    Status = member.Status,
                    AppliedDate = member.AppliedDate.ToLocalTime(),
                    MembershipNumber = member.MembershipNumber,
                    PhotoPath = member.PhotoPath,
                    BloodGroup = member.BloodGroup,
                    MembershipType = member.MembershipType,
                    Category = member.Category,
                    IsEmailPublic = member.IsEmailPublic,
                    IsMobilePublic = member.IsMobilePublic,
                    IsAddressPublic = member.IsAddressPublic,
                    IsNIDPublic = member.IsNIDPublic,
                    
                    // Gamification
                    ContributionPoints = member.ContributionPoints,
                    Rank = gains.rank,
                    CategoryBadge = gains.badge,
                    
                    // Mask PII if not privileged (SuperAdmin or self) AND not public
                    Email = (isPrivileged || member.IsEmailPublic) ? (member.Email ?? "") : (MaskPii(member.Email, 3, 3) ?? ""),
                    MobileNo = (isPrivileged || member.IsMobilePublic) ? (member.MobileNo ?? "") : (MaskPii(member.MobileNo, 4, 3) ?? ""),
                    NID = (isPrivileged || member.IsNIDPublic) ? (member.NID ?? "") : (MaskPii(member.NID, 3, 2) ?? "")
                };

                if (member.ECMembers != null && member.ECMembers.Any())
                {
                    dto.ECHistory = member.ECMembers.Select(em => new ECHistoryDto
                    {
                        Id = em.Id,
                        PeriodId = em.ECPeriodId,
                        PeriodTitle = em.ECPeriod?.Title ?? "Unknown",
                        Position = em.Position,
                        StartDate = em.StartDate.ToLocalTime(),
                        EndDate = em.EndDate.HasValue ? em.EndDate.Value.ToLocalTime() : null,
                        ChangeReason = em.ChangeReason,
                        IsCurrent = em.ECPeriod?.IsActive ?? false
                    }).OrderByDescending(h => h.StartDate).ToList();
                }

                // Enhanced Summary Data from normalized tables
                var ghcRecord = member.AcademicHistory?.FirstOrDefault(a => a.IsGHC);
                if (ghcRecord != null)
                {
                    dto.GHCLastCertificatePassingYear = ghcRecord.PassingYear;
                    dto.GHCLastCertificate = ghcRecord.Degree;
                    dto.GHCLastCertificateSubject = ghcRecord.Subject;
                }

                var currentJob = member.ProfessionalHistory?.FirstOrDefault(p => p.IsCurrent);
                if (currentJob != null)
                {
                    dto.Designation = currentJob.Designation;
                    dto.OrganizationName = currentJob.OrganizationName;
                    dto.ProfessionalSector = currentJob.Sector;
                    dto.Location = currentJob.Location;
                }

                memberDtos.Add(dto);
            }

            return new
            {
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                CurrentPage = page,
                PageSize = pageSize,
                Items = memberDtos
            };
        }
        public async Task<bool> AdminUpdateMemberAsync(int id, AdminMemberUpdateDto dto, int adminId, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members
                .Include(m => m.AcademicHistory)
                .Include(m => m.ProfessionalHistory)
                .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
            if (member == null) return false;

            // Track membership type change
            if (member.MembershipType != dto.MembershipType)
            {
                await _financialService.RecordMembershipChangeAsync(
                    id, 
                    member.MembershipType.ToString(), 
                    dto.MembershipType.ToString(), 
                    adminId, 
                    dto.MembershipChangeReason ?? "Administrative Update", 
                    cancellationToken);
            }

            member.FullName = dto.FullName;
            member.FatherName = dto.FatherName;
            member.MotherName = dto.MotherName;
            member.DateOfBirth = DateTime.SpecifyKind(dto.DateOfBirth, DateTimeKind.Utc);
            member.NID = dto.NID;
            member.MobileNo = dto.MobileNo;
            member.Email = dto.Email;
            member.PresentAddress = dto.PresentAddress;
            member.PermanentAddress = dto.PermanentAddress;

            member.EmergencyContactName = dto.EmergencyContactName;
            member.EmergencyContactRelation = dto.EmergencyContactRelation;
            member.EmergencyContactPhone = dto.EmergencyContactPhone;
            if (!string.IsNullOrWhiteSpace(dto.TShirtSize)) member.TShirtSize = dto.TShirtSize;

            member.NotifyEventCreation = dto.NotifyEventCreation;
            member.NotifyParticipationApproval = dto.NotifyParticipationApproval;
            member.NotifyRegistrationUpdate = dto.NotifyRegistrationUpdate;
            member.NotifyRelevantUpdates = dto.NotifyRelevantUpdates;

            member.Gender = dto.Gender;
            member.BloodGroup = dto.BloodGroup;
            member.MembershipNumber = dto.MembershipNumber;
            if (!string.IsNullOrWhiteSpace(dto.PhotoPath)) member.PhotoPath = dto.PhotoPath;
            if (!string.IsNullOrWhiteSpace(dto.SignaturePath)) member.SignaturePath = dto.SignaturePath;

            member.MembershipType = dto.MembershipType;
            member.Status = dto.Status;
            // Domain Validation: Only Founding members can be Lifelong Patrons
            if (dto.Category == Enums.MemberCategory.LifelongPatron && member.MembershipType != Enums.MembershipType.Founding)
            {
                _logger.LogWarning("Admin update attempted to assign Lifelong Patron to a non-founding member {Id}.", id);
                throw new InvalidOperationException("Only Founding members can be assigned as Lifelong Patrons.");
            }

            member.Category = dto.Category;
            member.MembershipChangeReason = dto.MembershipChangeReason;
            member.ECChangeReason = dto.ECChangeReason;
            member.IsVerified = dto.IsVerified;
            member.ContributionPoints = dto.ContributionPoints;
            member.LastUpdateDate = DateTime.UtcNow;

            // Summary fields sync (for directory/search performance)
            if (!string.IsNullOrWhiteSpace(dto.CertificatePath)) member.CertificatePath = dto.CertificatePath;
            if (!string.IsNullOrWhiteSpace(dto.PaymentProofPath)) member.PaymentProofPath = dto.PaymentProofPath;

            // Sync Academic History
            if (dto.AcademicHistory != null)
            {
                // Validation matching RegisterAsync
                if (dto.AcademicHistory.Any() && !dto.AcademicHistory.Any(a => a.IsGHC || a.InstitutionName.Contains("Haraganga", StringComparison.OrdinalIgnoreCase)))
                {
                    throw new InvalidOperationException("At least one academic record must be from Govt. Haraganga College.");
                }

                member.AcademicHistory.Clear();
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

            // Sync Professional History
            if (dto.ProfessionalHistory != null)
            {
                member.ProfessionalHistory.Clear();
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

            // Sync EC History
            if (dto.ECHistory != null)
            {
                var existingEC = await _db.ECMembers.Where(ec => ec.MemberId == id).ToListAsync(cancellationToken);
                _db.ECMembers.RemoveRange(existingEC);

                foreach (var ec in dto.ECHistory)
                {
                    var period = await _db.ECPeriods.FirstOrDefaultAsync(p => p.Title == ec.PeriodTitle, cancellationToken);
                    if (period == null) continue;

                    _db.ECMembers.Add(new ECMember
                    {
                        MemberId = id,
                        ECPeriodId = period.Id,
                        Position = ec.Position,
                        StartDate = DateTime.SpecifyKind(ec.StartDate, DateTimeKind.Utc),
                        EndDate = ec.EndDate.HasValue ? DateTime.SpecifyKind(ec.EndDate.Value, DateTimeKind.Utc) : null,
                        ChangeReason = ec.ChangeReason
                    });
                }
            }

            member.IsMobilePublic = dto.IsMobilePublic;
            member.IsNIDPublic = dto.IsNIDPublic;

            member.IsEmailPublic = dto.IsEmailPublic;
            member.IsAddressPublic = dto.IsAddressPublic;
            member.IsVerified = dto.IsVerified;
            member.ContributionPoints = dto.ContributionPoints;
            member.LastUpdateDate = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);

            // Rotate SecurityStamp if status transitions to Terminated or Resigned — invalidates all JWT sessions
            if (dto.Status == Enums.MembershipStatus.Terminated || dto.Status == Enums.MembershipStatus.InactiveResigned)
            {
                var user = await _db.Users.FirstOrDefaultAsync(u => u.MemberId == id, cancellationToken);
                if (user != null)
                {
                    user.SecurityStamp = Guid.NewGuid().ToString("N");
                    await _db.SaveChangesAsync(cancellationToken);
                    _logger.LogWarning("SecurityStamp rotated for member {MemberId} — all existing sessions terminated.", id);
                }
            }

            _logger.LogInformation("Member {MemberId} information updated by Admin", id);
            return true;
        }

        public async Task<bool> UpdateMemberDocumentsAsync(int id, UploadedFileDto? certificate, UploadedFileDto? paymentProof, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members.FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
            if (member == null) return false;

            if (certificate != null)
            {
                var path = await _storage.SaveFileAsync(certificate.Content, certificate.FileName, id, Enums.FileUploadType.Certificate, cancellationToken);
                var fu = new FileUpload { MemberId = id, UploadType = Enums.FileUploadType.Certificate, FileName = certificate.FileName, FilePath = path, SizeBytes = certificate.Length };
                await _db.FileUploads.AddAsync(fu, cancellationToken);
            }

            if (paymentProof != null)
            {
                var path = await _storage.SaveFileAsync(paymentProof.Content, paymentProof.FileName, id, Enums.FileUploadType.PaymentProof, cancellationToken);
                var fu = new FileUpload { MemberId = id, UploadType = Enums.FileUploadType.PaymentProof, FileName = paymentProof.FileName, FilePath = path, SizeBytes = paymentProof.Length };
                await _db.FileUploads.AddAsync(fu, cancellationToken);
            }

            member.LastUpdateDate = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<string> UpdateMemberPhotoAsync(int memberId, UploadedFileDto photo, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members.FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);
            if (member == null) throw new KeyNotFoundException($"Member {memberId} not found.");

            // Delete old file if exists
            if (!string.IsNullOrEmpty(member.PhotoPath))
            {
                try { await _storage.DeleteFileAsync(member.PhotoPath, cancellationToken); }
                catch (Exception ex) { _logger.LogWarning(ex, "Failed to delete old photo {Path}", member.PhotoPath); }
            }

            var path = await _storage.SaveFileAsync(photo.Content, photo.FileName, memberId, Enums.FileUploadType.Photo, cancellationToken);
            var fu = new FileUpload { MemberId = memberId, UploadType = Enums.FileUploadType.Photo, FileName = photo.FileName, FilePath = path, SizeBytes = photo.Length };
            await _db.FileUploads.AddAsync(fu, cancellationToken);

            member.PhotoPath = path;
            member.LastUpdateDate = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Photo updated for member {MemberId}", memberId);
            return path;
        }

        public async Task<string> UpdateMemberSignatureAsync(int memberId, UploadedFileDto signature, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members.FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);
            if (member == null) throw new KeyNotFoundException($"Member {memberId} not found.");

            // Delete old file if exists
            if (!string.IsNullOrEmpty(member.SignaturePath))
            {
                try { await _storage.DeleteFileAsync(member.SignaturePath, cancellationToken); }
                catch (Exception ex) { _logger.LogWarning(ex, "Failed to delete old signature {Path}", member.SignaturePath); }
            }

            var path = await _storage.SaveFileAsync(signature.Content, signature.FileName, memberId, Enums.FileUploadType.Signature, cancellationToken);
            var fu = new FileUpload { MemberId = memberId, UploadType = Enums.FileUploadType.Signature, FileName = signature.FileName, FilePath = path, SizeBytes = signature.Length };
            await _db.FileUploads.AddAsync(fu, cancellationToken);

            member.SignaturePath = path;
            member.LastUpdateDate = DateTime.UtcNow;
            await _db.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Signature updated for member {MemberId}", memberId);
            return path;
        }

        public async Task<(bool Success, string? ResetUrl)> SendAdminPasswordResetLinkAsync(int memberId, CancellationToken cancellationToken = default)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.MemberId == memberId, cancellationToken);
            var member = await _db.Members.FindAsync(new object[] { memberId }, cancellationToken);
            
            if (user == null || member == null) return (false, null);

            // Generate a secure token
            var token = Guid.NewGuid().ToString("N");
            user.ResetToken = token;
            user.ResetTokenExpiry = DateTime.UtcNow.AddHours(24);
            await _db.SaveChangesAsync(cancellationToken);

            // Log activity
            await _activityService.LogActivityAsync(memberId, "Password Reset", "Admin initiated password reset email.", cancellationToken: cancellationToken);
 
            var clientUrl = _config[Constants.ConfigKeys.ClientUrl] ?? "http://localhost:4200";
            var resetUrl = $"{clientUrl}/reset-password?email={Uri.EscapeDataString(member.Email)}&token={token}";
            
            // Try fetching PASSWORD_RESET template from DB first (database-first strategy)
            var dbTemplate = await _communicationService.GetTemplateByCodeAsync(Constants.TemplateCodes.PasswordReset, cancellationToken);

            string subject, body;
            if (dbTemplate != null)
            {
                subject = dbTemplate.Subject
                    .Replace("{{FullName}}", member.FullName);
                body = dbTemplate.Body
                    .Replace("{{FullName}}", member.FullName)
                    .Replace("{{ResetUrl}}", resetUrl)
                    .Replace("{{MembershipNumber}}", member.MembershipNumber ?? "Pending");
            }
            else
            {
                // Fallback to hardcoded HTML
                subject = Constants.EmailSubjects.PasswordReset;
                body = $@"
                <div style='font-family: sans-serif; padding: 20px; border: 1px solid #eee; border-radius: 10px; max-width: 600px; margin: auto;'>
                    <h2 style='color: #c5a059;'>Password Reset Initiated</h2>
                    <p>Hello <strong>{member.FullName}</strong>,</p>
                    <p>An administrator has initiated a password reset for your {Constants.Branding.AppName} account.</p>
                    <p>Please click the button below to set a new password. This link is valid for 24 hours.</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{resetUrl}' style='background: #111; color: #c5a059; padding: 12px 30px; text-decoration: none; border-radius: 8px; font-weight: 800; display: inline-block; border: 1px solid #c5a059;'>Reset My Password</a>
                    </div>
                    <p style='color: #666; font-size: 0.9rem;'>If you did not request this, please ignore this email.</p>
                </div>";
            }

            try
            {
                await _email.SendEmailAsync(member.Email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send password reset email to {Email}", member.Email);
                // We still return true and the link so the admin can copy it manually
            }

            return (true, resetUrl);
        }
        public async Task<int> BulkArchiveInactiveMembersAsync(CancellationToken cancellationToken = default)
        {
            var inactiveStatuses = new[] { 
                Enums.MembershipStatus.InactivePayment, 
                Enums.MembershipStatus.InactiveResigned, 
                Enums.MembershipStatus.Terminated 
            };
            
            // Criteria: Inactive for more than 6 months
            var threshold = DateTime.UtcNow.AddMonths(-6);
            
            var targetMembers = await _db.Members
                .Where(m => !m.IsArchived && inactiveStatuses.Contains(m.Status) && m.LastUpdateDate < threshold)
                .Select(m => m.Id)
                .ToListAsync(cancellationToken);
                
            int processedCount = 0;
            foreach (var id in targetMembers)
            {
                try 
                {
                    await ArchiveMemberAsync(id, cancellationToken);
                    processedCount++;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to bulk archive member {MemberId}", id);
                }
            }
            
            if (processedCount > 0)
                _logger.LogInformation("Successfully bulk-archived {Count} inactive members.", processedCount);
                
            return processedCount;
        }

        public async Task<object> GetPublicStatsAsync(CancellationToken cancellationToken = default)
        {
            var activeMembers = await _db.Members.CountAsync(m => m.Status == Enums.MembershipStatus.Active && !m.IsArchived, cancellationToken);
            var ecMembers = await _db.ECMembers.Where(em => em.ECPeriod != null && em.ECPeriod.IsActive && em.EndDate == null).CountAsync(cancellationToken);
            var totalEvents = await _db.AlumniEvents.CountAsync(e => e.Status == Enums.EventStatus.Published, cancellationToken);
            
            return new
            {
                TotalActiveMembers = activeMembers,
                CurrentECMembers = ecMembers,
                PublishedEvents = totalEvents,
                AlumniChapters = 12, // Placeholder
                LastUpdated = DateTime.UtcNow
            };
        }

        private string? MaskPii(string? value, int visibleStart = 4, int visibleEnd = 2)
        {
            if (string.IsNullOrEmpty(value)) return value;
            if (value.Length <= (visibleStart + visibleEnd)) return new string('*', Math.Max(value.Length, 6));
            
            var start = value.Substring(0, visibleStart);
            var end = value.Substring(value.Length - visibleEnd);
            var middle = new string('*', value.Length - (visibleStart + visibleEnd));
            return $"{start}{middle}{end}";
        }

        // --- Gamification & Profile Health Helpers ---

        private decimal CalculateProfileCompletion(Member member)
        {
            int totalFields = 11;
            int completedFields = 0;

            if (!string.IsNullOrEmpty(member.FullName)) completedFields++;
            if (!string.IsNullOrEmpty(member.Email)) completedFields++;
            if (!string.IsNullOrEmpty(member.MobileNo)) completedFields++;
            if (member.DateOfBirth != default && member.DateOfBirth.Year > 1900) completedFields++;
            if (member.Gender != Enums.Gender.None) completedFields++;
            if (!string.IsNullOrEmpty(member.MembershipNumber)) completedFields++;
            if (!string.IsNullOrEmpty(member.PhotoPath)) completedFields++;
            
            var ghcRecord = member.AcademicHistory?.FirstOrDefault(a => a.IsGHC);
            if (ghcRecord != null && ghcRecord.PassingYear > 0) completedFields++;
            if (ghcRecord != null && !string.IsNullOrEmpty(ghcRecord.Subject)) completedFields++;
            
            if (!string.IsNullOrEmpty(member.PresentAddress)) completedFields++;
            if (member.BloodGroup != Enums.BloodGroup.Unknown) completedFields++;

            return Math.Round((decimal)completedFields / totalFields * 100, 2);
        }

        private string GetCategoryBadge(int points)
        {
            if (points >= 1000) return "Legend";
            if (points >= 500) return "Elite";
            if (points >= 200) return "Active";
            return "Member";
        }

        private async Task<(int rank, string badge)> GetMemberGainsAsync(int memberId, int points, CancellationToken cancellationToken)
        {
            var rank = await _db.Members
                .Where(m => m.ContributionPoints > points && !m.IsArchived)
                .CountAsync(cancellationToken) + 1;

            return (rank, GetCategoryBadge(points));
        }
    }
}

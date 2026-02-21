using System;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GHCAA.Infrastructure.Services
{
    public class MemberService : IMemberService
    {
        private readonly ApplicationDbContext _db;
        private readonly IFileStorageService _storage;
        private readonly IFileUploadRepository _fileRepo;
        private readonly IOtpService _otp;
        private readonly IEmailService _email;
        private readonly IUserService _userService;
        private readonly ICommunicationService _communicationService;
        private readonly ILogger<MemberService> _logger;
        private readonly IActivityService _activityService;

        public MemberService(
            ApplicationDbContext db,
            IFileStorageService storage,
            IFileUploadRepository fileRepo,
            IOtpService otp,
            IEmailService email,
            IUserService userService,
            ICommunicationService communicationService,
            ILogger<MemberService> logger,
            IActivityService activityService)
        {
            _db = db;
            _storage = storage;
            _fileRepo = fileRepo;
            _otp = otp;
            _email = email;
            _userService = userService;
            _communicationService = communicationService;
            _logger = logger;
            _activityService = activityService;
        }

        public async Task<int> RegisterAsync(MemberRegistrationDto dto, UploadedFileDto? photo, UploadedFileDto? certificate, UploadedFileDto? paymentProof, CancellationToken cancellationToken = default)
        {
            // Prevent duplicates by NID/Email/Mobile
            if (await _db.Members.AnyAsync(m => m.Email == dto.Email || m.NID == dto.NID || m.MobileNo == dto.MobileNo, cancellationToken))
                throw new InvalidOperationException("Member with same Email, NID, or Mobile already exists.");

            var member = new Member
            {
                FullName = dto.FullName,
                FatherName = dto.FatherName,
                MotherName = dto.MotherName,
                DateOfBirth = dto.DateOfBirth,
                Gender = Enum.Parse<Enums.Gender>(dto.Gender),
                BloodGroup = Enum.Parse<Enums.BloodGroup>(dto.BloodGroup),
                NID = dto.NID,
                MobileNo = dto.MobileNo,
                Email = dto.Email,
                PresentAddress = dto.PresentAddress,
                PermanentAddress = dto.PermanentAddress,
                EmergencyContactName = dto.EmergencyContactName,
                EmergencyContactRelation = dto.EmergencyContactRelation,
                EmergencyContactPhone = dto.EmergencyContactPhone,
                HSCAdmissionYear = dto.HSCAdmissionYear,
                GHCAdmissionYear = dto.GHCAdmissionYear,
                LastCertificateFromGHC = dto.LastDegreeFromGHC,
                SubjectGroup = dto.SubjectGroup,
                GHCLastCertificatePassingYear = dto.GHCLastCertificatePassingYear,
                ProfessionalSector = dto.ProfessionalSector,
                Designation = dto.Designation,
                Status = Enums.MembershipStatus.Applied,
                AppliedDate = DateTime.UtcNow,
                EmailVerified = false
            };

            // Use synchronous Add to avoid missing extension methods in certain EF versions
            _db.Members.Add(member);
            await _db.SaveChangesAsync(cancellationToken);

            // Save files if present (use UploadedFileDto.Content stream)
            if (photo != null)
            {
                var path = await _storage.SaveFileAsync(photo.Content, photo.FileName, member.Id, Enums.FileUploadType.Photo, cancellationToken);
                var fu = new FileUpload { MemberId = member.Id, UploadType = Enums.FileUploadType.Photo, FileName = photo.FileName, FilePath = path, SizeBytes = photo.Length };
                await _fileRepo.AddAsync(fu, cancellationToken);
                member.PhotoPath = fu.FilePath;
            }

            if (certificate != null)
            {
                var path = await _storage.SaveFileAsync(certificate.Content, certificate.FileName, member.Id, Enums.FileUploadType.Certificate, cancellationToken);
                var fu = new FileUpload { MemberId = member.Id, UploadType = Enums.FileUploadType.Certificate, FileName = certificate.FileName, FilePath = path, SizeBytes = certificate.Length };
                await _fileRepo.AddAsync(fu, cancellationToken);
                member.CertificatePath = fu.FilePath;
            }

            if (paymentProof != null)
            {
                var path = await _storage.SaveFileAsync(paymentProof.Content, paymentProof.FileName, member.Id, Enums.FileUploadType.PaymentProof, cancellationToken);
                var fu = new FileUpload { MemberId = member.Id, UploadType = Enums.FileUploadType.PaymentProof, FileName = paymentProof.FileName, FilePath = path, SizeBytes = paymentProof.Length };
                await _fileRepo.AddAsync(fu, cancellationToken);
                member.PaymentProofPath = fu.FilePath;
            }

            // update member with file paths
            _db.Members.Update(member);
            await _db.SaveChangesAsync(cancellationToken);

            // Generate & send OTP
            await _otp.GenerateAndSendOtpAsync(member.Email, cancellationToken);

            _logger.LogInformation("Registered application for MemberId {MemberId}", member.Id);
            return member.Id;
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

        public async Task<ApproveMemberResultDto> ApproveMemberAsync(int memberId, int approvedByAdminId, CancellationToken cancellationToken = default)
        {
            // Find member
            var member = await _db.Members.FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);
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

            // Generate membership number: GHC-{PassingYear}-{Serial}
            var passingYear = member.GHCLastCertificatePassingYear;
            var existingMembersCount = await _db.Members
                .Where(m => m.GHCLastCertificatePassingYear == passingYear && m.MembershipNumber != null)
                .CountAsync(cancellationToken);
            
            var serial = (existingMembersCount + 1).ToString("D4"); // 4-digit zero-padded
            var membershipNumber = $"GHC-{passingYear}-{serial}";

            // Update member
            member.Status = Enums.MembershipStatus.Active;
            member.MembershipNumber = membershipNumber;
            member.ApprovedDate = DateTime.UtcNow;
            member.ApprovedBy = approvedByAdminId;

            await _db.SaveChangesAsync(cancellationToken);

            await _activityService.LogActivityAsync(memberId, "Approved", $"Member approved by Admin {approvedByAdminId}. Membership Number: {membershipNumber}", approvedByAdminId, cancellationToken: cancellationToken);

            _logger.LogInformation("Member {MemberId} approved by Admin {AdminId}. Membership Number: {MembershipNumber}", 
                memberId, approvedByAdminId, membershipNumber);

            // Create user account
            var defaultPassword = _userService.GenerateDefaultPassword();
            await _userService.CreateUserAccountAsync(memberId, membershipNumber, defaultPassword, cancellationToken);

            // Send Welcome Email
            try
            {
                var customVars = new Dictionary<string, string> { { "DefaultPassword", defaultPassword } };
                await _activityService.LogActivityAsync(memberId, "EmailSent", "Welcome email with credentials sent to member.", approvedByAdminId, cancellationToken: cancellationToken);
                await _communicationService.SendIndividualEmailAsync(memberId, "WELCOME_EMAIL", customVars, cancellationToken);
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

            // Remove registry filing or archive it? 
            // Better to remove it so they can re-register if it was a data error.
            _db.Members.Remove(member);
            await _db.SaveChangesAsync(cancellationToken);

            // Notify user via email
            await _email.SendEmailAsync(member.Email, "GHCAA Application Update", 
                $"Dear {member.FullName}, your registry application was not approved for the following reason: {reason}. You are welcome to submit a new application with updated data.");

            _logger.LogWarning("Admin {AdminId} rejected application {MemberId} for: {Reason}", adminId, id, reason);
            return true;
        }

        public async Task<MemberProfileDto?> GetProfileAsync(int memberId, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members.FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);
            if (member == null) return null;

            return new MemberProfileDto
            {
                Id = member.Id,
                FullName = member.FullName,
                Email = member.Email,
                MobileNo = member.MobileNo,
                MembershipNumber = member.MembershipNumber,
                Status = member.Status,
                GHCLastCertificatePassingYear = member.GHCLastCertificatePassingYear,
                LastCertificateFromGHC = member.LastCertificateFromGHC,
                SubjectGroup = member.SubjectGroup,
                ProfessionalSector = member.ProfessionalSector,
                Designation = member.Designation,
                PhotoPath = member.PhotoPath,
                PresentAddress = member.PresentAddress,
                PermanentAddress = member.PermanentAddress,
                BloodGroup = member.BloodGroup,
                MembershipType = member.MembershipType,
                FatherName = member.FatherName,
                MotherName = member.MotherName,
                DateOfBirth = member.DateOfBirth,
                Gender = member.Gender,
                NID = member.NID,
                EmergencyContactName = member.EmergencyContactName,
                EmergencyContactRelation = member.EmergencyContactRelation,
                EmergencyContactPhone = member.EmergencyContactPhone,
                HSCAdmissionYear = member.HSCAdmissionYear,
                GHCAdmissionYear = member.GHCAdmissionYear,
                CertificatePath = member.CertificatePath,
                IsMobilePublic = member.IsMobilePublic,
                IsEmailPublic = member.IsEmailPublic,
                IsAddressPublic = member.IsAddressPublic
            };
        }

        public async Task<bool> UpdateProfileAsync(int memberId, UpdateProfileDto dto, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members.FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);
            if (member == null) return false;

            member.PresentAddress = dto.PresentAddress;
            member.PermanentAddress = dto.PermanentAddress;
            member.ProfessionalSector = dto.ProfessionalSector;
            member.Designation = dto.Designation;
            member.SubjectGroup = dto.SubjectGroup;
            member.LastCertificateFromGHC = dto.LastDegreeFromGHC;
            member.GHCLastCertificatePassingYear = dto.GHCLastCertificatePassingYear;
            member.IsMobilePublic = dto.IsMobilePublic;
            member.IsEmailPublic = dto.IsEmailPublic;
            member.IsAddressPublic = dto.IsAddressPublic;

            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Profile updated for member {MemberId}", memberId);
            await _activityService.LogActivityAsync(memberId, "ProfileUpdate", "Member updated their profile information.", memberId, cancellationToken: cancellationToken);
            return true;
        }

        public async Task<bool> ArchiveMemberAsync(int memberId, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members.FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);
            if (member == null) return false;

            member.IsArchived = true;
            
            // Also archive the associated user if exists
            var user = await _db.Users.FirstOrDefaultAsync(u => u.MemberId == memberId, cancellationToken);
            if (user != null)
            {
                user.IsArchived = true;
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
            if (member == null) return null;

            return new
            {
                Photo = member.PhotoPath,
                Certificate = member.CertificatePath,
                PaymentProof = member.PaymentProofPath
            };
        }

        public async Task<object> GetDashboardStatsAsync(CancellationToken cancellationToken = default)
        {
            var totalMembers = await _db.Members.CountAsync(m => !m.IsArchived, cancellationToken);
            var applied = await _db.Members.CountAsync(m => m.Status == Enums.MembershipStatus.Applied && !m.IsArchived, cancellationToken);
            var active = await _db.Members.CountAsync(m => m.Status == Enums.MembershipStatus.Active && !m.IsArchived, cancellationToken);
            var inactive = await _db.Members.CountAsync(m => (m.Status == Enums.MembershipStatus.InactivePayment || m.Status == Enums.MembershipStatus.InactiveResigned) && !m.IsArchived, cancellationToken);
            
            var totalCollection = await _db.FinancialRecords
                .Where(r => r.RecordType == Enums.FinancialRecordType.Income)
                .SumAsync(r => r.Amount, cancellationToken);
            
            var totalExpense = await _db.FinancialRecords
                .Where(r => r.RecordType == Enums.FinancialRecordType.Expense)
                .SumAsync(r => r.Amount, cancellationToken);

            return new
            {
                TotalMembers = totalMembers,
                Applied = applied,
                Active = active,
                Inactive = inactive,
                Balance = totalCollection - totalExpense,
                LastUpdated = DateTime.UtcNow
            };
        }

        public async Task<IEnumerable<MemberProfileDto>> GetAllMembersAsync(bool includeArchived = false, CancellationToken cancellationToken = default)
        {
            IQueryable<Member> query = _db.Members;
            
            if (includeArchived)
            {
                query = query.IgnoreQueryFilters();
            }

            return await query.Select(member => new MemberProfileDto
            {
                Id = member.Id,
                FullName = member.FullName,
                Email = member.Email,
                MobileNo = member.MobileNo,
                MembershipNumber = member.MembershipNumber,
                Status = member.Status,
                GHCLastCertificatePassingYear = member.GHCLastCertificatePassingYear,
                LastCertificateFromGHC = member.LastCertificateFromGHC,
                SubjectGroup = member.SubjectGroup,
                ProfessionalSector = member.ProfessionalSector,
                Designation = member.Designation,
                PhotoPath = member.PhotoPath,
                PresentAddress = member.PresentAddress,
                PermanentAddress = member.PermanentAddress,
                BloodGroup = member.BloodGroup,
                MembershipType = member.MembershipType,
                FatherName = member.FatherName,
                MotherName = member.MotherName,
                DateOfBirth = member.DateOfBirth,
                Gender = member.Gender,
                NID = member.NID,
                EmergencyContactName = member.EmergencyContactName,
                EmergencyContactRelation = member.EmergencyContactRelation,
                EmergencyContactPhone = member.EmergencyContactPhone,
                HSCAdmissionYear = member.HSCAdmissionYear,
                GHCAdmissionYear = member.GHCAdmissionYear,
                CertificatePath = member.CertificatePath,
                IsMobilePublic = member.IsMobilePublic,
                IsEmailPublic = member.IsEmailPublic,
                IsAddressPublic = member.IsAddressPublic
            }).ToListAsync(cancellationToken);
        }
    }
}
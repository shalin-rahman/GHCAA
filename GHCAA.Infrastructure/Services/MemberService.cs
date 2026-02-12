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
        private readonly ILogger<MemberService> _logger;

        public MemberService(
            ApplicationDbContext db,
            IFileStorageService storage,
            IFileUploadRepository fileRepo,
            IOtpService otp,
            IEmailService email,
            IUserService userService,
            ILogger<MemberService> logger)
        {
            _db = db;
            _storage = storage;
            _fileRepo = fileRepo;
            _otp = otp;
            _email = email;
            _userService = userService;
            _logger = logger;
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
                LastDegreeFromGHC = Enum.Parse<Enums.Degree>(dto.LastDegreeFromGHC),
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

            _logger.LogInformation("Member {MemberId} approved by Admin {AdminId}. Membership Number: {MembershipNumber}", 
                memberId, approvedByAdminId, membershipNumber);

            // Create user account
            var defaultPassword = _userService.GenerateDefaultPassword();
            await _userService.CreateUserAccountAsync(memberId, membershipNumber, defaultPassword, cancellationToken);

            return new ApproveMemberResultDto
            {
                MembershipNumber = membershipNumber,
                DefaultPassword = defaultPassword
            };
        }
    }
}
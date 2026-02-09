GHCAA.Infrastructure\Services\MemberService.cs
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Services
{
    public class MemberService : IMemberService
    {
        private readonly ApplicationDbContext _db;
        private readonly IFileStorageService _storage;
        private readonly IFileUploadRepository _fileRepo;
        private readonly IOtpService _otp;
        private readonly IEmailService _email;
        private readonly ILogger<MemberService> _logger;

        public MemberService(ApplicationDbContext db, IFileStorageService storage, IFileUploadRepository fileRepo, IOtpService otp, IEmailService email, ILogger<MemberService> logger)
        {
            _db = db;
            _storage = storage;
            _fileRepo = fileRepo;
            _otp = otp;
            _email = email;
            _logger = logger;
        }

        public async Task<int> RegisterAsync(MemberRegistrationDto dto, IFormFile? photo, IFormFile? certificate, IFormFile? paymentProof, CancellationToken cancellationToken = default)
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
                Gender = Enum.Parse<GHCAA.Domain.Enums.Gender>(dto.Gender),
                BloodGroup = Enum.Parse<GHCAA.Domain.Enums.BloodGroup>(dto.BloodGroup),
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
                LastDegreeFromGHC = dto.LastDegreeFromGHC,
                SubjectGroup = dto.SubjectGroup,
                GHCLastCertificatePassingYear = dto.GHCLastCertificatePassingYear,
                ProfessionalSector = dto.ProfessionalSector,
                Designation = dto.Designation,
                Status = GHCAA.Domain.Enums.MembershipStatus.Applied,
                AppliedDate = DateTime.UtcNow,
                EmailVerified = false // Add this property to Member entity (see change below)
            };

            await _db.Members.AddAsync(member, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            // Save files if present
            if (photo != null)
            {
                var path = await _storage.SaveFileAsync(photo.OpenReadStream(), photo.FileName, member.Id, GHCAA.Domain.Enums.FileUploadType.Photo, cancellationToken);
                var fu = new FileUpload { MemberId = member.Id, UploadType = GHCAA.Domain.Enums.FileUploadType.Photo, FileName = photo.FileName, FilePath = path, SizeBytes = photo.Length };
                await _fileRepo.AddAsync(fu, cancellationToken);
                member.PhotoPath = fu.FilePath;
            }

            if (certificate != null)
            {
                var path = await _storage.SaveFileAsync(certificate.OpenReadStream(), certificate.FileName, member.Id, GHCAA.Domain.Enums.FileUploadType.Certificate, cancellationToken);
                var fu = new FileUpload { MemberId = member.Id, UploadType = GHCAA.Domain.Enums.FileUploadType.Certificate, FileName = certificate.FileName, FilePath = path, SizeBytes = certificate.Length };
                await _fileRepo.AddAsync(fu, cancellationToken);
                member.CertificatePath = fu.FilePath;
            }

            if (paymentProof != null)
            {
                var path = await _storage.SaveFileAsync(paymentProof.OpenReadStream(), paymentProof.FileName, member.Id, GHCAA.Domain.Enums.FileUploadType.PaymentProof, cancellationToken);
                var fu = new FileUpload { MemberId = member.Id, UploadType = GHCAA.Domain.Enums.FileUploadType.PaymentProof, FileName = paymentProof.FileName, FilePath = path, SizeBytes = paymentProof.Length };
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
            var m = await _db.Members.FindAsync(new object[] { memberId }, cancellationToken);
            if (m == null) throw new KeyNotFoundException("Member not found");
            return new MemberRegistrationResultDto
            {
                MemberId = m.Id,
                Message = $"Status: {m.Status}",
                EmailSent = m.Email != null
            };
        }
    }
}
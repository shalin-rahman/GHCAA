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
        private readonly INotificationService _notificationService;

        public MemberService(
            ApplicationDbContext db,
            IFileStorageService storage,
            IFileUploadRepository fileRepo,
            IOtpService otp,
            IEmailService email,
            IUserService userService,
            ICommunicationService communicationService,
            ILogger<MemberService> logger,
            IActivityService activityService,
            INotificationService notificationService)
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
            _notificationService = notificationService;
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
                HighestCertificate = dto.HighestCertificate,
                HighestCertificateGroup = dto.HighestCertificateGroup,
                HighestCertificateSubject = dto.HighestCertificateSubject,
                HighestCertificatePassingYear = dto.HighestCertificatePassingYear,
                GHCAdmissionYear = dto.GHCAdmissionYear,
                GHCLastCertificate = dto.GHCLastCertificate,
                GHCLastCertificateGroup = dto.GHCLastCertificateGroup,
                GHCLastCertificateSubject = dto.GHCLastCertificateSubject,
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
                
                // Add System Notification
                await _notificationService.CreateNotificationAsync(memberId, "Welcome to GHCAA!", "Your membership has been approved. You can now access the full portal.", "Approval", "/portal/dashboard", cancellationToken);
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
            var member = await _db.Members
                .Include(m => m.ECMembers)
                .ThenInclude(em => em.ECPeriod)
                .FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);
            
            if (member == null) return null;

            var dto = new MemberProfileDto
            {
                Id = member.Id,
                FullName = member.FullName,
                Email = member.Email,
                MobileNo = member.MobileNo,
                MembershipNumber = member.MembershipNumber,
                Status = member.Status,
                GHCLastCertificatePassingYear = member.GHCLastCertificatePassingYear,
                GHCLastCertificateGroup = member.GHCLastCertificateGroup,
                GHCLastCertificateSubject = member.GHCLastCertificateSubject,
                GHCLastCertificate = member.GHCLastCertificate,
                HighestCertificate = member.HighestCertificate,
                HighestCertificateGroup = member.HighestCertificateGroup,
                HighestCertificateSubject = member.HighestCertificateSubject,
                HighestCertificatePassingYear = member.HighestCertificatePassingYear,
                ProfessionalSector = member.ProfessionalSector,
                Designation = member.Designation,
                PhotoPath = member.PhotoPath,
                PresentAddress = member.PresentAddress,
                PermanentAddress = member.PermanentAddress,
                BloodGroup = member.BloodGroup,
                MembershipType = member.MembershipType,
                Category = member.Category,
                ECPosition = member.ECPosition,
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

            if (member.ECMembers != null && member.ECMembers.Any())
            {
                dto.ECHistory = member.ECMembers.Select(em => new ECHistoryDto
                {
                    PeriodTitle = em.ECPeriod?.Title ?? "Unknown",
                    Position = em.Position,
                    StartDate = em.StartDate,
                    EndDate = em.EndDate,
                    ChangeReason = em.ChangeReason,
                    IsCurrent = em.ECPeriod?.IsActive ?? false
                }).OrderByDescending(h => h.StartDate).ToList();
            }

            return dto;
        }

        public async Task<bool> UpdateProfileAsync(int memberId, UpdateProfileDto dto, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members.FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);
            if (member == null) return false;

            if (!string.IsNullOrWhiteSpace(dto.FullName)) member.FullName = dto.FullName;
            if (!string.IsNullOrWhiteSpace(dto.FatherName)) member.FatherName = dto.FatherName;
            if (!string.IsNullOrWhiteSpace(dto.MotherName)) member.MotherName = dto.MotherName;
            if (dto.DateOfBirth != default) member.DateOfBirth = dto.DateOfBirth;
            member.Gender = dto.Gender;
            member.BloodGroup = dto.BloodGroup;
            if (!string.IsNullOrWhiteSpace(dto.PresentAddress)) member.PresentAddress = dto.PresentAddress;
            if (!string.IsNullOrWhiteSpace(dto.PermanentAddress)) member.PermanentAddress = dto.PermanentAddress;
            if (!string.IsNullOrWhiteSpace(dto.ProfessionalSector)) member.ProfessionalSector = dto.ProfessionalSector;
            if (!string.IsNullOrWhiteSpace(dto.Designation)) member.Designation = dto.Designation;
            member.HSCAdmissionYear = dto.HSCAdmissionYear;
            if (!string.IsNullOrWhiteSpace(dto.HighestCertificate)) member.HighestCertificate = dto.HighestCertificate;
            if (!string.IsNullOrWhiteSpace(dto.HighestCertificateGroup)) member.HighestCertificateGroup = dto.HighestCertificateGroup;
            if (!string.IsNullOrWhiteSpace(dto.HighestCertificateSubject)) member.HighestCertificateSubject = dto.HighestCertificateSubject;
            if (dto.HighestCertificatePassingYear > 0) member.HighestCertificatePassingYear = dto.HighestCertificatePassingYear;
            member.GHCAdmissionYear = dto.GHCAdmissionYear;
            if (!string.IsNullOrWhiteSpace(dto.GHCLastCertificate)) member.GHCLastCertificate = dto.GHCLastCertificate;
            if (!string.IsNullOrWhiteSpace(dto.GHCLastCertificateGroup)) member.GHCLastCertificateGroup = dto.GHCLastCertificateGroup;
            if (!string.IsNullOrWhiteSpace(dto.GHCLastCertificateSubject)) member.GHCLastCertificateSubject = dto.GHCLastCertificateSubject;
            if (dto.GHCLastCertificatePassingYear > 0) member.GHCLastCertificatePassingYear = dto.GHCLastCertificatePassingYear;
            
            if (!string.IsNullOrWhiteSpace(dto.EmergencyContactName)) member.EmergencyContactName = dto.EmergencyContactName;
            if (!string.IsNullOrWhiteSpace(dto.EmergencyContactRelation)) member.EmergencyContactRelation = dto.EmergencyContactRelation;
            if (!string.IsNullOrWhiteSpace(dto.EmergencyContactPhone)) member.EmergencyContactPhone = dto.EmergencyContactPhone;

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

            // End active EC roles
            var activeRoles = await _db.ECMembers
                .Where(em => em.MemberId == memberId && em.EndDate == null)
                .ToListAsync(cancellationToken);
            
            foreach (var role in activeRoles)
            {
                role.EndDate = DateTime.UtcNow;
                role.ChangeReason = "Member archived";
            }
            member.ECPosition = Enums.ECPosition.None;

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

        public async Task<object> GetAllMembersAsync(int page = 1, int pageSize = 10, string searchQuery = "", string statusFilter = "all", bool includeArchived = false, CancellationToken cancellationToken = default)
        {
            IQueryable<Member> query = _db.Members
                .Include(m => m.ECMembers)
                .ThenInclude(em => em.ECPeriod);
            
            if (includeArchived)
            {
                query = query.IgnoreQueryFilters();
            }

            // Apply search filter
            if (!string.IsNullOrWhiteSpace(searchQuery))
            {
                var q = searchQuery.ToLower();
                query = query.Where(m => 
                    (m.FullName != null && m.FullName.ToLower().Contains(q)) ||
                    (m.Email != null && m.Email.ToLower().Contains(q)) ||
                    (m.MembershipNumber != null && m.MembershipNumber.ToLower().Contains(q)));
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

            // Get total count
            var totalItems = await query.CountAsync(cancellationToken);

            // Apply paging
            var membersQuery = query.OrderByDescending(m => m.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            var members = await membersQuery.ToListAsync(cancellationToken);
            var memberDtos = members.Select(member => {
                var dto = new MemberProfileDto
                {
                    Id = member.Id,
                    FullName = member.FullName,
                    Email = member.Email,
                    MobileNo = member.MobileNo,
                    MembershipNumber = member.MembershipNumber,
                    Status = member.Status,
                    GHCLastCertificatePassingYear = member.GHCLastCertificatePassingYear,
                    GHCLastCertificateGroup = member.GHCLastCertificateGroup,
                    GHCLastCertificateSubject = member.GHCLastCertificateSubject,
                    GHCLastCertificate = member.GHCLastCertificate,
                    HighestCertificate = member.HighestCertificate,
                    HighestCertificateGroup = member.HighestCertificateGroup,
                    HighestCertificateSubject = member.HighestCertificateSubject,
                    HighestCertificatePassingYear = member.HighestCertificatePassingYear,
                    ProfessionalSector = member.ProfessionalSector,
                    Designation = member.Designation,
                    PhotoPath = member.PhotoPath,
                    PresentAddress = member.PresentAddress,
                    PermanentAddress = member.PermanentAddress,
                    BloodGroup = member.BloodGroup,
                    MembershipType = member.MembershipType,
                    Category = member.Category,
                    ECPosition = member.ECPosition,
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

                if (member.ECMembers != null && member.ECMembers.Any())
                {
                    dto.ECHistory = member.ECMembers.Select(em => new ECHistoryDto
                    {
                        PeriodTitle = em.ECPeriod?.Title ?? "Unknown",
                        Position = em.Position,
                        StartDate = em.StartDate,
                        EndDate = em.EndDate,
                        ChangeReason = em.ChangeReason,
                        IsCurrent = em.ECPeriod?.IsActive ?? false
                    }).OrderByDescending(h => h.StartDate).ToList();
                }
                return dto;
            });

            return new
            {
                TotalItems = totalItems,
                TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
                CurrentPage = page,
                PageSize = pageSize,
                Items = memberDtos
            };
        }
        public async Task<bool> AdminUpdateMemberAsync(int id, AdminMemberUpdateDto dto, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members.FindAsync(new object[] { id }, cancellationToken);
            if (member == null) return false;

            member.FullName = dto.FullName;
            member.FatherName = dto.FatherName;
            member.MotherName = dto.MotherName;
            member.DateOfBirth = dto.DateOfBirth;
            member.NID = dto.NID;
            member.MobileNo = dto.MobileNo;
            member.Email = dto.Email;
            member.PresentAddress = dto.PresentAddress;
            member.PermanentAddress = dto.PermanentAddress;

            if (Enum.TryParse<Enums.Gender>(dto.Gender, true, out var gender))
                member.Gender = gender;
            
            if (Enum.TryParse<Enums.BloodGroup>(dto.BloodGroup, true, out var blood))
                member.BloodGroup = blood;
            member.HSCAdmissionYear = dto.HSCAdmissionYear;
            member.HighestCertificate = dto.HighestCertificate;
            member.HighestCertificateGroup = dto.HighestCertificateGroup;
            member.HighestCertificateSubject = dto.HighestCertificateSubject;
            member.HighestCertificatePassingYear = dto.HighestCertificatePassingYear;
            member.GHCAdmissionYear = dto.GHCAdmissionYear;
            member.GHCLastCertificate = dto.GHCLastCertificate;
            member.GHCLastCertificateGroup = dto.GHCLastCertificateGroup;
            member.GHCLastCertificateSubject = dto.GHCLastCertificateSubject;
            member.GHCLastCertificatePassingYear = dto.GHCLastCertificatePassingYear;
            member.ProfessionalSector = dto.ProfessionalSector;
            member.Designation = dto.Designation;
            member.MembershipNumber = dto.MembershipNumber;

            if (Enum.TryParse<Enums.MembershipType>(dto.MembershipType, true, out var mType))
                member.MembershipType = mType;

            if (Enum.TryParse<Enums.MemberCategory>(dto.Category, true, out var mCat))
                member.Category = mCat;
            
            // Robust enum parsing (handles names or numeric indices)
            if (Enum.TryParse<Enums.ECPosition>(dto.ECPosition, true, out var ecPos))
            {
                if (member.ECPosition != ecPos)
                {
                    var oldPos = member.ECPosition;
                    member.ECPosition = ecPos;
                    
                    var activePeriod = await _db.ECPeriods.FirstOrDefaultAsync(p => p.IsActive, cancellationToken);
                    if (activePeriod != null)
                    {
                        var now = DateTime.UtcNow;
                        
                        // 1. End all currently active EC records for this member in this period
                        var currentECRecords = await _db.ECMembers
                            .Where(em => em.MemberId == id && em.ECPeriodId == activePeriod.Id && em.EndDate == null)
                            .ToListAsync(cancellationToken);
                        
                        foreach (var record in currentECRecords)
                        {
                            record.EndDate = now;
                            record.ChangeReason = dto.ECChangeReason ?? $"Position changed from {oldPos} to {ecPos}.";
                        }
                        
                        // 2. Start the new position if it's not 'None'
                        if (ecPos != Enums.ECPosition.None)
                        {
                            _db.ECMembers.Add(new ECMember 
                            { 
                                MemberId = id, 
                                ECPeriodId = activePeriod.Id, 
                                Position = ecPos,
                                StartDate = now,
                                ChangeReason = dto.ECChangeReason
                            });
                        }
                    }
                }
            }

            member.IsMobilePublic = dto.IsMobilePublic;
            member.IsEmailPublic = dto.IsEmailPublic;
            member.IsAddressPublic = dto.IsAddressPublic;
            member.LastUpdateDate = DateTime.UtcNow;

            await _db.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Member {MemberId} information updated by Admin", id);
            return true;
        }

        public async Task<bool> SendAdminPasswordResetLinkAsync(int memberId, CancellationToken cancellationToken = default)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.MemberId == memberId, cancellationToken);
            var member = await _db.Members.FindAsync(new object[] { memberId }, cancellationToken);
            
            if (user == null || member == null) return false;

            // Generate a secure token
            var token = Guid.NewGuid().ToString("N");
            user.ResetToken = token;
            user.ResetTokenExpiry = DateTime.UtcNow.AddHours(24);
            await _db.SaveChangesAsync(cancellationToken);

            // Log activity
            await _activityService.LogActivityAsync(memberId, "Password Reset", "Admin initiated password reset email.", cancellationToken: cancellationToken);

            var resetUrl = $"http://localhost:4200/reset-password?email={member.Email}&token={token}";
            
            // Try fetching PASSWORD_RESET template from DB first (database-first strategy)
            var dbTemplate = await _communicationService.GetTemplateByCodeAsync("PASSWORD_RESET", cancellationToken);

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
                subject = "GHCAA Account Password Reset";
                body = $@"
                <div style='font-family: sans-serif; padding: 20px; border: 1px solid #eee; border-radius: 10px; max-width: 600px; margin: auto;'>
                    <h2 style='color: #c5a059;'>Password Reset Initiated</h2>
                    <p>Hello <strong>{member.FullName}</strong>,</p>
                    <p>An administrator has initiated a password reset for your GHCAA account.</p>
                    <p>Please click the button below to set a new password. This link is valid for 24 hours.</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{resetUrl}' style='background: #111; color: #c5a059; padding: 12px 30px; text-decoration: none; border-radius: 8px; font-weight: 800; display: inline-block; border: 1px solid #c5a059;'>Reset My Password</a>
                    </div>
                    <p style='color: #666; font-size: 0.9rem;'>If you did not request this, please ignore this email.</p>
                </div>";
            }

            await _email.SendEmailAsync(member.Email, subject, body);
            return true;
        }
        public async Task<object> GetPublicStatsAsync(CancellationToken cancellationToken = default)
        {
            var count = await _db.Members.CountAsync(m => m.Status == Enums.MembershipStatus.Active, cancellationToken);
            var eventsCount = await _db.AlumniEvents.CountAsync(e => e.IsActive || e.Date < DateTime.UtcNow, cancellationToken);
            
            return new { TotalMembers = count, Countries = 15, Batches = 68, EventsHosted = eventsCount };
        }
    }
}

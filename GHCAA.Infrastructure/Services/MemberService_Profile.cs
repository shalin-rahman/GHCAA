using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GHCAA.Infrastructure.Services
{
    // Profile reads and writes: fetching and updating a member's own profile, the file uploads
    // that hang off it (documents, photo, signature), and the admin-initiated password reset.
    public partial class MemberService
    {
        public async Task<MemberProfileDto?> GetProfileAsync(int memberId, bool isPrivileged = false, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members
                .AsNoTracking()
                .Include(m => m.ECMembers)
                    .ThenInclude(em => em.ECPeriod)
                .Include(m => m.SentFamilyLinkRequests)
                    .ThenInclude(r => r.TargetMember)
                .Include(m => m.ReceivedFamilyLinkRequests)
                    .ThenInclude(r => r.Requester)
                .Include(m => m.AcademicHistory)
                .Include(m => m.ProfessionalHistory)
                .Include(m => m.PaymentHistories)
                .FirstOrDefaultAsync(m => m.Id == memberId, cancellationToken);

            if (member == null) return null;

            var gains = await GetMemberGainsAsync(member.Id, member.ContributionPoints, cancellationToken);
            var registrationPaymentCompleted = member.PaymentHistories != null && member.PaymentHistories.Any(p => p.FinancialCategory == Enums.FinancialCategory.RegistrationFee && p.Status == Enums.PaymentStatus.Completed);
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
                NID = (isPrivileged || member.IsNIDPublic) ? member.NID : (MaskPii(member.NID, 3, 2) ?? string.Empty),
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
                NotifyCommitteeChanges = member.NotifyCommitteeChanges,
                AppliedDate = DateTime.SpecifyKind(member.AppliedDate, DateTimeKind.Utc),
                // Gamification & Health
                ContributionPoints = member.ContributionPoints,
                Rank = gains.rank,
                // 30.28: computed from the SAME 4-item criteria as the member dashboard's
                // "Complete Your Profile" checklist (Identity & Photo / GHC History /
                // Professional Info / Registration Payment), so the percentage bar and the
                // step indicators never disagree.
                ProfileCompletionPercentage = CalculateChecklistProfileCompletion(member, registrationPaymentCompleted),
                IsProfileComplete = member.IsProfileComplete,
                PaymentStatus = registrationPaymentCompleted ? "Completed" : "Pending",
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
            member.NotifyCommitteeChanges = dto.NotifyCommitteeChanges;

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

            // Update IsProfileComplete status
            member.IsProfileComplete = CalculateProfileCompletion(member) >= 100;
            if (member.IsProfileComplete)
            {
                await _activityService.LogActivityAsync(memberId, "Profile Complete", "Member has completed 100% of their profile.", cancellationToken: cancellationToken);
            }

            await _db.SaveChangesAsync(cancellationToken);
            await _activityService.LogActivityAsync(memberId, "Updated", "Member updated profile details and history.", cancellationToken: cancellationToken);
            _logger.LogInformation("Member {MemberId} updated profile and history", memberId);
            return true;
        }

        public async Task<object?> GetMemberDocumentsAsync(int memberId, CancellationToken cancellationToken = default)
        {
            var member = await _db.Members.FindAsync(new object[] { memberId }, cancellationToken);
            // 29C.2: Return null for an unknown id instead of dereferencing a null member (NRE → 500).
            if (member == null) return null;

            return new
            {
                Photo = member.PhotoPath
            };
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

        public async Task<(bool Success, string? ResetUrl)> SendAdminPasswordResetLinkAsync(int memberId, bool isPrivilegedCaller, CancellationToken cancellationToken = default)
        {
            var user = await _db.Users.Include(u => u.Roles).FirstOrDefaultAsync(u => u.MemberId == memberId, cancellationToken);
            var member = await _db.Members.FindAsync(new object[] { memberId }, cancellationToken);

            if (user == null || member == null) return (false, null);

            // A plain Admin must never be able to force a reset link onto a SuperAdmin's own
            // account — paired with the same guard in AdminUpdateMemberAsync, this closes the
            // email-rewrite-then-reset takeover path.
            if (!isPrivilegedCaller && user.Roles.Any(r => r.Name == "SuperAdmin"))
                throw new UnauthorizedAccessException("Only a SuperAdmin may reset a SuperAdmin's own password.");

            // Generate a secure token
            var token = Guid.NewGuid().ToString("N");
            user.ResetToken = token;
            user.ResetTokenExpiry = DateTime.UtcNow.AddHours(24);
            await _db.SaveChangesAsync(cancellationToken);

            // An admin-initiated reset otherwise leaves any refresh token issued before it still
            // valid, so a session taken over before the reset survives the reset.
            await _tokenService.RevokeAllRefreshTokensAsync(user.Id, cancellationToken);

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
                var config = await _orgConfigService.GetConfigAsync();
                var locale = config.Localization.Locales.TryGetValue(config.Localization.DefaultLocale, out var lp) ? lp : null;
                subject = locale?.EmailSubjects.PasswordReset ?? "GHCAA Account Password Reset";

                body = $@"
                <div style='font-family: sans-serif; padding: 20px; border: 1px solid #eee; border-radius: 10px; max-width: 600px; margin: auto;'>
                    <h2 style='color: #c5a059;'>Password Reset Initiated</h2>
                    <p>Hello <strong>{member.FullName}</strong>,</p>
                    <p>An administrator has initiated a password reset for your {config.Branding.ShortName} account.</p>
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
    }
}

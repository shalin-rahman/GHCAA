using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using static GHCAA.Domain.Enums;

namespace GHCAA.Application.DTOs
{
    /// <summary>
    /// Shared base for member creation and profile update DTOs.
    /// All [Required] and length rules are enforced here and inherited automatically.
    /// </summary>
    public abstract class BaseMemberDto
    {
        // ── Personal ─────────────────────────────────────────────────────────────
        [Required(ErrorMessage = "Full name is required.")]
        [MaxLength(200, ErrorMessage = "Full name must not exceed 200 characters.")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Father's name is required.")]
        [MaxLength(200, ErrorMessage = "Father's name must not exceed 200 characters.")]
        public string FatherName { get; set; } = null!;

        [Required(ErrorMessage = "Mother's name is required.")]
        [MaxLength(200, ErrorMessage = "Mother's name must not exceed 200 characters.")]
        public string MotherName { get; set; } = null!;

        [Required(ErrorMessage = "Date of birth is required.")]
        public DateTime DateOfBirth { get; set; }

        [Required(ErrorMessage = "National ID (NID) is required.")]
        [MinLength(6, ErrorMessage = "NID must be at least 6 characters.")]
        [MaxLength(20, ErrorMessage = "NID must not exceed 20 characters.")]
        public string NID { get; set; } = null!;

        [Required(ErrorMessage = "Mobile number is required.")]
        [RegularExpression(@"^(\+88)?01[3-9][\s-]*\d{8}$", ErrorMessage = "Enter a valid Bangladeshi mobile number (e.g. 01XXXXXXXXX).")]
        public string MobileNo { get; set; } = null!;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "A valid email address is required.")]
        [MaxLength(200, ErrorMessage = "Email must not exceed 200 characters.")]
        public string Email { get; set; } = null!;

        public Gender Gender { get; set; } = Gender.None;
        public BloodGroup BloodGroup { get; set; } = BloodGroup.Unknown;

        // ── Address ───────────────────────────────────────────────────────────────
        [Required(ErrorMessage = "Present address is required.")]
        [MaxLength(500, ErrorMessage = "Address must not exceed 500 characters.")]
        public string PresentAddress { get; set; } = null!;

        [Required(ErrorMessage = "Permanent address is required.")]
        [MaxLength(500, ErrorMessage = "Address must not exceed 500 characters.")]
        public string PermanentAddress { get; set; } = null!;

        [MaxLength(10)]
        public string? TShirtSize { get; set; }

        // ── Emergency contact ─────────────────────────────────────────────────────
        [Required(ErrorMessage = "Emergency contact name is required.")]
        [MaxLength(200, ErrorMessage = "Emergency contact name must not exceed 200 characters.")]
        public string EmergencyContactName { get; set; } = null!;

        [Required(ErrorMessage = "Emergency contact relation is required.")]
        [MaxLength(100, ErrorMessage = "Relation must not exceed 100 characters.")]
        public string EmergencyContactRelation { get; set; } = null!;

        [Required(ErrorMessage = "Emergency contact phone is required.")]
        [RegularExpression(@"^(\+88)?01[3-9][\s-]*\d{8}$", ErrorMessage = "Enter a valid Bangladeshi mobile number for the emergency contact.")]
        public string EmergencyContactPhone { get; set; } = null!;

        // ── Privacy flags ─────────────────────────────────────────────────────────
        public bool IsMobilePublic { get; set; }
        public bool IsEmailPublic { get; set; }
        public bool IsAddressPublic { get; set; }
        public bool IsNIDPublic { get; set; }
        public DateTime? LastUpdateDate { get; set; }

        // Notification Preferences
        public bool NotifyEventCreation { get; set; } = true;
        public bool NotifyParticipationApproval { get; set; } = true;
        public bool NotifyRegistrationUpdate { get; set; } = true;
        public bool NotifyRelevantUpdates { get; set; } = true;

        // Flattened Academic/Professional (for high-level UI/Imports/Directory)
        public string? HighestCertificate { get; set; }
        public string? HighestCertificateGroup { get; set; }
        public string? HighestCertificateSubject { get; set; }
        public int? HighestCertificatePassingYear { get; set; }
        public int? HSCAdmissionYear { get; set; }

        public string? GHCLastCertificate { get; set; }
        public string? GHCLastCertificateGroup { get; set; }
        public string? GHCLastCertificateSubject { get; set; }
        public int? GHCLastCertificatePassingYear { get; set; }
        public int? GHCAdmissionYear { get; set; }

        public string? ProfessionalSector { get; set; }
        public string? Designation { get; set; }
        public string? OrganizationName { get; set; }
        public string? Location { get; set; } // Current job location

        // Summary Path (Flattened for UI convenience)
        public string? CertificatePath { get; set; }
        public string? PaymentProofPath { get; set; }

        // ── Attachments & history ─────────────────────────────────────────────────
        public string? PhotoPath { get; set; }
        public string? SignaturePath { get; set; }

        [MinLength(1, ErrorMessage = "At least one academic record is required.")]
        public List<AcademicRecordDto> AcademicHistory { get; set; } = new();

        public List<ProfessionalRecordDto> ProfessionalHistory { get; set; } = new();
        public List<ECHistoryDto> ECHistory { get; set; } = new();
    }
}

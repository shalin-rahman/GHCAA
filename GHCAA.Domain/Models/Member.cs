using System;
using static GHCAA.Domain.Enums;

namespace GHCAA.Domain.Models
{
    public class Member
    {
        public int Id { get; set; }

        // A. Personal (2.1 A)
        public string FullName { get; set; } = null!;
        public string FatherName { get; set; } = null!;
        public string MotherName { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public BloodGroup BloodGroup { get; set; }
        public string NID { get; set; } = null!; // Numeric: 10, 13, or 17 digits
        public string MobileNo { get; set; } = null!; // 11 digits
        public string Email { get; set; } = null!; // Verified via OTP
        public bool EmailVerified { get; set; } = false;
        public string PresentAddress { get; set; } = null!;
        public string PermanentAddress { get; set; } = null!;
        public string EmergencyContactName { get; set; } = null!;
        public string EmergencyContactRelation { get; set; } = null!;
        public string EmergencyContactPhone { get; set; } = null!;

        // B. Academic (2.1 B)
        public int HSCAdmissionYear { get; set; } // 1950 - Current
        public int GHCAdmissionYear { get; set; } // Separated from HSC
        public string LastCertificateFromGHC { get; set; } = "Other"; // Dynamic lookup: HSC, Bachelor, etc.
        public string SubjectGroup { get; set; } = null!;
        public int GHCLastCertificatePassingYear { get; set; } // 1950 - Current

        // C. Professional (2.1 C)
        public string ProfessionalSector { get; set; } = null!;
        public string Designation { get; set; } = null!; // Role, Organization, Location

        // D. Attachments & System Logic (2.1 D & Section 4)
        public string? PhotoPath { get; set; }
        public string? CertificatePath { get; set; }
        public string? PaymentProofPath { get; set; }

        // Workflow & Status
        public MembershipStatus Status { get; set; }
        public DateTime AppliedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public int? ApprovedBy { get; set; } // Admin ID
        public string? MembershipNumber { get; set; }

        // Privacy Settings
        public bool IsMobilePublic { get; set; } = false;
        public bool IsEmailPublic { get; set; } = false;
        public bool IsAddressPublic { get; set; } = false;

        public bool IsArchived { get; set; } = false;
        public DateTime LastUpdateDate { get; set; } = DateTime.UtcNow;

        // Membership details
        public MembershipType MembershipType { get; set; } = MembershipType.General;
        public MemberCategory Category { get; set; } = MemberCategory.None;
        public ECPosition ECPosition { get; set; } = ECPosition.None;

        // Navigation
        public User? User { get; set; }
    }
}

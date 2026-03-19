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

        // D. Attachments & System Logic (2.1 D & Section 4)
        public string? PhotoPath { get; set; }
        public string? TShirtSize { get; set; }

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
        public bool IsNIDPublic { get; set; } = false;
        public bool IsFamilyPublic { get; set; } = false;
        public bool HasAcceptedTerms { get; set; } = false;

        public bool IsArchived { get; set; } = false;
        public DateTime LastUpdateDate { get; set; } = DateTime.UtcNow;

        // Membership details
        public MembershipType MembershipType { get; set; } = MembershipType.General;
        public MemberCategory Category { get; set; } = MemberCategory.None;
        public bool IsVerified { get; set; } = false; // Blue Tick / Verification Status

        // Navigation
        public User? User { get; set; }
        public ICollection<ECMember> ECMembers { get; set; } = new List<ECMember>();
        public ICollection<AcademicRecord> AcademicHistory { get; set; } = new List<AcademicRecord>();
        public ICollection<ProfessionalRecord> ProfessionalHistory { get; set; } = new List<ProfessionalRecord>();
        public ICollection<PaymentHistory> PaymentHistories { get; set; } = new List<PaymentHistory>();

        // Family link requests
        public ICollection<FamilyLinkRequest> SentFamilyLinkRequests { get; set; } = new List<FamilyLinkRequest>();
        public ICollection<FamilyLinkRequest> ReceivedFamilyLinkRequests { get; set; } = new List<FamilyLinkRequest>();
    }
}

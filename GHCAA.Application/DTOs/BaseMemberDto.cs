using System;
using System.Collections.Generic;
using static GHCAA.Domain.Enums;

namespace GHCAA.Application.DTOs
{
    public abstract class BaseMemberDto
    {
        public string FullName { get; set; } = null!;
        public string FatherName { get; set; } = null!;
        public string MotherName { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public string NID { get; set; } = null!;
        public string MobileNo { get; set; } = null!;
        public string Email { get; set; } = null!;
        public Gender Gender { get; set; } = Gender.Male;
        public BloodGroup BloodGroup { get; set; } = BloodGroup.APositive;
        public MembershipType MembershipType { get; set; } = MembershipType.General;

        public string PresentAddress { get; set; } = null!;
        public string PermanentAddress { get; set; } = null!;
        public string? TShirtSize { get; set; }

        public string EmergencyContactName { get; set; } = null!;
        public string EmergencyContactRelation { get; set; } = null!;
        public string EmergencyContactPhone { get; set; } = null!;

        // Privacy
        public bool IsMobilePublic { get; set; }
        public bool IsEmailPublic { get; set; }
        public bool IsAddressPublic { get; set; }
        public bool IsNIDPublic { get; set; }
        public bool HasAcceptedTerms { get; set; }
        public bool HasAcceptedGdpr { get; set; }

        // Attachments
        public string? PhotoPath { get; set; }

        // History
        public List<AcademicRecordDto> AcademicHistory { get; set; } = new();
        public List<ProfessionalRecordDto> ProfessionalHistory { get; set; } = new();
        public List<ECHistoryDto> ECHistory { get; set; } = new();

        // Payment (Refined)
        public int PaymentMethodId { get; set; }
        public string? TransactionId { get; set; }
    }
}

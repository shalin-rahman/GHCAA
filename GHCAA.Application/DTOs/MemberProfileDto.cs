using static GHCAA.Domain.Enums;

namespace GHCAA.Application.DTOs
{
    public class MemberProfileDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string MobileNo { get; set; } = null!;
        public string? MembershipNumber { get; set; }
        public MembershipStatus Status { get; set; }
        public MembershipType MembershipType { get; set; }
        public MemberCategory Category { get; set; }
        public ECPosition ECPosition { get; set; }
        
        // Personal
        public string FatherName { get; set; } = null!;
        public string MotherName { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public Gender Gender { get; set; }
        public BloodGroup BloodGroup { get; set; }
        public string NID { get; set; } = null!;
        public string EmergencyContactName { get; set; } = null!;
        public string EmergencyContactRelation { get; set; } = null!;
        public string EmergencyContactPhone { get; set; } = null!;

        // Academic
        public int? HSCAdmissionYear { get; set; }
        public string HighestCertificate { get; set; } = null!;
        public string HighestCertificateGroup { get; set; } = null!;
        public string HighestCertificateSubject { get; set; } = null!;
        public int HighestCertificatePassingYear { get; set; }

        public int? GHCAdmissionYear { get; set; }
        public string GHCLastCertificate { get; set; } = null!;
        public string GHCLastCertificateGroup { get; set; } = null!;
        public string GHCLastCertificateSubject { get; set; } = null!;
        public int GHCLastCertificatePassingYear { get; set; }
        
        // Professional
        public string ProfessionalSector { get; set; } = null!;
        public string Designation { get; set; } = null!;
        
        // Info
        public string? PhotoPath { get; set; }
        public string? CertificatePath { get; set; }
        public string PresentAddress { get; set; } = null!;
        public string PermanentAddress { get; set; } = null!;
        
        // Privacy
        public bool IsMobilePublic { get; set; }
        public bool IsEmailPublic { get; set; }
        public bool IsAddressPublic { get; set; }
        
        public List<ECHistoryDto> ECHistory { get; set; } = new();
    }

    public class ECHistoryDto
    {
        public string PeriodTitle { get; set; } = null!;
        public ECPosition Position { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? ChangeReason { get; set; }
        public bool IsCurrent { get; set; }
    }
}

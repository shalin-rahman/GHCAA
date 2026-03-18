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
        public DateTime AppliedDate { get; set; }
        public DateTime? ApprovedDate { get; set; }
        
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
        public string? PaymentProofPath { get; set; }
        public string PresentAddress { get; set; } = null!;
        public string PermanentAddress { get; set; } = null!;
        
        // Privacy
        public bool IsMobilePublic { get; set; }
        public bool IsEmailPublic { get; set; }
        public bool IsAddressPublic { get; set; }
        public bool IsNIDPublic { get; set; }
        public bool HasAcceptedTerms { get; set; }
        
        public List<ECHistoryDto> ECHistory { get; set; } = new();
        public List<AcademicRecordDto> AcademicHistory { get; set; } = new();
        public List<ProfessionalRecordDto> ProfessionalHistory { get; set; } = new();
    }

    public class AcademicRecordDto
    {
        public int Id { get; set; }
        public string InstitutionName { get; set; } = null!;
        public string Degree { get; set; } = null!;
        public string Subject { get; set; } = null!;
        public int? AdmissionYear { get; set; }
        public int PassingYear { get; set; }
        public bool IsGHC { get; set; }
        public string? Result { get; set; }
    }

    public class ProfessionalRecordDto
    {
        public int Id { get; set; }
        public string OrganizationName { get; set; } = null!;
        public string Designation { get; set; } = null!;
        public string? Sector { get; set; }
        public string? Location { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCurrent { get; set; }
    }

    public class ECHistoryDto
    {
        public int Id { get; set; }
        public int PeriodId { get; set; }
        public string PeriodTitle { get; set; } = null!;
        public ECPosition Position { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? ChangeReason { get; set; }
        public bool IsCurrent { get; set; }
    }

    public class MemberSummaryDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;
        public string? MembershipNumber { get; set; }
        public string? PhotoPath { get; set; }
        public MembershipStatus Status { get; set; }
        public DateTime AppliedDate { get; set; }
        public int PassingYear { get; set; }
        public int GhcLastCertificatePassingYear { get; set; }
        public string? GhcLastCertificate { get; set; }
        public string? GhcLastCertificateGroup { get; set; }
        public string? GhcLastCertificateSubject { get; set; }
        public string? ProfessionalSector { get; set; }
        public string? Designation { get; set; }
        public BloodGroup BloodGroup { get; set; }
        public string? Email { get; set; }
        public bool IsEmailPublic { get; set; }
        public string? MobileNo { get; set; }
        public bool IsMobilePublic { get; set; }
        public bool IsAddressPublic { get; set; }
        public string? NID { get; set; }
        public bool IsNIDPublic { get; set; }
        public MembershipType MembershipType { get; set; }
        public MemberCategory Category { get; set; }
        public List<ECHistoryDto> ECHistory { get; set; } = new();
    }
}

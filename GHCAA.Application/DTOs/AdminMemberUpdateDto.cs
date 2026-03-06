using System;

namespace GHCAA.Application.DTOs
{
    public class AdminMemberUpdateDto
    {
        // Identification
        public string FullName { get; set; } = null!;
        public string FatherName { get; set; } = null!;
        public string MotherName { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public string NID { get; set; } = null!;
        public string MobileNo { get; set; } = null!;
        public string Email { get; set; } = null!;

        // Address
        public string PresentAddress { get; set; } = null!;
        public string PermanentAddress { get; set; } = null!;

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

        // Membership Status & Type
        public string MembershipType { get; set; } = null!;
        public string Category { get; set; } = null!;
        public string ECPosition { get; set; } = null!;
        public string? MembershipNumber { get; set; }
        
        // Privacy
        public bool IsMobilePublic { get; set; }
        public bool IsEmailPublic { get; set; }
        public bool IsAddressPublic { get; set; }
        
        public string? ECChangeReason { get; set; }
    }
}

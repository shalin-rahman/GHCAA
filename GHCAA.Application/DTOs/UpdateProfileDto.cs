using GHCAA.Domain;
namespace GHCAA.Application.DTOs
{
    public class UpdateProfileDto
    {
        public string FullName { get; set; } = null!;
        public string FatherName { get; set; } = null!;
        public string MotherName { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public Enums.Gender Gender { get; set; }
        public Enums.BloodGroup BloodGroup { get; set; }

        public string PresentAddress { get; set; } = null!;
        public string PermanentAddress { get; set; } = null!;
        public string ProfessionalSector { get; set; } = null!;
        public string Designation { get; set; } = null!;

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
        
        // Emergency Contact
        public string EmergencyContactName { get; set; } = null!;
        public string EmergencyContactRelation { get; set; } = null!;
        public string EmergencyContactPhone { get; set; } = null!;

        // Privacy Settings
        public bool IsMobilePublic { get; set; }
        public bool IsEmailPublic { get; set; }
        public bool IsAddressPublic { get; set; }
    }
}

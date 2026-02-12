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
        
        // Academic
        public int GHCLastCertificatePassingYear { get; set; }
        public string LastCertificateFromGHC { get; set; } = null!;
        public string SubjectGroup { get; set; } = null!;
        
        // Professional
        public string ProfessionalSector { get; set; } = null!;
        public string Designation { get; set; } = null!;
        
        // Info
        public string? PhotoPath { get; set; }
        public string PresentAddress { get; set; } = null!;
        public string PermanentAddress { get; set; } = null!;
        public BloodGroup BloodGroup { get; set; }
        
        // Privacy
        public bool IsMobilePublic { get; set; }
        public bool IsEmailPublic { get; set; }
        public bool IsAddressPublic { get; set; }
    }
}

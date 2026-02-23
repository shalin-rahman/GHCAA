namespace GHCAA.Application.DTOs
{
    public class UpdateProfileDto
    {
        public string PresentAddress { get; set; } = null!;
        public string PermanentAddress { get; set; } = null!;
        public string ProfessionalSector { get; set; } = null!;
        public string Designation { get; set; } = null!;

        // Academic
        public string SubjectGroup { get; set; } = null!;
        public string LastDegreeFromGHC { get; set; } = null!;
        public int GHCLastCertificatePassingYear { get; set; }
        
        // Privacy Settings
        public bool IsMobilePublic { get; set; }
        public bool IsEmailPublic { get; set; }
        public bool IsAddressPublic { get; set; }
    }
}

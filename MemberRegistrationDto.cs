GHCAA.Application\DTOs\MemberRegistrationDto.cs
using System;

namespace GHCAA.Application.DTOs
{
    public class MemberRegistrationDto
    {
        // Personal
        public string FullName { get; set; } = null!;
        public string FatherName { get; set; } = null!;
        public string MotherName { get; set; } = null!;
        public DateTime DateOfBirth { get; set; }
        public string Gender { get; set; } = null!; // client will use dropdown values
        public string BloodGroup { get; set; } = null!;
        public string NID { get; set; } = null!;
        public string MobileNo { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PresentAddress { get; set; } = null!;
        public string PermanentAddress { get; set; } = null!;
        public string EmergencyContactName { get; set; } = null!;
        public string EmergencyContactRelation { get; set; } = null!;
        public string EmergencyContactPhone { get; set; } = null!;

        // Academic
        public int HSCAdmissionYear { get; set; }
        public int GHCAdmissionYear { get; set; }
        public string LastDegreeFromGHC { get; set; } = null!;
        public string SubjectGroup { get; set; } = null!;
        public int GHCLastCertificatePassingYear { get; set; }

        // Professional
        public string ProfessionalSector { get; set; } = null!;
        public string Designation { get; set; } = null!;
    }
}
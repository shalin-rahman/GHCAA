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

        public string PresentAddress { get; set; } = null!;
        public string PermanentAddress { get; set; } = null!;

        // Academic - Common
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

        // Professional - Common
        public string ProfessionalSector { get; set; } = null!;
        public string Designation { get; set; } = null!;

        // Privacy
        public bool IsMobilePublic { get; set; }
        public bool IsEmailPublic { get; set; }
        public bool IsAddressPublic { get; set; }
        public bool HasAcceptedTerms { get; set; }

        // Attachments
        public string? PhotoPath { get; set; }

        // History
        public List<AcademicRecordDto> AcademicHistory { get; set; } = new();
        public List<ProfessionalRecordDto> ProfessionalHistory { get; set; } = new();
    }
}

using System;

namespace GHCAA.Application.DTOs
{
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
        public string? CertificatePath { get; set; }
    }
}

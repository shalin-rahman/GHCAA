using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GHCAA.Application.DTOs
{
    public class AcademicRecordDto : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Institution name is required.")]
        [MaxLength(250, ErrorMessage = "Institution name must not exceed 250 characters.")]
        public string InstitutionName { get; set; } = null!;

        [Required(ErrorMessage = "Degree is required.")]
        [MaxLength(100, ErrorMessage = "Degree must not exceed 100 characters.")]
        public string Degree { get; set; } = null!;

        [Required(ErrorMessage = "Subject/Major is required.")]
        [MaxLength(100, ErrorMessage = "Subject must not exceed 100 characters.")]
        public string Subject { get; set; } = null!;

        [Range(1900, 2100, ErrorMessage = "Admission year must be between 1900 and 2100.")]
        public int? AdmissionYear { get; set; }

        [Required(ErrorMessage = "Passing year is required.")]
        [Range(1900, 2100, ErrorMessage = "Passing year must be between 1900 and 2100.")]
        public int? PassingYear { get; set; }

        public bool IsGHC { get; set; }

        [MaxLength(100, ErrorMessage = "Result must not exceed 100 characters.")]
        public string? Result { get; set; }

        public string? CertificatePath { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (AdmissionYear.HasValue && PassingYear < AdmissionYear.Value)
            {
                yield return new ValidationResult(
                    "Passing year cannot be earlier than admission year.",
                    new[] { nameof(PassingYear) });
            }
        }
    }
}

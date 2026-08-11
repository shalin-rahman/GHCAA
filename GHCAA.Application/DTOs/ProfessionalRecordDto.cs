using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GHCAA.Application.DTOs
{
    public class ProfessionalRecordDto : IValidatableObject
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Organization name is required.")]
        [MaxLength(250, ErrorMessage = "Organization name must not exceed 250 characters.")]
        public string OrganizationName { get; set; } = null!;

        [Required(ErrorMessage = "Designation is required.")]
        [MaxLength(150, ErrorMessage = "Designation must not exceed 150 characters.")]
        public string Designation { get; set; } = null!;

        [MaxLength(100, ErrorMessage = "Sector must not exceed 100 characters.")]
        public string? Sector { get; set; }

        [MaxLength(200, ErrorMessage = "Location must not exceed 200 characters.")]
        public string? Location { get; set; }

        [Required(ErrorMessage = "Start date is required.")]
        public DateTime StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public bool IsCurrent { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!IsCurrent && !EndDate.HasValue)
            {
                yield return new ValidationResult(
                    "End date is required if the job is not current.",
                    new[] { nameof(EndDate) });
            }

            if (EndDate.HasValue && EndDate.Value < StartDate)
            {
                yield return new ValidationResult(
                    "End date cannot be earlier than start date.",
                    new[] { nameof(EndDate) });
            }
        }
    }
}

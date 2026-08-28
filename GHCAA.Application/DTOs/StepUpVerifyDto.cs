using System.ComponentModel.DataAnnotations;

namespace GHCAA.Application.DTOs
{
    public class StepUpVerifyDto
    {
        [Required(ErrorMessage = "Verification code is required.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "Verification code must be 6 digits.")]
        public string Code { get; set; } = null!;
    }
}

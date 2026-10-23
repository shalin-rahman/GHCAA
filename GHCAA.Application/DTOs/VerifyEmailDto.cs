using System.ComponentModel.DataAnnotations;

namespace GHCAA.Application.DTOs
{
    public class VerifyEmailDto
    {
        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "A valid email address is required.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "OTP code is required.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "OTP code must be exactly 6 digits.")]
        public string OtpCode { get; set; } = null!;
    }
}

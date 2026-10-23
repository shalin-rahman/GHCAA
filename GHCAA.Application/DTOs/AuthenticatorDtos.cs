using System.ComponentModel.DataAnnotations;

namespace GHCAA.Application.DTOs
{
    // 95.6. Authenticator app enrolment for step-up.
    // SetupNeedsPassword is true for a user with no email on file. They cannot pass an email
    // step-up, so setup asks for their password instead.
    public record AuthenticatorStatusDto(bool Enrolled, bool Pending, int RecoveryCodesLeft, bool SetupNeedsPassword = false);

    // Returned once, at setup. The secret is shown for people who cannot scan the QR code.
    public record AuthenticatorSetupDto(string Secret, string OtpAuthUri, string QrCodeDataUri);

    public record RecoveryCodesDto(IReadOnlyList<string> Codes);

    public class AuthenticatorCodeDto
    {
        [Required(ErrorMessage = "Enter the 6-digit code from your authenticator app.")]
        [RegularExpression(@"^\d{6}$", ErrorMessage = "The code must be 6 digits.")]
        public string Code { get; set; } = null!;
    }

    public class AuthenticatorPasswordDto
    {
        [Required(ErrorMessage = "Enter your password.")]
        public string Password { get; set; } = null!;
    }
}

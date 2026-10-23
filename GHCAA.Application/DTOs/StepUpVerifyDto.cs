using GHCAA.Domain;
using System.ComponentModel.DataAnnotations;

namespace GHCAA.Application.DTOs
{
    public class StepUpVerifyDto : IValidatableObject
    {
        // 95.6: an app recovery code (XXXXX-XXXXX) is also let through here. The email path
        // still needs 6 digits, which Validate checks.
        [Required(ErrorMessage = "Verification code is required.")]
        [RegularExpression(@"^(\d{6}|[A-Za-z0-9]{5}-?[A-Za-z0-9]{5})$", ErrorMessage = "Verification code must be 6 digits, or a recovery code.")]
        public string Code { get; set; } = null!;

        // "email" (the default) or "app" for an authenticator app or recovery code.
        [RegularExpression("^(email|app)$", ErrorMessage = "Method must be email or app.")]
        public string? Method { get; set; }

        public bool UsesApp => Method == Constants.Authenticator.MethodApp;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!UsesApp && Code != null && !(Code.Length == 6 && Code.All(char.IsAsciiDigit)))
                yield return new ValidationResult("Verification code must be 6 digits.", [nameof(Code)]);
        }
    }
}

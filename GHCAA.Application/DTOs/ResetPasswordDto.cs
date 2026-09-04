using System.ComponentModel.DataAnnotations;

namespace GHCAA.Application.DTOs
{
    public class ResetPasswordDto
    {
        // A member's link carries their email here; a system admin has no Member/email, so their
        // link carries their Username instead (see AuthService.ResetPasswordAsync). Same field,
        // two account kinds, hence no [EmailAddress] format check.
        [Required(ErrorMessage = "Email or username is required.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Reset token is required.")]
        public string Token { get; set; } = null!;

        [Required(ErrorMessage = "New password is required.")]
        [MinLength(8, ErrorMessage = "Password must be at least 8 characters.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
            ErrorMessage = "Password must contain at least one uppercase letter, one lowercase letter, and one digit.")]
        public string NewPassword { get; set; } = null!;
    }
}

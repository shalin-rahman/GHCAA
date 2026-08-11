using System.ComponentModel.DataAnnotations;

namespace GHCAA.Application.DTOs
{
    public class ContactMessageDto
    {
        [Required(ErrorMessage = "Full name is required.")]
        [MaxLength(150, ErrorMessage = "Name must not exceed 150 characters.")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Email address is required.")]
        [EmailAddress(ErrorMessage = "A valid email address is required.")]
        [MaxLength(200, ErrorMessage = "Email must not exceed 200 characters.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Subject is required.")]
        [MaxLength(250, ErrorMessage = "Subject must not exceed 250 characters.")]
        public string Subject { get; set; } = null!;

        [Required(ErrorMessage = "Message body is required.")]
        [MinLength(10, ErrorMessage = "Message must be at least 10 characters.")]
        [MaxLength(2000, ErrorMessage = "Message must not exceed 2000 characters.")]
        public string Message { get; set; } = null!;
    }
}

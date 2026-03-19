using System.ComponentModel.DataAnnotations;

namespace GHCAA.Application.DTOs
{
    public class ResendOtpDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
    }
}

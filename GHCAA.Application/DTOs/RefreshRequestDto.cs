using System.ComponentModel.DataAnnotations;

namespace GHCAA.Application.DTOs
{
    public class RefreshRequestDto
    {
        [Required(ErrorMessage = "Refresh token is required.")]
        public string RefreshToken { get; set; } = null!;
    }
}

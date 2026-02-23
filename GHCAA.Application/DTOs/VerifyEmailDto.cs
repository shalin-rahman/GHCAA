namespace GHCAA.Application.DTOs
{
    public class VerifyEmailDto
    {
        public string Email { get; set; } = null!;
        public string OtpCode { get; set; } = null!;
    }
}

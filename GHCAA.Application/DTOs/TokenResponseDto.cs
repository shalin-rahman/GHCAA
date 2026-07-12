namespace GHCAA.Application.DTOs
{
    public class TokenResponseDto
    {
        public string Token { get; set; } = null!;
        public string Username { get; set; } = null!;
        public int? MemberId { get; set; }
        public string Role { get; set; } = "Member";
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? MobileNo { get; set; }
        public bool MustChangePassword { get; set; }
        public string? RefreshToken { get; set; }
    }
}

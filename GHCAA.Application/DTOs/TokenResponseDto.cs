namespace GHCAA.Application.DTOs
{
    public class TokenResponseDto
    {
        public string Token { get; set; } = null!;
        public string Username { get; set; } = null!;
        public int? MemberId { get; set; }
    }
}

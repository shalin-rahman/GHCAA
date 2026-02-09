namespace GHCAA.Application.DTOs
{
    public class MemberRegistrationResultDto
    {
        public int MemberId { get; set; }
        public string Message { get; set; } = null!;
        public bool EmailSent { get; set; }
    }
}
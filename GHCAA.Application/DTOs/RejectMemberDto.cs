namespace GHCAA.Application.DTOs
{
    public class RejectMemberDto
    {
        public int RejectedByAdminId { get; set; }
        public string Reason { get; set; } = null!;
    }
}

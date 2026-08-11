namespace GHCAA.Application.DTOs
{
    // Admin identity is resolved from the JWT MemberId claim server-side (see AdminController.RejectMember),
    // not the request body.
    public class RejectMemberDto
    {
        public string Reason { get; set; } = null!;
    }
}

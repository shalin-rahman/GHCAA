namespace GHCAA.Application.DTOs
{
    // Body carries no admin identity — the acting admin is resolved from the JWT MemberId claim
    // server-side (see AdminController.ApproveMember). Kept as the [FromBody] binding target.
    public class ApproveMemberDto
    {
    }
}

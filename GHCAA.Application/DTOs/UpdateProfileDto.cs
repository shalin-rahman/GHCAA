using GHCAA.Domain;
using static GHCAA.Domain.Enums;

namespace GHCAA.Application.DTOs
{
    public class UpdateProfileDto : BaseMemberDto
    {
        public Gender Gender { get; set; }
        public BloodGroup BloodGroup { get; set; }

        public string EmergencyContactName { get; set; } = null!;
        public string EmergencyContactRelation { get; set; } = null!;
        public string EmergencyContactPhone { get; set; } = null!;
    }
}

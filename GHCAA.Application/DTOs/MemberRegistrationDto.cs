using System;

namespace GHCAA.Application.DTOs
{
    public class MemberRegistrationDto : BaseMemberDto
    {
        public string Gender { get; set; } = null!;
        public string BloodGroup { get; set; } = null!;
        
        public string EmergencyContactName { get; set; } = null!;
        public string EmergencyContactRelation { get; set; } = null!;
        public string EmergencyContactPhone { get; set; } = null!;
    }
}
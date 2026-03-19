using System;

namespace GHCAA.Application.DTOs
{
    public class ProfessionalRecordDto
    {
        public int Id { get; set; }
        public string OrganizationName { get; set; } = null!;
        public string Designation { get; set; } = null!;
        public string? Sector { get; set; }
        public string? Location { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsCurrent { get; set; }
    }
}

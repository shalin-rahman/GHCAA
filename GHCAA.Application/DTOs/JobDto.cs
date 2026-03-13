using System;
using GHCAA.Domain;

namespace GHCAA.Application.DTOs
{
    public class JobDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string CompanyName { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Requirements { get; set; } = null!;
        public string? ApplicationEmail { get; set; }
        public string? ApplicationLink { get; set; }
        public DateTime PostedDate { get; set; }
        public DateTime? ApplicationDeadline { get; set; }
        public Enums.JobCategory Category { get; set; }
        public bool IsActive { get; set; }
        public int PostedByMemberId { get; set; }
        public string? PostedByMemberName { get; set; }
    }

    public class CreateJobDto
    {
        public string Title { get; set; } = null!;
        public string CompanyName { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Requirements { get; set; } = null!;
        public string? ApplicationEmail { get; set; }
        public string? ApplicationLink { get; set; }
        public DateTime? ApplicationDeadline { get; set; }
        public Enums.JobCategory Category { get; set; }
    }
}

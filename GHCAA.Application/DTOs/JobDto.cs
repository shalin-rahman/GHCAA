using System.ComponentModel.DataAnnotations;
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
        public Enums.JobCategory JobCategory { get; set; }
        public bool IsActive { get; set; }
        public int PostedByMemberId { get; set; }
        public string? PostedByMemberName { get; set; }
        public Enums.SubmissionStatus Status { get; set; }
        public string? RejectionReason { get; set; }
    }

    // Only Title and CompanyName are required: the mobile form sends no Requirements and may leave
    // Location and Description empty. The optional text fields must stay string?, because with
    // nullable enabled MVC treats a non-nullable string as [Required]. No [EmailAddress], because
    // the web form sends "" for no email.
    public class CreateJobDto
    {
        [Required, StringLength(200)]
        public string Title { get; set; } = null!;
        [Required, StringLength(200)]
        public string CompanyName { get; set; } = null!;
        [StringLength(200)]
        public string? Location { get; set; }
        [StringLength(4000)]
        public string? Description { get; set; }
        [StringLength(4000)]
        public string? Requirements { get; set; }
        [StringLength(256)]
        public string? ApplicationEmail { get; set; }
        [StringLength(500)]
        public string? ApplicationLink { get; set; }
        public DateTime? ApplicationDeadline { get; set; }
        public Enums.JobCategory JobCategory { get; set; }

        // 82.52: gates the "Job Posted" notification PostJobAsync fires when an
        // admin's posting auto-approves. Default true matches that this already
        // notified unconditionally before this flag existed.
        public bool NotifyMembers { get; set; } = true;
    }
}

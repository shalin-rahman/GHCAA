using System;
using System.ComponentModel.DataAnnotations;

namespace GHCAA.Domain.Models
{
    public class MentorshipRequest
    {
        public int Id { get; set; }

        [Required]
        public int RequesterId { get; set; }
        public Member? Requester { get; set; }

        [Required]
        public int MentorId { get; set; }
        public Member? Mentor { get; set; }

        [MaxLength(500)]
        public string? Message { get; set; }

        /// <summary>Area/domain the mentee is seeking guidance in (e.g., "Software Engineering", "Civil Service").</summary>
        [MaxLength(200)]
        public string? Domain { get; set; }

        public MentorshipStatus Status { get; set; } = MentorshipStatus.Pending;

        [MaxLength(500)]
        public string? ResponseNote { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RespondedAt { get; set; }
    }

    public enum MentorshipStatus
    {
        Pending,
        Accepted,
        Declined,
        Completed
    }
}

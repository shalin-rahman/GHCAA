using System;
using System.ComponentModel.DataAnnotations;

namespace GHCAA.Domain.Models
{
    public class EmailLog
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string RecipientEmail { get; set; } = null!;

        [Required]
        [MaxLength(500)]
        public string Subject { get; set; } = null!;

        [Required]
        public string Body { get; set; } = null!;

        public string? TemplateCode { get; set; }

        public DateTime SentDate { get; set; } = DateTime.UtcNow;

        [Required]
        [MaxLength(20)]
        public string Channel { get; set; } = "Email";

        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = "Sent";

        public string? ErrorMessage { get; set; }

        public int? RecipientMemberId { get; set; }

        public int? InitiatedByMemberId { get; set; } // Who triggered the broadcast

        [MaxLength(500)]
        public string? TargetAudience { get; set; }

        [Required]
        [MaxLength(20)]
        public string DeliveryScope { get; set; } = "Targeted";
    }
}

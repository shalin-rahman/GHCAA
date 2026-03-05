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
        
        [MaxLength(100)]
        public string Status { get; set; } = "Sent"; // Sent, Failed
        
        public string? ErrorMessage { get; set; }
        
        public int? InitiatedByMemberId { get; set; } // Who triggered the broadcast
        
        public string? TargetAudience { get; set; } // batch: 2024, type: General, custom: list
    }
}

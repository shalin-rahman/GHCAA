using System;
using System.ComponentModel.DataAnnotations;

namespace GHCAA.Domain.Models
{
    public class EventTask
    {
        public int Id { get; set; }
        public int EventId { get; set; }
        
        [Required, MaxLength(200)]
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        
        public int? AssignedMemberId { get; set; }
        public DateTime? DueDate { get; set; }
        public bool IsCompleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation
        public AlumniEvent? Event { get; set; }
        public Member? AssignedMember { get; set; }
    }
}

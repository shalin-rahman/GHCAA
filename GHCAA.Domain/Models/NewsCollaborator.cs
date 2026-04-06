using System;

namespace GHCAA.Domain.Models
{
    public class NewsCollaborator
    {
        public int Id { get; set; }
        public int NewsPostId { get; set; }
        public int UserId { get; set; }
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
        
        // Navigation properties
        public NewsPost? NewsPost { get; set; }
        public User? User { get; set; }
    }
}

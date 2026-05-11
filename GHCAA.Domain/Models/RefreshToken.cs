using System.ComponentModel.DataAnnotations;

namespace GHCAA.Domain.Models
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        // Stored as SHA-256 hex; the plaintext token is only returned to the client.
        [MaxLength(64)]
        public string TokenHash { get; set; } = null!;

        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsRevoked { get; set; }

        public User User { get; set; } = null!;
    }
}

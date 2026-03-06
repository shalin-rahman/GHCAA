using System;

namespace GHCAA.Domain.Models
{
    public class User
    {
        public int Id { get; set; }

        // Username will be the MembershipNumber (e.g., GHC-2026-0001)
        public string Username { get; set; } = null!;

        // PasswordHash stored using BCrypt
        public string PasswordHash { get; set; } = null!;

        // Link to Member (created only after approval)
        public int? MemberId { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public bool IsArchived { get; set; } = false;

        // Security
        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }

        // Navigation
        public Member? Member { get; set; }
        public ICollection<Role> Roles { get; set; } = new List<Role>();
    }
}

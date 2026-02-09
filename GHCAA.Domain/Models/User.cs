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
        public int MemberId { get; set; }

        // Audit
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation
        public Member? Member { get; set; }
    }
}

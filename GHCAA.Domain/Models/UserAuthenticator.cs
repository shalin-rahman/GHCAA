namespace GHCAA.Domain.Models
{
    // 95.6. One authenticator app per user, used for step-up instead of an emailed code.
    public class UserAuthenticator
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        // The shared secret, encrypted with ASP.NET data protection. Never returned after setup.
        public string ProtectedSecret { get; set; } = null!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Null until the user types a first code from the app. A pending row is never accepted at step-up.
        public DateTime? ConfirmedAt { get; set; }

        // The 30-second step of the last accepted code, so the same code cannot be used twice.
        public long LastUsedStep { get; set; }

        // SHA-256 hex of each unused recovery code, one per line.
        public string RecoveryCodeHashes { get; set; } = string.Empty;

        // Wrong codes since the last good one. Hitting the limit sets LockedUntil and starts the count again.
        public int FailedAttempts { get; set; }
        public DateTime? LockedUntil { get; set; }

        public User User { get; set; } = null!;
    }
}

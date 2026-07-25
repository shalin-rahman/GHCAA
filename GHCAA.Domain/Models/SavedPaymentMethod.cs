using System;

namespace GHCAA.Domain.Models
{
    public class SavedPaymentMethod
    {
        public int Id { get; set; }
        public int MemberId { get; set; }
        public Member? Member { get; set; }

        public string DisplayName { get; set; } = string.Empty; // e.g., "Personal bKash", "Nagad Personal"
        public string Method { get; set; } = string.Empty; // e.g., "Bkash", "Nagad"
        public string AccountNumber { get; set; } = string.Empty; // Masked or full depending on security
        public string? Icon { get; set; }

        public bool IsDefault { get; set; }
        public DateTime LastUsedAt { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}

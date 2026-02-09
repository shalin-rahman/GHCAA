using System;
using static GHCAA.Domain.Enums;

namespace GHCAA.Domain.Models
{
    public class Otp
    {
        public int Id { get; set; }
        public string Email { get; set; } = null!;
        public string Code { get; set; } = null!; // store as string to preserve leading zeros
        public DateTime ExpiryAt { get; set; }
        public bool IsVerified { get; set; } = false;
        public int Attempts { get; set; } = 0;
        public OtpPurpose Purpose { get; set; } = OtpPurpose.Registration;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

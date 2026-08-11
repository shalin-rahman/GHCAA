using System;

namespace GHCAA.Domain.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string MessageContent { get; set; } = null!;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;

        // Navigation members are not strictly required for MVP but good for EF
        public Member? Sender { get; set; }
        public Member? Receiver { get; set; }
    }
}

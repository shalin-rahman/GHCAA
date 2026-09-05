using System;

namespace GHCAA.Domain.Models
{
    // TODO 37.3: a pledge against a Campaign. MemberId is nullable so a non-alumni donor can give —
    // DonorName/Email/Phone carry the identity when there is no Member row, same idea as
    // EventRegistration's guest fields.
    public class CampaignPledge
    {
        public int Id { get; set; }
        public int CampaignId { get; set; }
        public int? MemberId { get; set; }
        public string DonorName { get; set; } = null!;
        public string? DonorEmail { get; set; }
        public string? DonorPhone { get; set; }
        public decimal Amount { get; set; }
        public decimal AmountReceived { get; set; }
        public Enums.PledgeStatus Status { get; set; } = Enums.PledgeStatus.Pledged;
        public bool IsAnonymous { get; set; }
        public string? Message { get; set; }
        public DateTime PledgedAt { get; set; } = DateTime.UtcNow;

        // Set when an admin confirms receipt: writes a FinancialRecord (Income/Donation) and
        // back-links here so the pledge and the ledger row can never disagree about what happened.
        public int? FinancialRecordId { get; set; }

        // Navigation
        public Campaign? Campaign { get; set; }
        public Member? Member { get; set; }
        public FinancialRecord? FinancialRecord { get; set; }
    }
}

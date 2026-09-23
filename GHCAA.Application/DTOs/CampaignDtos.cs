using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GHCAA.Domain;

namespace GHCAA.Application.DTOs
{
    public class CampaignDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string Story { get; set; } = null!;
        public string? CoverImagePath { get; set; }
        public decimal TargetAmount { get; set; }
        public decimal AmountReceived { get; set; }
        public DateTime StartsOn { get; set; }
        public DateTime? EndsOn { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateCampaignDto
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = null!;

        [Required, MaxLength(200)]
        public string Slug { get; set; } = null!;

        [Required, MaxLength(4000)]
        public string Story { get; set; } = null!;

        public string? CoverImagePath { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal TargetAmount { get; set; }

        [Required]
        public DateTime StartsOn { get; set; }
        public DateTime? EndsOn { get; set; }
        public bool IsActive { get; set; } = true;
    }

    public class UpdateCampaignDto : CreateCampaignDto
    {
        [Required]
        public int Id { get; set; }
        public bool IsArchived { get; set; }
    }

    public class CampaignPledgeDto
    {
        public int Id { get; set; }
        public int CampaignId { get; set; }
        public int? MemberId { get; set; }
        public string DonorName { get; set; } = null!;
        public decimal Amount { get; set; }
        public decimal AmountReceived { get; set; }
        public Enums.PledgeStatus Status { get; set; }
        public bool IsAnonymous { get; set; }
        public string? Message { get; set; }
        public DateTime PledgedAt { get; set; }
    }

    public class CreatePledgeDto
    {
        [Range(1, double.MaxValue, ErrorMessage = "Pledge amount must be greater than zero.")]
        public decimal Amount { get; set; }

        // Required only when the caller is not a signed-in member (server fills DonorName from
        // the member's profile otherwise, the same way guest event registration works).
        [MaxLength(200)]
        public string? DonorName { get; set; }

        [MaxLength(200)]
        public string? DonorEmail { get; set; }

        [MaxLength(50)]
        public string? DonorPhone { get; set; }

        public bool IsAnonymous { get; set; }

        [MaxLength(1000)]
        public string? Message { get; set; }
    }

    public class ConfirmPledgeReceiptDto
    {
        [Required]
        public int PledgeId { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal AmountReceived { get; set; }
    }

    public class DonorRecognitionTierDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal MinimumAmount { get; set; }
        public string? Description { get; set; }
    }

    public class CreateDonorRecognitionTierDto
    {
        [Required, MaxLength(100)]
        public string Name { get; set; } = null!;

        [Range(0, double.MaxValue)]
        public decimal MinimumAmount { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }
    }

    public class HonourRollEntryDto
    {
        // "Anonymous" when the pledge's IsAnonymous is set — enforced in the projection, not here,
        // so no template can accidentally render the real name.
        public string DisplayName { get; set; } = null!;
        public decimal AmountReceived { get; set; }
        public string? Message { get; set; }
    }

    public class HonourRollTierDto
    {
        public string TierName { get; set; } = null!;
        public decimal MinimumAmount { get; set; }
        public List<HonourRollEntryDto> Donors { get; set; } = new();
    }

    public class CampaignHonourRollDto
    {
        public decimal TargetAmount { get; set; }
        public decimal TotalReceived { get; set; }
        public double ProgressPercent { get; set; }
        public int DonorCount { get; set; }
        public List<HonourRollTierDto> Tiers { get; set; } = new();
        // Confirmed donors below every configured tier's minimum still count toward the total and
        // the donor count, but have no tier bucket to sit in.
        public List<HonourRollEntryDto> Untiered { get; set; } = new();
    }
}

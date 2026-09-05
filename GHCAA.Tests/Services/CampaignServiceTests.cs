using FluentAssertions;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Tests.Services;

/// <summary>
/// Work Package 37.3 — fundraising campaigns + donor honour roll.
/// </summary>
[TestFixture]
public class CampaignServiceTests : TestBase
{
    private CampaignService NewService() => new(_context);

    private async Task<Campaign> SeedCampaignAsync(decimal targetAmount = 10000m)
    {
        var campaign = new Campaign
        {
            Title = "New Library Wing",
            Slug = "new-library-wing",
            Story = "Help us build the new library wing.",
            TargetAmount = targetAmount,
            StartsOn = DateTime.UtcNow.AddDays(-1),
            IsActive = true,
            CreatedBy = 1
        };
        _context.Campaigns.Add(campaign);
        await _context.SaveChangesAsync();
        return campaign;
    }

    [Test]
    public async Task GetHonourRollAsync_NeverLeaksDonorNameForAnAnonymousPledge()
    {
        var campaign = await SeedCampaignAsync();
        _context.CampaignPledges.Add(new CampaignPledge
        {
            CampaignId = campaign.Id,
            DonorName = "Karim Rahman",
            Amount = 500,
            AmountReceived = 500,
            IsAnonymous = true,
            PledgedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var roll = await NewService().GetHonourRollAsync(campaign.Slug);

        roll.Should().NotBeNull();
        var allEntries = roll!.Tiers.SelectMany(t => t.Donors).Concat(roll.Untiered).ToList();
        allEntries.Should().ContainSingle();
        allEntries[0].DisplayName.Should().Be("Anonymous");
        allEntries[0].DisplayName.Should().NotContain("Karim");
    }

    [Test]
    public async Task ConfirmPledgeReceiptAsync_IsIdempotent_AndWritesExactlyOneDonationRecord()
    {
        var campaign = await SeedCampaignAsync();
        var pledge = new CampaignPledge
        {
            CampaignId = campaign.Id,
            DonorName = "Anisur Islam",
            Amount = 1000,
            PledgedAt = DateTime.UtcNow
        };
        _context.CampaignPledges.Add(pledge);
        await _context.SaveChangesAsync();

        var service = NewService();
        var dto = new GHCAA.Application.DTOs.ConfirmPledgeReceiptDto { PledgeId = pledge.Id, AmountReceived = 1000 };

        (await service.ConfirmPledgeReceiptAsync(dto, adminId: 1)).Should().BeTrue();
        // Retried request / double click — must not write a second ledger row for the same money.
        (await service.ConfirmPledgeReceiptAsync(dto, adminId: 1)).Should().BeTrue();

        var donationRecords = await _context.FinancialRecords
            .Where(r => r.FinancialCategory == Enums.FinancialCategory.Donation)
            .ToListAsync();
        donationRecords.Should().ContainSingle();

        var updatedPledge = await _context.CampaignPledges.FindAsync(pledge.Id);
        updatedPledge!.Status.Should().Be(Enums.PledgeStatus.Paid);
        updatedPledge.FinancialRecordId.Should().Be(donationRecords[0].Id);
    }

    [Test]
    public async Task GetHonourRollAsync_ProgressExcludesUnconfirmedPledges()
    {
        var campaign = await SeedCampaignAsync(targetAmount: 1000m);
        _context.CampaignPledges.AddRange(
            new CampaignPledge { CampaignId = campaign.Id, DonorName = "Confirmed Donor", Amount = 400, AmountReceived = 400, PledgedAt = DateTime.UtcNow },
            new CampaignPledge { CampaignId = campaign.Id, DonorName = "Pledged Only", Amount = 600, AmountReceived = 0, PledgedAt = DateTime.UtcNow }
        );
        await _context.SaveChangesAsync();

        var roll = await NewService().GetHonourRollAsync(campaign.Slug);

        roll.Should().NotBeNull();
        roll!.TotalReceived.Should().Be(400m, "the unconfirmed pledge has not actually been received yet");
        roll.DonorCount.Should().Be(1);
        roll.ProgressPercent.Should().Be(40.0);
    }

    [Test]
    public async Task CreatePledgeAsync_GuestDonor_RecordsWithNoMemberId()
    {
        var campaign = await SeedCampaignAsync();
        var dto = new GHCAA.Application.DTOs.CreatePledgeDto { Amount = 250, DonorName = "Guest Well-Wisher", IsAnonymous = false };

        var pledge = await NewService().CreatePledgeAsync(campaign.Slug, dto, memberId: null);

        pledge.MemberId.Should().BeNull();
        pledge.DonorName.Should().Be("Guest Well-Wisher");
        pledge.Status.Should().Be(Enums.PledgeStatus.Pledged);
    }
}

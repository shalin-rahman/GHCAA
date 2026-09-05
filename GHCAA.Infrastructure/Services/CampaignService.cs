using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Services
{
    // TODO 37.3: fundraising campaigns + donor honour roll.
    public class CampaignService : ICampaignService
    {
        private readonly ApplicationDbContext _db;

        public CampaignService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<CampaignDto>> GetPublicCampaignsAsync(CancellationToken cancellationToken = default)
        {
            var campaigns = await _db.Campaigns
                .Include(c => c.Pledges)
                .Where(c => c.IsActive && !c.IsArchived)
                .OrderByDescending(c => c.StartsOn)
                .ToListAsync(cancellationToken);

            return campaigns.Select(c => MapCampaign(c, c.Pledges.Where(p => p.AmountReceived > 0).Sum(p => p.AmountReceived)));
        }

        public async Task<CampaignDto?> GetCampaignBySlugAsync(string slug, CancellationToken cancellationToken = default)
        {
            var campaign = await _db.Campaigns
                .Include(c => c.Pledges)
                .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive && !c.IsArchived, cancellationToken);

            if (campaign == null) return null;
            return MapCampaign(campaign, campaign.Pledges.Where(p => p.AmountReceived > 0).Sum(p => p.AmountReceived));
        }

        public async Task<CampaignHonourRollDto?> GetHonourRollAsync(string slug, CancellationToken cancellationToken = default)
        {
            var campaign = await _db.Campaigns
                .Include(c => c.Pledges)
                .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive && !c.IsArchived, cancellationToken);

            if (campaign == null) return null;

            // Progress and the honour roll both count only confirmed money — a pledge an admin has
            // not yet confirmed receipt for is a promise, not a donation.
            var confirmed = campaign.Pledges.Where(p => p.AmountReceived > 0).ToList();

            // Ordering by a decimal column at the SQL level isn't supported by every provider this
            // app runs on (SQLite, used by the test suite, rejects it outright) — materialise first,
            // order client-side. The table is small (admin-configured tiers), so this is cheap.
            var tiers = (await _db.DonorRecognitionTiers.ToListAsync(cancellationToken))
                .OrderByDescending(t => t.MinimumAmount)
                .ToList();

            var result = new CampaignHonourRollDto
            {
                TargetAmount = campaign.TargetAmount,
                TotalReceived = confirmed.Sum(p => p.AmountReceived),
                DonorCount = confirmed.Count,
                ProgressPercent = campaign.TargetAmount > 0
                    ? Math.Min(100.0, (double)(confirmed.Sum(p => p.AmountReceived) / campaign.TargetAmount) * 100.0)
                    : 0.0
            };

            foreach (var tier in tiers)
            {
                var tierDonors = confirmed
                    .Where(p => p.AmountReceived >= tier.MinimumAmount)
                    .OrderByDescending(p => p.AmountReceived)
                    .Select(ToHonourRollEntry)
                    .ToList();

                if (tierDonors.Count > 0)
                {
                    result.Tiers.Add(new HonourRollTierDto
                    {
                        TierName = tier.Name,
                        MinimumAmount = tier.MinimumAmount,
                        Donors = tierDonors
                    });
                }

                // Each confirmed pledge belongs to exactly one tier: the highest one it clears.
                confirmed = confirmed.Where(p => p.AmountReceived < tier.MinimumAmount).ToList();
            }

            // Whatever is left cleared no tier at all — still real money, still counted in the
            // total and the donor count above, just with nowhere to sit in the tier list.
            result.Untiered = confirmed.OrderByDescending(p => p.AmountReceived).Select(ToHonourRollEntry).ToList();

            return result;
        }

        private static HonourRollEntryDto ToHonourRollEntry(CampaignPledge p) => new()
        {
            // Anonymity is enforced here, in the one place every consumer of this projection reads
            // from — never in a template, which a future screen could forget to check.
            DisplayName = p.IsAnonymous ? "Anonymous" : p.DonorName,
            AmountReceived = p.AmountReceived,
            Message = p.Message
        };

        public async Task<CampaignPledgeDto> CreatePledgeAsync(string slug, CreatePledgeDto dto, int? memberId, CancellationToken cancellationToken = default)
        {
            var campaign = await _db.Campaigns
                .FirstOrDefaultAsync(c => c.Slug == slug && c.IsActive && !c.IsArchived, cancellationToken)
                ?? throw new KeyNotFoundException("Campaign not found or not currently accepting pledges.");

            string donorName = dto.DonorName ?? string.Empty;
            if (memberId.HasValue && string.IsNullOrWhiteSpace(donorName))
            {
                var member = await _db.Members.FindAsync(new object[] { memberId.Value }, cancellationToken);
                donorName = member?.FullName ?? "Alumni Donor";
            }
            if (string.IsNullOrWhiteSpace(donorName))
                throw new ArgumentException("A donor name is required for a guest pledge.");

            var pledge = new CampaignPledge
            {
                CampaignId = campaign.Id,
                MemberId = memberId,
                DonorName = donorName,
                DonorEmail = dto.DonorEmail,
                DonorPhone = dto.DonorPhone,
                Amount = dto.Amount,
                AmountReceived = 0,
                Status = Enums.PledgeStatus.Pledged,
                IsAnonymous = dto.IsAnonymous,
                Message = dto.Message,
                PledgedAt = DateTime.UtcNow
            };

            await _db.CampaignPledges.AddAsync(pledge, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return MapPledge(pledge);
        }

        public async Task<IEnumerable<CampaignPledgeDto>> GetMemberPledgesAsync(int memberId, CancellationToken cancellationToken = default)
        {
            var pledges = await _db.CampaignPledges
                .Where(p => p.MemberId == memberId)
                .OrderByDescending(p => p.PledgedAt)
                .ToListAsync(cancellationToken);

            return pledges.Select(MapPledge);
        }

        public async Task<IEnumerable<CampaignDto>> GetAllCampaignsForAdminAsync(CancellationToken cancellationToken = default)
        {
            var campaigns = await _db.Campaigns
                .Include(c => c.Pledges)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync(cancellationToken);

            return campaigns.Select(c => MapCampaign(c, c.Pledges.Where(p => p.AmountReceived > 0).Sum(p => p.AmountReceived)));
        }

        public async Task<CampaignDto> CreateCampaignAsync(CreateCampaignDto dto, int adminId, CancellationToken cancellationToken = default)
        {
            if (await _db.Campaigns.AnyAsync(c => c.Slug == dto.Slug, cancellationToken))
                throw new InvalidOperationException($"A campaign with slug '{dto.Slug}' already exists.");

            var campaign = new Campaign
            {
                Title = dto.Title,
                Slug = dto.Slug,
                Story = dto.Story,
                CoverImagePath = dto.CoverImagePath,
                TargetAmount = dto.TargetAmount,
                StartsOn = DateTime.SpecifyKind(dto.StartsOn, DateTimeKind.Utc),
                EndsOn = dto.EndsOn.HasValue ? DateTime.SpecifyKind(dto.EndsOn.Value, DateTimeKind.Utc) : null,
                IsActive = dto.IsActive,
                CreatedBy = adminId,
                CreatedAt = DateTime.UtcNow
            };

            await _db.Campaigns.AddAsync(campaign, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return MapCampaign(campaign, 0);
        }

        public async Task<CampaignDto?> UpdateCampaignAsync(UpdateCampaignDto dto, CancellationToken cancellationToken = default)
        {
            var campaign = await _db.Campaigns.Include(c => c.Pledges).FirstOrDefaultAsync(c => c.Id == dto.Id, cancellationToken);
            if (campaign == null) return null;

            if (await _db.Campaigns.AnyAsync(c => c.Slug == dto.Slug && c.Id != dto.Id, cancellationToken))
                throw new InvalidOperationException($"A campaign with slug '{dto.Slug}' already exists.");

            campaign.Title = dto.Title;
            campaign.Slug = dto.Slug;
            campaign.Story = dto.Story;
            campaign.CoverImagePath = dto.CoverImagePath;
            campaign.TargetAmount = dto.TargetAmount;
            campaign.StartsOn = DateTime.SpecifyKind(dto.StartsOn, DateTimeKind.Utc);
            campaign.EndsOn = dto.EndsOn.HasValue ? DateTime.SpecifyKind(dto.EndsOn.Value, DateTimeKind.Utc) : null;
            campaign.IsActive = dto.IsActive;
            campaign.IsArchived = dto.IsArchived;

            await _db.SaveChangesAsync(cancellationToken);
            return MapCampaign(campaign, campaign.Pledges.Where(p => p.AmountReceived > 0).Sum(p => p.AmountReceived));
        }

        public async Task<IEnumerable<CampaignPledgeDto>> GetPledgesForAdminAsync(int campaignId, CancellationToken cancellationToken = default)
        {
            var pledges = await _db.CampaignPledges
                .Where(p => p.CampaignId == campaignId)
                .OrderByDescending(p => p.PledgedAt)
                .ToListAsync(cancellationToken);

            return pledges.Select(MapPledge);
        }

        public async Task<bool> ConfirmPledgeReceiptAsync(ConfirmPledgeReceiptDto dto, int adminId, CancellationToken cancellationToken = default)
        {
            var pledge = await _db.CampaignPledges
                .Include(p => p.Campaign)
                .FirstOrDefaultAsync(p => p.Id == dto.PledgeId, cancellationToken);
            if (pledge == null || pledge.Campaign == null) return false;

            // Idempotent: a pledge already linked to a FinancialRecord has already been confirmed.
            // Re-confirming (a retried request, a double click) must not write a second ledger row
            // for the same money.
            if (pledge.FinancialRecordId.HasValue) return true;

            var record = new FinancialRecord
            {
                Year = DateTime.UtcNow.Year,
                RecordType = Enums.FinancialRecordType.Income,
                FinancialCategory = Enums.FinancialCategory.Donation,
                Date = DateTime.UtcNow,
                Amount = dto.AmountReceived,
                Description = $"Donation - {pledge.Campaign.Title} - {(pledge.IsAnonymous ? "Anonymous" : pledge.DonorName)}",
                Reference = $"PLEDGE-{pledge.Id}",
                CreatedAt = DateTime.UtcNow,
                CreatedByAdminId = adminId
            };
            await _db.FinancialRecords.AddAsync(record, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);

            pledge.AmountReceived = dto.AmountReceived;
            pledge.FinancialRecordId = record.Id;
            pledge.Status = dto.AmountReceived >= pledge.Amount ? Enums.PledgeStatus.Paid : Enums.PledgeStatus.PartiallyPaid;
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<IEnumerable<DonorRecognitionTierDto>> GetTiersAsync(CancellationToken cancellationToken = default)
        {
            var tiers = await _db.DonorRecognitionTiers.ToListAsync(cancellationToken);
            return tiers
                .OrderByDescending(t => t.MinimumAmount)
                .Select(t => new DonorRecognitionTierDto { Id = t.Id, Name = t.Name, MinimumAmount = t.MinimumAmount, Description = t.Description });
        }

        public async Task<DonorRecognitionTierDto> CreateTierAsync(CreateDonorRecognitionTierDto dto, CancellationToken cancellationToken = default)
        {
            var tier = new DonorRecognitionTier { Name = dto.Name, MinimumAmount = dto.MinimumAmount, Description = dto.Description };
            await _db.DonorRecognitionTiers.AddAsync(tier, cancellationToken);
            await _db.SaveChangesAsync(cancellationToken);
            return new DonorRecognitionTierDto { Id = tier.Id, Name = tier.Name, MinimumAmount = tier.MinimumAmount, Description = tier.Description };
        }

        private static CampaignDto MapCampaign(Campaign c, decimal amountReceived) => new()
        {
            Id = c.Id,
            Title = c.Title,
            Slug = c.Slug,
            Story = c.Story,
            CoverImagePath = c.CoverImagePath,
            TargetAmount = c.TargetAmount,
            AmountReceived = amountReceived,
            StartsOn = c.StartsOn,
            EndsOn = c.EndsOn,
            IsActive = c.IsActive
        };

        private static CampaignPledgeDto MapPledge(CampaignPledge p) => new()
        {
            Id = p.Id,
            CampaignId = p.CampaignId,
            MemberId = p.MemberId,
            DonorName = p.DonorName,
            Amount = p.Amount,
            AmountReceived = p.AmountReceived,
            Status = p.Status,
            IsAnonymous = p.IsAnonymous,
            Message = p.Message,
            PledgedAt = p.PledgedAt
        };
    }
}

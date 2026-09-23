using GHCAA.Application.DTOs;

namespace GHCAA.Application.Interfaces
{
    public interface ICampaignService
    {
        Task<IEnumerable<CampaignDto>> GetPublicCampaignsAsync(CancellationToken cancellationToken = default);
        Task<CampaignDto?> GetCampaignBySlugAsync(string slug, CancellationToken cancellationToken = default);
        Task<CampaignHonourRollDto?> GetHonourRollAsync(string slug, CancellationToken cancellationToken = default);

        Task<CampaignPledgeDto> CreatePledgeAsync(string slug, CreatePledgeDto dto, int? memberId, CancellationToken cancellationToken = default);
        Task<IEnumerable<CampaignPledgeDto>> GetMemberPledgesAsync(int memberId, CancellationToken cancellationToken = default);

        // Admin
        Task<IEnumerable<CampaignDto>> GetAllCampaignsForAdminAsync(CancellationToken cancellationToken = default);
        Task<CampaignDto> CreateCampaignAsync(CreateCampaignDto dto, int adminId, CancellationToken cancellationToken = default);
        Task<CampaignDto?> UpdateCampaignAsync(UpdateCampaignDto dto, CancellationToken cancellationToken = default);
        Task<IEnumerable<CampaignPledgeDto>> GetPledgesForAdminAsync(int campaignId, CancellationToken cancellationToken = default);

        // Idempotent: confirming an already-confirmed pledge again does not write a second
        // FinancialRecord — it is a no-op that returns the existing linked record's id.
        Task<bool> ConfirmPledgeReceiptAsync(ConfirmPledgeReceiptDto dto, int adminId, CancellationToken cancellationToken = default);

        Task<IEnumerable<DonorRecognitionTierDto>> GetTiersAsync(CancellationToken cancellationToken = default);
        Task<DonorRecognitionTierDto> CreateTierAsync(CreateDonorRecognitionTierDto dto, CancellationToken cancellationToken = default);
    }
}

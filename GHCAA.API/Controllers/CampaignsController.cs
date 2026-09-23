using System.Security.Claims;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Application.Security;
using GHCAA.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GHCAA.API.Extensions;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/campaigns")]
    public class CampaignsController : ControllerBase
    {
        private readonly ICampaignService _campaigns;

        public CampaignsController(ICampaignService campaigns)
        {
            _campaigns = campaigns;
        }

        [HttpGet("public")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicCampaigns(CancellationToken cancellationToken)
        {
            return Ok(await _campaigns.GetPublicCampaignsAsync(cancellationToken));
        }

        [HttpGet("{slug}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
        {
            var campaign = await _campaigns.GetCampaignBySlugAsync(slug, cancellationToken);
            return campaign == null ? NotFound() : Ok(campaign);
        }

        [HttpGet("{slug}/honour-roll")]
        [AllowAnonymous]
        public async Task<IActionResult> GetHonourRoll(string slug, CancellationToken cancellationToken)
        {
            var roll = await _campaigns.GetHonourRollAsync(slug, cancellationToken);
            return roll == null ? NotFound() : Ok(roll);
        }

        // Pledging is allowed anonymously (a guest donor has no account), so this endpoint is not
        // [Authorize] — a signed-in member's MemberId claim, when present, still links the pledge to
        // their profile for "my giving history".
        [HttpPost("{slug}/pledges")]
        [AllowAnonymous]
        public async Task<IActionResult> CreatePledge(string slug, [FromBody] CreatePledgeDto dto, CancellationToken cancellationToken)
        {
            int? memberId = null;
            if (int.TryParse(this.CurrentMemberIdRaw(), out var mid))
                memberId = mid;

            try
            {
                var pledge = await _campaigns.CreatePledgeAsync(slug, dto, memberId, cancellationToken);
                return Ok(pledge);
            }
            catch (KeyNotFoundException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status404NotFound);
            }
            catch (ArgumentException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
            }
        }

        [HttpGet("my-pledges")]
        [Authorize]
        public async Task<IActionResult> GetMyPledges(CancellationToken cancellationToken)
        {
            if (!int.TryParse(this.CurrentMemberIdRaw(), out var memberId))
                return Unauthorized();

            return Ok(await _campaigns.GetMemberPledgesAsync(memberId, cancellationToken));
        }

        [HttpGet("admin/all")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> GetAllForAdmin(CancellationToken cancellationToken)
        {
            return Ok(await _campaigns.GetAllCampaignsForAdminAsync(cancellationToken));
        }

        [HttpPost("admin")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> Create([FromBody] CreateCampaignDto dto, CancellationToken cancellationToken)
        {
            if (!int.TryParse(this.CurrentUserIdRaw(), out var adminId))
                return Unauthorized();

            try
            {
                var campaign = await _campaigns.CreateCampaignAsync(dto, adminId, cancellationToken);
                return CreatedAtAction(nameof(GetBySlug), new { slug = campaign.Slug }, campaign);
            }
            catch (InvalidOperationException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
            }
        }

        [HttpPut("admin")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> Update([FromBody] UpdateCampaignDto dto, CancellationToken cancellationToken)
        {
            try
            {
                var campaign = await _campaigns.UpdateCampaignAsync(dto, cancellationToken);
                return campaign == null ? NotFound() : Ok(campaign);
            }
            catch (InvalidOperationException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status400BadRequest);
            }
        }

        [HttpGet("admin/{campaignId}/pledges")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> GetPledgesForAdmin(int campaignId, CancellationToken cancellationToken)
        {
            return Ok(await _campaigns.GetPledgesForAdminAsync(campaignId, cancellationToken));
        }

        [HttpPost("admin/pledges/confirm-receipt")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> ConfirmReceipt([FromBody] ConfirmPledgeReceiptDto dto, CancellationToken cancellationToken)
        {
            if (!int.TryParse(this.CurrentUserIdRaw(), out var adminId))
                return Unauthorized();

            var success = await _campaigns.ConfirmPledgeReceiptAsync(dto, adminId, cancellationToken);
            return success ? Ok() : NotFound();
        }

        [HttpGet("admin/tiers")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> GetTiers(CancellationToken cancellationToken)
        {
            return Ok(await _campaigns.GetTiersAsync(cancellationToken));
        }

        [HttpPost("admin/tiers")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> CreateTier([FromBody] CreateDonorRecognitionTierDto dto, CancellationToken cancellationToken)
        {
            return Ok(await _campaigns.CreateTierAsync(dto, cancellationToken));
        }
    }
}

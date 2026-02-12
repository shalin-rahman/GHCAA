using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/networking")]
    [Authorize]
    public class NetworkingController : ControllerBase
    {
        private readonly INetworkingService _networkingService;
        private readonly IIDCardService _idCardService;

        public NetworkingController(INetworkingService networkingService, IIDCardService idCardService)
        {
            _networkingService = networkingService;
            _idCardService = idCardService;
        }

        [HttpGet("my-id-card")]
        public async Task<IActionResult> GetMyIDCard(CancellationToken cancellationToken)
        {
            var memberIdClaim = User.FindFirst("MemberId")?.Value;
            if (string.IsNullOrEmpty(memberIdClaim) || !int.TryParse(memberIdClaim, out var memberId))
                return BadRequest("Invalid user session");

            var dataUri = await _idCardService.GenerateIDCardDataUriAsync(memberId, cancellationToken);
            return Ok(new { DataUri = dataUri });
        }

        [HttpGet("id-card/{memberId}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetMemberIDCard(int memberId, CancellationToken cancellationToken)
        {
            var dataUri = await _idCardService.GenerateIDCardDataUriAsync(memberId, cancellationToken);
            return Ok(new { DataUri = dataUri });
        }

        [HttpGet("my-certificate")]
        public async Task<IActionResult> GetMyCertificate(CancellationToken cancellationToken)
        {
            var memberIdClaim = User.FindFirst("MemberId")?.Value;
            if (string.IsNullOrEmpty(memberIdClaim) || !int.TryParse(memberIdClaim, out var memberId))
                return BadRequest("Invalid user session");

            var dataUri = await _idCardService.GenerateCertificateDataUriAsync(memberId, cancellationToken);
            return Ok(new { DataUri = dataUri });
        }

        [HttpGet("certificate/{memberId}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetMemberCertificate(int memberId, CancellationToken cancellationToken)
        {
            var dataUri = await _idCardService.GenerateCertificateDataUriAsync(memberId, cancellationToken);
            return Ok(new { DataUri = dataUri });
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> Search([FromQuery] MemberSearchFilterDto filter, CancellationToken cancellationToken)
        {
            var results = await _networkingService.SearchMembersAsync(filter, cancellationToken);
            return Ok(results);
        }

        [HttpGet("member/{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicProfile(int id, CancellationToken cancellationToken)
        {
            var profile = await _networkingService.GetMemberProfileAsync(id, cancellationToken);
            if (profile == null) return NotFound();
            return Ok(profile);
        }

        [HttpGet("committee")]
        [AllowAnonymous]
        public async Task<IActionResult> GetExecutiveCommittee([FromQuery] int? year, CancellationToken cancellationToken)
        {
            var committee = await _networkingService.GetExecutiveCommitteeAsync(year, cancellationToken);
            return Ok(committee);
        }

        [HttpGet("updates")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLatestUpdates([FromQuery] int count = 10, CancellationToken cancellationToken = default)
        {
            var updates = await _networkingService.GetLatestAlumniUpdatesAsync(count, cancellationToken);
            return Ok(updates);
        }
    }
}

using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GHCAA.Application.Security;

namespace GHCAA.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/me")]
    public class MeController : ControllerBase
    {
        private readonly IMemberService _memberService;
        private readonly IIDCardService _idCardService;

        public MeController(IMemberService memberService, IIDCardService idCardService)
        {
            _memberService = memberService;
            _idCardService = idCardService;
        }

        private int GetMemberId()
        {
            var claim = User.FindFirst(AppClaimTypes.MemberId)?.Value;
            if (string.IsNullOrEmpty(claim) || !int.TryParse(claim, out var memberId))
                throw new UnauthorizedAccessException();
            return memberId;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetMyProfile(CancellationToken cancellationToken)
        {
            var profile = await _memberService.GetProfileAsync(GetMemberId(), isPrivileged: true, cancellationToken);
            if (profile == null) return NotFound();
            return Ok(profile);
        }

        [HttpGet("id-card")]
        public async Task<IActionResult> GetMyIDCard(CancellationToken cancellationToken)
        {
            // Only active members get an ID card
            var profile = await _memberService.GetProfileAsync(GetMemberId(), isPrivileged: false, cancellationToken);
            if (profile == null || profile.Status != GHCAA.Domain.Enums.MembershipStatus.Active)
                return Forbid();

            var dataUri = await _idCardService.GenerateIDCardDataUriAsync(GetMemberId(), cancellationToken);
            return Ok(new { DataUri = dataUri });
        }

        [HttpGet("id-card/pdf")]
        public async Task<IActionResult> DownloadMyIDCardPdf(CancellationToken cancellationToken)
        {
            var profile = await _memberService.GetProfileAsync(GetMemberId(), isPrivileged: false, cancellationToken);
            if (profile == null || profile.Status != GHCAA.Domain.Enums.MembershipStatus.Active)
                return Forbid();

            var pdfBytes = await _idCardService.GenerateIDCardPdfAsync(GetMemberId(), cancellationToken);
            return File(pdfBytes, "application/pdf", $"GHCAA_ID_Card_{GetMemberId()}.pdf");
        }

        [HttpGet("certificate")]
        public async Task<IActionResult> GetMyCertificate(CancellationToken cancellationToken)
        {
            var profile = await _memberService.GetProfileAsync(GetMemberId(), isPrivileged: false, cancellationToken);
            if (profile == null || profile.Status != GHCAA.Domain.Enums.MembershipStatus.Active)
                return Forbid();

            var dataUri = await _idCardService.GenerateCertificateDataUriAsync(GetMemberId(), cancellationToken);
            return Ok(new { DataUri = dataUri });
        }

        [HttpGet("certificate/pdf")]
        public async Task<IActionResult> DownloadMyCertificatePdf(CancellationToken cancellationToken)
        {
            var profile = await _memberService.GetProfileAsync(GetMemberId(), isPrivileged: false, cancellationToken);
            if (profile == null || profile.Status != GHCAA.Domain.Enums.MembershipStatus.Active)
                return Forbid();

            var pdfBytes = await _idCardService.GenerateCertificatePdfAsync(GetMemberId(), cancellationToken);
            return File(pdfBytes, "application/pdf", $"GHCAA_Certificate_{GetMemberId()}.pdf");
        }
    }
}

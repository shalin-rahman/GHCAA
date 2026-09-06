using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GHCAA.Domain;
using GHCAA.Application.Security;
using GHCAA.API.Extensions;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/mentorship")]
    [Authorize]
    public class MentorshipController : ControllerBase
    {
        private readonly IMentorshipService _service;

        public MentorshipController(IMentorshipService service)
        {
            _service = service;
        }

        private int GetMemberId()
        {
            var val = this.CurrentMemberIdRaw();
            return int.TryParse(val, out var id) ? id : 0;
        }

        /// <summary>Member sends a mentorship request to another member.</summary>
        [HttpPost]
        public async Task<IActionResult> SendRequest([FromBody] SendMentorshipRequestDto dto, CancellationToken ct)
        {
            var requesterId = GetMemberId();
            if (requesterId == 0) return Unauthorized();
            if (requesterId == dto.MentorId) return Problem(detail: "You cannot send a mentorship request to yourself.", statusCode: StatusCodes.Status400BadRequest);

            try
            {
                var request = await _service.SendRequestAsync(requesterId, dto.MentorId, dto.Message, dto.Domain, ct);
                return Ok(new { request.Id, request.Status });
            }
            catch (InvalidOperationException ex)
            {
                return Problem(detail: ex.Message, statusCode: StatusCodes.Status409Conflict);
            }
        }

        /// <summary>Get all requests the current member has sent.</summary>
        [HttpGet("sent")]
        public async Task<IActionResult> GetSent(CancellationToken ct)
        {
            var memberId = GetMemberId();
            if (memberId == 0) return Unauthorized();
            return Ok(await _service.GetSentRequestsAsync(memberId, ct));
        }

        /// <summary>Get all mentorship requests the current member has received as a mentor.</summary>
        [HttpGet("received")]
        public async Task<IActionResult> GetReceived(CancellationToken ct)
        {
            var memberId = GetMemberId();
            if (memberId == 0) return Unauthorized();
            return Ok(await _service.GetReceivedRequestsAsync(memberId, ct));
        }

        /// <summary>Mentor accepts or declines a request.</summary>
        [HttpPost("{id}/respond")]
        public async Task<IActionResult> Respond(int id, [FromBody] RespondMentorshipDto dto, CancellationToken ct)
        {
            var memberId = GetMemberId();
            if (memberId == 0) return Unauthorized();
            var success = await _service.RespondAsync(id, memberId, dto.Accept, dto.Note, ct);
            return success ? Ok() : NotFound();
        }

        /// <summary>Either party can mark the mentorship as completed.</summary>
        [HttpPost("{id}/complete")]
        public async Task<IActionResult> MarkComplete(int id, CancellationToken ct)
        {
            var memberId = GetMemberId();
            if (memberId == 0) return Unauthorized();
            var success = await _service.MarkCompleteAsync(id, memberId, ct);
            return success ? Ok() : NotFound();
        }

        /// <summary>Admin view of all mentorship requests.</summary>
        [HttpGet("admin/all")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> GetAllForAdmin(CancellationToken ct)
        {
            return Ok(await _service.GetAllForAdminAsync(ct));
        }
    }

    public class SendMentorshipRequestDto
    {
        public int MentorId { get; set; }
        public string? Message { get; set; }
        public string? Domain { get; set; }
    }

    public class RespondMentorshipDto
    {
        public bool Accept { get; set; }
        public string? Note { get; set; }
    }
}

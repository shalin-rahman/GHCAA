using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GHCAA.API.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly IEventService _eventService;

        public EventsController(IEventService eventService)
        {
            _eventService = eventService;
        }

        // --- PUBLIC / MEMBER ENDPOINTS ---

        [HttpGet]
        public async Task<IActionResult> GetActiveEvents(CancellationToken cancellationToken)
        {
            var events = await _eventService.GetActiveEventsAsync(cancellationToken);
            return Ok(events);
        }

        [HttpGet("{id}/participants")]
        [AllowAnonymous]
        public async Task<IActionResult> GetPublicParticipants(int id, CancellationToken cancellationToken)
        {
            var participants = await _eventService.GetPublicParticipantsAsync(id, cancellationToken);
            return Ok(participants);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventById(int id, CancellationToken cancellationToken)
        {
            var ev = await _eventService.GetEventByIdAsync(id, cancellationToken);
            return ev == null ? NotFound() : Ok(ev);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterForEvent([FromForm] RegisterForEventDto dto, IFormFile? receipt, CancellationToken cancellationToken)
        {
            int? memberId = null;
            if (User.Identity?.IsAuthenticated == true)
            {
                var memberIdClaim = User.FindFirst("MemberId")?.Value;
                if (!string.IsNullOrEmpty(memberIdClaim) && int.TryParse(memberIdClaim, out var mid))
                {
                    memberId = mid;
                }
            }

            // If unauthenticated, check if non-member registration is requested
            bool isGuestFullfilled = dto.IsNonMember || (!string.IsNullOrEmpty(dto.GuestEmail) && !string.IsNullOrEmpty(dto.GuestName));
            
            if (!memberId.HasValue && !isGuestFullfilled)
            {
                return Unauthorized("A member account is required for this registration.");
            }

            UploadedFileDto? receiptDto = null;
            if (receipt != null)
            {
                var ms = new MemoryStream();
                await receipt.CopyToAsync(ms, cancellationToken);
                ms.Position = 0;
                receiptDto = new UploadedFileDto
                {
                    Content = ms,
                    FileName = receipt.FileName,
                    ContentType = receipt.ContentType,
                    Length = receipt.Length
                };
            }

            var result = await _eventService.RegisterForEventAsync(dto, memberId, receiptDto, cancellationToken);
            return Ok(result);
        }

        [HttpGet("my-registrations")]
        [Authorize]
        public async Task<IActionResult> GetMyRegistrations(CancellationToken cancellationToken)
        {
            var memberIdClaim = User.FindFirst("MemberId")?.Value;
            if (string.IsNullOrEmpty(memberIdClaim) || !int.TryParse(memberIdClaim, out var memberId))
            {
                if (User.IsInRole("SuperAdmin"))
                    return Ok(new List<object>());
                
                return BadRequest("User is not associated with a member account.");
            }

            var registrations = await _eventService.GetRegistrationsByMemberAsync(memberId, cancellationToken);
            return Ok(registrations);
        }

        [HttpGet("registration/{id}")]
        [Authorize]
        public async Task<IActionResult> GetRegistrationForInvitation(int id, CancellationToken cancellationToken)
        {
            var registration = await _eventService.GetRegistrationByIdAsync(id, cancellationToken);
            if (registration == null) return NotFound();

            bool isAdmin = User.IsInRole("Admin");
            var memberIdClaim = User.FindFirst("MemberId")?.Value;

            if (!isAdmin)
            {
                if (string.IsNullOrEmpty(memberIdClaim) || !int.TryParse(memberIdClaim, out var memberId) || registration.MemberId != memberId)
                {
                    return Forbid();
                }

                if (registration.Status != GHCAA.Domain.Enums.EventRegistrationStatus.Approved)
                {
                    return BadRequest("Invitation is only available for approved registrations.");
                }
            }

            return Ok(registration);
        }

        // --- ADMIN ENDPOINTS ---

        [HttpGet("admin/all")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetAllEventsForAdmin(CancellationToken cancellationToken)
        {
            var events = await _eventService.GetAllEventsForAdminAsync(cancellationToken);
            return Ok(events);
        }

        [HttpPost("admin")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto, CancellationToken cancellationToken)
        {
            var result = await _eventService.CreateEventAsync(dto, cancellationToken);
            return CreatedAtAction(nameof(GetEventById), new { id = result.Id }, result);
        }

        [HttpPut("admin/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventDto dto, CancellationToken cancellationToken)
        {
            dto.Id = id;
            var result = await _eventService.UpdateEventAsync(dto, cancellationToken);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpDelete("admin/{id}")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> DeleteEvent(int id, CancellationToken cancellationToken)
        {
            var success = await _eventService.DeleteEventAsync(id, cancellationToken);
            return success ? Ok() : NotFound();
        }

        [HttpPost("admin/{id}/logo")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> UploadEventLogo(int id, IFormFile logo, CancellationToken cancellationToken)
        {
            if (logo == null || logo.Length == 0) return BadRequest("No file uploaded");

            using var ms = new MemoryStream();
            await logo.CopyToAsync(ms, cancellationToken);
            ms.Position = 0;

            var uploadedFile = new UploadedFileDto
            {
                Content = ms,
                FileName = logo.FileName,
                ContentType = logo.ContentType,
                Length = logo.Length
            };

            var logoUrl = await _eventService.UpdateEventLogoAsync(id, uploadedFile, cancellationToken);
            return Ok(new { ImageUrl = logoUrl });
        }

        [HttpGet("admin/registrations")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> GetAllRegistrations(int page = 1, int pageSize = 10, int? eventId = null, string? status = null, string? search = null, CancellationToken cancellationToken = default)
        {
            var registrations = await _eventService.GetAllRegistrationsForAdminAsync(page, pageSize, eventId, status, search, cancellationToken);
            return Ok(registrations);
        }

        [HttpPost("admin/approve-registration")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> ApproveRegistration([FromBody] ApproveRegistrationDto dto, CancellationToken cancellationToken)
        {
            var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(adminIdClaim, out var adminUserId))
            {
                return Unauthorized();
            }

            var success = await _eventService.ApproveRegistrationAsync(dto.RegistrationId, adminUserId, dto.Approve, cancellationToken);
            return success ? Ok() : NotFound();
        }

        [HttpPost("admin/registrations/{id}/send-invitation")]
        [Authorize(Policy = "AdminOnly")]
        public async Task<IActionResult> SendInvitation(int id, CancellationToken cancellationToken)
        {
            var success = await _eventService.SendInvitationEmailAsync(id, cancellationToken);
            return success ? Ok() : BadRequest("Failed to send invitation or registration not approved.");
        }
    }
}

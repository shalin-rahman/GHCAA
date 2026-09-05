using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using GHCAA.Application.Security;
using GHCAA.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GHCAA.API.Controllers
{
    // WP81.2: one place to see everything waiting on you, admin or member, instead of five
    // separate screens. Pulls a count and a summary row from each existing pending endpoint —
    // it does not decide anything or duplicate any approval logic, only aggregates.
    [ApiController]
    [Route("api/pending")]
    public class PendingApprovalsController : ControllerBase
    {
        private readonly INewsService _newsService;
        private readonly IGalleryService _galleryService;
        private readonly IJobHubService _jobService;
        private readonly IMemberService _memberService;
        private readonly IEventService _eventService;
        private readonly IFamilyLinkService _familyLinkService;
        private readonly IMentorshipService _mentorshipService;

        public PendingApprovalsController(
            INewsService newsService,
            IGalleryService galleryService,
            IJobHubService jobService,
            IMemberService memberService,
            IEventService eventService,
            IFamilyLinkService familyLinkService,
            IMentorshipService mentorshipService)
        {
            _newsService = newsService;
            _galleryService = galleryService;
            _jobService = jobService;
            _memberService = memberService;
            _eventService = eventService;
            _familyLinkService = familyLinkService;
            _mentorshipService = mentorshipService;
        }

        /// <summary>Everything an admin has waiting for a decision, across every approval queue.</summary>
        [HttpGet("admin/summary")]
        [Authorize(Policy = Constants.Policies.AdminOnly)]
        public async Task<IActionResult> GetAdminSummary(CancellationToken cancellationToken)
        {
            var news = await _newsService.GetPendingSubmissionsAsync(cancellationToken);
            var galleries = await _galleryService.GetPendingGalleryApprovalsAsync(cancellationToken);
            var photos = await _galleryService.GetPendingPhotoApprovalsAsync(cancellationToken);
            var jobs = await _jobService.GetPendingJobsAsync(cancellationToken);
            // pageSize=5: a summary row, not the full list — the existing screens still own paging.
            var members = await _memberService.GetAllMembersAsync(1, 5, "", "Applied", "all", "all", false, true, cancellationToken);
            var eventRegistrations = await _eventService.GetAllRegistrationsForAdminAsync(1, 5, null, "Pending", null, cancellationToken);

            return Ok(new
            {
                News = news,
                Galleries = galleries,
                Photos = photos,
                Jobs = jobs,
                Members = members,
                EventRegistrations = eventRegistrations,
            });
        }

        /// <summary>Everything the calling member has submitted or requested that is still pending someone else's action.</summary>
        [HttpGet("me/summary")]
        [Authorize]
        public async Task<IActionResult> GetMySummary(CancellationToken cancellationToken)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized();

            var memberId = GetMemberId();
            if (memberId == null) return Unauthorized();

            var news = await _newsService.GetMySubmissionsAsync(userId, cancellationToken);
            var familyLinkSent = await _familyLinkService.GetSentRequestsAsync(memberId.Value, cancellationToken);
            var mentorshipSent = await _mentorshipService.GetSentRequestsAsync(memberId.Value, cancellationToken);

            var jobs = await _jobService.GetMemberJobsAsync(memberId.Value, cancellationToken);
            var pendingJobs = jobs.Where(j => j.Status == Enums.SubmissionStatus.Pending);

            var registrations = await _eventService.GetRegistrationsByMemberAsync(memberId.Value, cancellationToken);
            var pendingRegistrations = registrations.Where(r => r.Status == Enums.EventRegistrationStatus.Pending);

            return Ok(new
            {
                News = news,
                FamilyLinkRequestsSent = familyLinkSent,
                MentorshipRequestsSent = mentorshipSent,
                PendingJobPostings = pendingJobs,
                PendingEventRegistrations = pendingRegistrations,
            });
        }

        // 24.50: null on an absent/malformed claim, same convention as FamilyLinkController.
        private int? GetMemberId()
        {
            var value = User.FindFirstValue(AppClaimTypes.MemberId);
            return int.TryParse(value, out var id) ? id : null;
        }
    }
}

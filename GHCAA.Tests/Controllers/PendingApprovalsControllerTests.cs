using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Controllers;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Controllers
{
    [TestFixture]
    public class PendingApprovalsControllerTests
    {
        private Mock<INewsService> _newsServiceMock;
        private Mock<IGalleryService> _galleryServiceMock;
        private Mock<IJobHubService> _jobServiceMock;
        private Mock<IMemberService> _memberServiceMock;
        private Mock<IEventService> _eventServiceMock;
        private Mock<IFamilyLinkService> _familyLinkServiceMock;
        private Mock<IMentorshipService> _mentorshipServiceMock;
        private PendingApprovalsController _controller;

        [SetUp]
        public void Setup()
        {
            _newsServiceMock = new Mock<INewsService>();
            _galleryServiceMock = new Mock<IGalleryService>();
            _jobServiceMock = new Mock<IJobHubService>();
            _memberServiceMock = new Mock<IMemberService>();
            _eventServiceMock = new Mock<IEventService>();
            _familyLinkServiceMock = new Mock<IFamilyLinkService>();
            _mentorshipServiceMock = new Mock<IMentorshipService>();

            _controller = new PendingApprovalsController(
                _newsServiceMock.Object,
                _galleryServiceMock.Object,
                _jobServiceMock.Object,
                _memberServiceMock.Object,
                _eventServiceMock.Object,
                _familyLinkServiceMock.Object,
                _mentorshipServiceMock.Object);
        }

        private void SetUser(int? userId, int? memberId)
        {
            var claims = new List<Claim>();
            if (userId.HasValue) claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
            if (memberId.HasValue) claims.Add(new Claim(GHCAA.Application.Security.AppClaimTypes.MemberId, memberId.Value.ToString()));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test")) }
            };
        }

        [Test]
        public async Task GetAdminSummary_ReturnsOkWithAllQueues()
        {
            _newsServiceMock.Setup(x => x.GetPendingSubmissionsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<NewsPostDto>());
            _galleryServiceMock.Setup(x => x.GetPendingGalleryApprovalsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EventGallery>());
            _galleryServiceMock.Setup(x => x.GetPendingPhotoApprovalsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EventPhoto>());
            _jobServiceMock.Setup(x => x.GetPendingJobsAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<JobDto>());
            _memberServiceMock.Setup(x => x.GetAllMembersAsync(1, 5, "", nameof(GHCAA.Domain.Enums.MembershipStatus.Applied), "all", "all", false, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new { TotalItems = 0, Items = new object[0] });
            _eventServiceMock.Setup(x => x.GetAllRegistrationsForAdminAsync(1, 5, null, nameof(GHCAA.Domain.Enums.EventRegistrationStatus.Pending), null, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new { TotalItems = 0, Items = new object[0] });

            var result = await _controller.GetAdminSummary(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            _newsServiceMock.Verify(x => x.GetPendingSubmissionsAsync(It.IsAny<CancellationToken>()), Times.Once);
            _eventServiceMock.Verify(x => x.GetAllRegistrationsForAdminAsync(1, 5, null, nameof(GHCAA.Domain.Enums.EventRegistrationStatus.Pending), null, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetMySummary_NoMemberClaim_ReturnsUnauthorized()
        {
            SetUser(userId: 5, memberId: null);

            var result = await _controller.GetMySummary(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
        }

        [Test]
        public async Task GetMySummary_ValidMember_ReturnsOkAndFiltersToPendingOnly()
        {
            SetUser(userId: 5, memberId: 10);

            _newsServiceMock.Setup(x => x.GetMySubmissionsAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<NewsPostDto>());
            _familyLinkServiceMock.Setup(x => x.GetSentRequestsAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<FamilyLinkRequestDto>());
            _mentorshipServiceMock.Setup(x => x.GetSentRequestsAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<object>());
            _jobServiceMock.Setup(x => x.GetMemberJobsAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<JobDto>
                {
                    new JobDto { Id = 1, Title = "A", CompanyName = "X", Location = "L", Description = "D", Requirements = "R", Status = GHCAA.Domain.Enums.SubmissionStatus.Pending },
                    new JobDto { Id = 2, Title = "B", CompanyName = "X", Location = "L", Description = "D", Requirements = "R", Status = GHCAA.Domain.Enums.SubmissionStatus.Approved },
                });
            _eventServiceMock.Setup(x => x.GetRegistrationsByMemberAsync(10, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<EventRegistration>
                {
                    new EventRegistration { Id = 1, Status = GHCAA.Domain.Enums.EventRegistrationStatus.Pending },
                    new EventRegistration { Id = 2, Status = GHCAA.Domain.Enums.EventRegistrationStatus.Approved },
                });

            var result = await _controller.GetMySummary(CancellationToken.None) as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            var jobsProp = result!.Value!.GetType().GetProperty("PendingJobPostings")!.GetValue(result.Value) as System.Collections.Generic.IEnumerable<JobDto>;
            Assert.That(jobsProp, Has.Exactly(1).Items);

            var regsProp = result.Value!.GetType().GetProperty("PendingEventRegistrations")!.GetValue(result.Value) as System.Collections.Generic.IEnumerable<EventRegistration>;
            Assert.That(regsProp, Has.Exactly(1).Items);
        }
    }
}

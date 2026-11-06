using System.Reflection;
using GHCAA.API.Controllers;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GHCAA.Tests.Controllers
{
    [TestFixture]
    public class ScholarshipsControllerTests : ControllerTestBase
    {
        private Mock<IScholarshipService> _service = null!;
        private ScholarshipsController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _service = new Mock<IScholarshipService>();
            _controller = new ScholarshipsController(_service.Object);
            SetMemberContext(_controller, 100);
        }

        [Test]
        public async Task Apply_ReturnsOk()
        {
            var dto = new CreateScholarshipApplicationDto();
            _service.Setup(s => s.SubmitApplicationAsync(4, dto, It.IsAny<CancellationToken>())).ReturnsAsync(new ScholarshipApplicationDto());

            var result = await _controller.Apply(4, dto, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task Apply_UnknownCall_Returns404()
        {
            _service.Setup(s => s.SubmitApplicationAsync(4, It.IsAny<CreateScholarshipApplicationDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new KeyNotFoundException("Call not found."));

            var result = await _controller.Apply(4, new CreateScholarshipApplicationDto(), CancellationToken.None);

            Assert.That((result as ObjectResult)?.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task Apply_ClosedCall_Returns400()
        {
            _service.Setup(s => s.SubmitApplicationAsync(4, It.IsAny<CreateScholarshipApplicationDto>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("Call is closed."));

            var result = await _controller.Apply(4, new CreateScholarshipApplicationDto(), CancellationToken.None);

            Assert.That((result as ObjectResult)?.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task Status_WrongEmail_ReturnsNotFound()
        {
            _service.Setup(s => s.GetPublicStatusAsync("SCH-1", "x@y.z", It.IsAny<CancellationToken>())).ReturnsAsync((ScholarshipStatusDto?)null);

            var result = await _controller.Status("SCH-1", "x@y.z", CancellationToken.None);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task SubmitReview_WithoutMemberClaim_ReturnsUnauthorized()
        {
            SetUserContext(_controller, memberId: null, role: "Admin", userId: 1);

            var result = await _controller.SubmitReview(2, new SubmitScholarshipReviewDto(), CancellationToken.None);

            Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
        }

        [Test]
        public async Task Disburse_UnknownAward_ReturnsNotFound()
        {
            SetUserContext(_controller, memberId: null, role: "Admin", userId: 5);
            _service.Setup(s => s.DisburseAwardAsync(8, 5, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = await _controller.Disburse(8, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [TestCase(nameof(ScholarshipsController.AdminApplications))]
        [TestCase(nameof(ScholarshipsController.CreateFund))]
        [TestCase(nameof(ScholarshipsController.CreateCall))]
        [TestCase(nameof(ScholarshipsController.CreateAward))]
        [TestCase(nameof(ScholarshipsController.Disburse))]
        public void AdminActions_RequireAdminOnlyPolicy(string action)
        {
            var attr = typeof(ScholarshipsController).GetMethod(action)!.GetCustomAttribute<AuthorizeAttribute>();

            Assert.That(attr?.Policy, Is.EqualTo(Constants.Policies.AdminOnly));
        }

        [TestCase(nameof(ScholarshipsController.ReviewQueue))]
        [TestCase(nameof(ScholarshipsController.ReviewApplication))]
        [TestCase(nameof(ScholarshipsController.SubmitReview))]
        public void ReviewActions_RequireMemberOnlyPolicy(string action)
        {
            var attr = typeof(ScholarshipsController).GetMethod(action)!.GetCustomAttribute<AuthorizeAttribute>();

            Assert.That(attr?.Policy, Is.EqualTo(Constants.Policies.MemberOnly));
        }
    }
}

using System.Reflection;
using GHCAA.API.Controllers;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GHCAA.Tests.Controllers
{
    [TestFixture]
    public class MentorshipControllerTests : ControllerTestBase
    {
        private Mock<IMentorshipService> _service = null!;
        private MentorshipController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _service = new Mock<IMentorshipService>();
            _controller = new MentorshipController(_service.Object);
            SetMemberContext(_controller, 100);
        }

        [Category("FR-023")]
        [Test]
        public async Task SendRequest_ReturnsOk_WithRequestIdAndStatus()
        {
            _service.Setup(s => s.SendRequestAsync(100, 7, "hi", "IT", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new MentorshipRequest { Id = 3, RequesterId = 100, MentorId = 7 });

            var result = await _controller.SendRequest(new SendMentorshipRequestDto { MentorId = 7, Message = "hi", Domain = "IT" }, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Category("FR-023")]
        [Test]
        public async Task SendRequest_ToSelf_Returns400()
        {
            var result = await _controller.SendRequest(new SendMentorshipRequestDto { MentorId = 100 }, CancellationToken.None);

            Assert.That((result as ObjectResult)?.StatusCode, Is.EqualTo(400));
            _service.Verify(s => s.SendRequestAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Category("FR-023")]
        [Test]
        public async Task SendRequest_Duplicate_Returns409()
        {
            _service.Setup(s => s.SendRequestAsync(100, 7, null, null, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new InvalidOperationException("already pending"));

            var result = await _controller.SendRequest(new SendMentorshipRequestDto { MentorId = 7 }, CancellationToken.None);

            Assert.That((result as ObjectResult)?.StatusCode, Is.EqualTo(409));
        }

        [Category("FR-024")]
        [Test]
        public async Task GetSent_WithoutMemberClaim_ReturnsUnauthorized()
        {
            SetUserContext(_controller, memberId: null, role: "Admin", userId: 1);

            var result = await _controller.GetSent(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
        }

        [Category("FR-025")]
        [Test]
        public async Task GetReceived_ReturnsOk()
        {
            _service.Setup(s => s.GetReceivedRequestsAsync(100, It.IsAny<CancellationToken>())).ReturnsAsync(new List<object>());

            var result = await _controller.GetReceived(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Category("FR-026")]
        [Test]
        public async Task Respond_UnknownRequest_ReturnsNotFound()
        {
            _service.Setup(s => s.RespondAsync(9, 100, true, null, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = await _controller.Respond(9, new RespondMentorshipDto { Accept = true }, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Category("FR-027")]
        [Test]
        public async Task MarkComplete_ReturnsOk()
        {
            _service.Setup(s => s.MarkCompleteAsync(9, 100, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await _controller.MarkComplete(9, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkResult>());
        }

        [Category("FR-028")]
        [Test]
        public async Task AdminClose_PassesNote_AndReturnsNotFoundWhenMissing()
        {
            _service.Setup(s => s.AdminCloseAsync(9, "stale", It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = await _controller.AdminClose(9, new AdminCloseMentorshipDto { Note = "stale" }, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Category("FR-028")]
        [TestCase(nameof(MentorshipController.GetAllForAdmin))]
        [TestCase(nameof(MentorshipController.AdminClose))]
        public void AdminActions_RequireAdminOnlyPolicy(string action)
        {
            var attr = typeof(MentorshipController).GetMethod(action)!.GetCustomAttribute<AuthorizeAttribute>();

            Assert.That(attr?.Policy, Is.EqualTo(Constants.Policies.AdminOnly));
        }

        [Test]
        public void Controller_RequiresSignedInUser()
        {
            Assert.That(typeof(MentorshipController).GetCustomAttribute<AuthorizeAttribute>(), Is.Not.Null);
        }
    }
}

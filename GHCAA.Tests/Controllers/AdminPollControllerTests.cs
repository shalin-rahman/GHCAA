using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Controllers;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Controllers
{
    [TestFixture]
    public class AdminPollControllerTests : ControllerTestBase
    {
        private const int AdminMemberId = 42;
        private const int PollId = 7;
        private const string PollTitle = "Member survey";

        private Mock<IPollService> _pollService = null!;
        private AdminPollController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _pollService = new Mock<IPollService>();
            _controller = new AdminPollController(_pollService.Object);
        }

        [Test]
        public async Task GetAllPolls_ReturnsServiceResults()
        {
            var polls = new List<PollDto> { new() { Id = 4, Title = PollTitle } };
            _pollService.Setup(service => service.GetAllPollsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(polls);

            var result = await _controller.GetAllPolls(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            Assert.That(((OkObjectResult)result).Value, Is.SameAs(polls));
        }

        [Test]
        public async Task CreatePoll_UsesTheMemberClaimAndReturnsCreatedLocation()
        {
            var dto = new CreatePollDto { Title = PollTitle, Options = new List<string> { "Yes", "No" } };
            SetUserContext(_controller, memberId: AdminMemberId);
            _pollService.Setup(service => service.CreatePollAsync(dto, AdminMemberId, It.IsAny<CancellationToken>())).ReturnsAsync(9);

            var result = await _controller.CreatePoll(dto, CancellationToken.None);

            var created = result as CreatedAtActionResult;
            Assert.That(created, Is.Not.Null);
            Assert.That(created!.ActionName, Is.EqualTo(nameof(AdminPollController.GetAllPolls)));
            _pollService.Verify(service => service.CreatePollAsync(dto, AdminMemberId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task ToggleStatus_ReturnsOk_WhenPollExists(bool isActive)
        {
            _pollService.Setup(service => service.TogglePollStatusAsync(PollId, isActive, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await _controller.ToggleStatus(PollId, isActive, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            _pollService.Verify(service => service.TogglePollStatusAsync(PollId, isActive, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task ToggleStatus_ReturnsNotFound_WhenPollDoesNotExist()
        {
            _pollService.Setup(service => service.TogglePollStatusAsync(PollId, false, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = await _controller.ToggleStatus(PollId, false, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task DeletePoll_MapsServiceOutcomeToTheHttpResult(bool deleted)
        {
            _pollService.Setup(service => service.DeletePollAsync(PollId, It.IsAny<CancellationToken>())).ReturnsAsync(deleted);

            var result = await _controller.DeletePoll(PollId, CancellationToken.None);

            Assert.That(result, Is.TypeOf(deleted ? typeof(OkObjectResult) : typeof(NotFoundResult)));
        }
    }
}

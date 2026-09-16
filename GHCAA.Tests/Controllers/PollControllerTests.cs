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
    public class PollControllerTests : ControllerTestBase
    {
        private const int MemberId = 42;
        private const int PollId = 7;
        private const int PollOptionId = 3;
        private const string PollTitle = "Member survey";

        private Mock<IPollService> _pollService = null!;
        private PollController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _pollService = new Mock<IPollService>();
            _controller = new PollController(_pollService.Object);
        }

        [Test]
        public async Task MemberActions_ReturnUnauthorizedWithoutMemberClaim()
        {
            var vote = new PollVoteDto { OptionIds = new List<int> { PollOptionId } };
            SetUserContext(_controller, memberId: null);

            var activeResult = await _controller.GetActivePolls(CancellationToken.None);
            var pollResult = await _controller.GetPoll(PollId, CancellationToken.None);
            var voteResult = await _controller.Vote(PollId, vote, CancellationToken.None);

            Assert.That(activeResult, Is.InstanceOf<UnauthorizedResult>());
            Assert.That(pollResult, Is.InstanceOf<UnauthorizedResult>());
            Assert.That(voteResult, Is.InstanceOf<UnauthorizedResult>());
            _pollService.VerifyNoOtherCalls();
        }

        [Test]
        public async Task GetActivePolls_ReturnsServiceResultForClaimMember()
        {
            var polls = new List<PollDto> { CreatePollDto() };
            SetMemberContext(_controller, MemberId);
            _pollService.Setup(service => service.GetActivePollsAsync(MemberId, It.IsAny<CancellationToken>())).ReturnsAsync(polls);

            var result = await _controller.GetActivePolls(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            Assert.That(((OkObjectResult)result).Value, Is.SameAs(polls));
            _pollService.Verify(service => service.GetActivePollsAsync(MemberId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task GetPollById_MapsServiceResultToOkOrNotFound(bool pollExists)
        {
            SetMemberContext(_controller, MemberId);
            _pollService
                .Setup(service => service.GetPollByIdAsync(PollId, MemberId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(pollExists ? CreatePollDto() : null);

            var result = await _controller.GetPoll(PollId, CancellationToken.None);

            Assert.That(result, Is.TypeOf(pollExists ? typeof(OkObjectResult) : typeof(NotFoundResult)));
        }

        [TestCase(true)]
        [TestCase(false)]
        public async Task Vote_MapsAcceptedAndRejectedVote(bool accepted)
        {
            var vote = new PollVoteDto { OptionIds = new List<int> { PollOptionId } };
            SetMemberContext(_controller, MemberId);
            _pollService
                .Setup(service => service.VoteAsync(PollId, MemberId, vote.OptionIds, It.IsAny<CancellationToken>()))
                .ReturnsAsync(accepted);

            var result = await _controller.Vote(PollId, vote, CancellationToken.None);

            Assert.That(result, Is.TypeOf(accepted ? typeof(OkObjectResult) : typeof(ObjectResult)));
            if (!accepted)
            {
                Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(400));
            }
            _pollService.Verify(service => service.VoteAsync(PollId, MemberId, vote.OptionIds, It.IsAny<CancellationToken>()), Times.Once);
        }

        private static PollDto CreatePollDto() => new() { Id = PollId, Title = PollTitle };
    }
}

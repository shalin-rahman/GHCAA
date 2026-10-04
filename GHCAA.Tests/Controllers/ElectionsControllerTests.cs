using System;
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
    public class ElectionsControllerTests : ControllerTestBase
    {
        private Mock<IElectionService> _electionServiceMock;
        private Mock<IElectionDocumentService> _documentServiceMock;
        private Mock<IElectionApprovalService> _approvalServiceMock;
        private ElectionsController _controller;

        [SetUp]
        public void Setup()
        {
            _electionServiceMock = new Mock<IElectionService>();
            _documentServiceMock = new Mock<IElectionDocumentService>();
            _approvalServiceMock = new Mock<IElectionApprovalService>();
            _controller = new ElectionsController(_electionServiceMock.Object, _documentServiceMock.Object, _approvalServiceMock.Object);
            SetUserContext(_controller, memberId: 10, role: "Member");
        }

        private static readonly CastBallotDto Ballot = new([new BallotSeatChoiceDto(1, [2])]);

        [Test]
        [Category("FR-39")]
        public async Task Vote_ServiceFails_Returns400Problem()
        {
            _electionServiceMock.Setup(x => x.CastBallotAsync(1, 10, Ballot, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync((false, "not-recorded", (string?)null));

            var result = await _controller.Vote(1, Ballot, CancellationToken.None);

            var obj = result as ObjectResult;
            Assert.That(obj, Is.Not.Null);
            Assert.That(obj!.StatusCode, Is.EqualTo(400));
            Assert.That(((ProblemDetails)obj.Value!).Detail, Is.EqualTo("Vote could not be recorded."));
        }

        [Test]
        [Category("NFR-R5")]
        public async Task Vote_AlreadyVoted_Returns409WithCode()
        {
            _electionServiceMock.Setup(x => x.CastBallotAsync(1, 10, Ballot, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync((false, "already-voted", (string?)null));

            var result = await _controller.Vote(1, Ballot, CancellationToken.None);

            var obj = result as ObjectResult;
            Assert.That(obj!.StatusCode, Is.EqualTo(409));
            Assert.That(((ProblemDetails)obj.Value!).Extensions["code"], Is.EqualTo(GHCAA.Domain.Constants.ErrorCodes.AlreadyVoted));
        }

        [Test]
        [Category("FR-39")]
        public async Task Vote_Success_ReturnsTrackingCode()
        {
            _electionServiceMock.Setup(x => x.CastBallotAsync(1, 10, Ballot, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync((true, (string?)null, "ABCD-EFGH-JKMN"));

            var result = await _controller.Vote(1, Ballot, CancellationToken.None);

            var ok = result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            Assert.That(((CastBallotResultDto)ok!.Value!).TrackingCode, Is.EqualTo("ABCD-EFGH-JKMN"));
        }

        // 37.13h. The count can wait for a second person, like the other two-person steps.
        private void CountReturns(ElectionCountRunResult run) =>
            _approvalServiceMock.Setup(x => x.CountAsync(1, 10, It.IsAny<IReadOnlyCollection<string>>(), "key", It.IsAny<CancellationToken>()))
                                .ReturnsAsync(run);

        [Test]
        [Category("FR-39")]
        public async Task Count_WaitingForASecondPerson_Returns202()
        {
            var pending = new ElectionApprovalDto(5, 1, GHCAA.Domain.Enums.ElectionApprovalAction.Count, 10, "alice", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), null);
            CountReturns(new ElectionCountRunResult(null, null, pending));

            var result = await _controller.Count(1, new CountElectionRequest("key"), CancellationToken.None);

            var accepted = result as AcceptedResult;
            Assert.That(accepted, Is.Not.Null);
            Assert.That(accepted!.Value, Is.EqualTo(pending));
        }

        [Test]
        [Category("FR-39")]
        public async Task Count_AlreadyWaiting_Returns409()
        {
            CountReturns(new ElectionCountRunResult(null, "already-pending", null));

            var result = await _controller.Count(1, new CountElectionRequest("key"), CancellationToken.None);

            Assert.That((result as ObjectResult)!.StatusCode, Is.EqualTo(409));
        }

        [Test]
        [Category("FR-39")]
        public async Task Count_ByTheApprover_Returns403()
        {
            CountReturns(new ElectionCountRunResult(null, "same-person", null));

            var result = await _controller.Count(1, new CountElectionRequest("key"), CancellationToken.None);

            Assert.That((result as ObjectResult)!.StatusCode, Is.EqualTo(403));
        }

        [Test]
        [Category("FR-39")]
        public async Task Count_Counted_ReturnsResults()
        {
            IReadOnlyList<ElectionResultDto> results = [new ElectionResultDto(1, 2, 10, true, false)];
            CountReturns(new ElectionCountRunResult(results, null, null));

            var result = await _controller.Count(1, new CountElectionRequest("key"), CancellationToken.None);

            Assert.That((result as OkObjectResult)!.Value, Is.EqualTo(results));
        }
    }
}

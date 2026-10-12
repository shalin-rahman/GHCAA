using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Controllers;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
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
            _controller = new ElectionsController(_electionServiceMock.Object, _documentServiceMock.Object, _approvalServiceMock.Object, new Mock<IElectionAuditWriter>().Object);
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

        [Test]
        [Category("FR-39")]
        public async Task Create_PeriodRejected_Returns400WithAPlainReason()
        {
            var request = new CreateElectionDto("E", 5, DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow, DateTime.UtcNow, 0);
            _electionServiceMock.Setup(x => x.CreateAsync(It.IsAny<CreateElectionDto>(), It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(((ElectionSummaryDto?)null, "period-has-members"));

            var result = await _controller.Create(request, CancellationToken.None);

            var obj = result as ObjectResult;
            Assert.That(obj!.StatusCode, Is.EqualTo(400));
            Assert.That(((ProblemDetails)obj.Value!).Detail, Is.EqualTo("That EC period already has committee members. Choose a new period."));
        }

        [Test]
        [Category("FR-49")]
        public async Task OpenedBallots_NotPublished_Returns404()
        {
            _electionServiceMock.Setup(x => x.GetOpenedBallotsAsync(1, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync((OpenedBallotsDto?)null);

            Assert.That(await _controller.OpenedBallots(1, CancellationToken.None), Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        [Category("FR-49")]
        public async Task OpenedBallots_Published_ReturnsOk()
        {
            _electionServiceMock.Setup(x => x.GetOpenedBallotsAsync(1, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(new OpenedBallotsDto(1, []));

            Assert.That(await _controller.OpenedBallots(1, CancellationToken.None), Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        [Category("FR-48")]
        public async Task AuditVerify_Anonymous_ReturnsTheVerdict()
        {
            var audit = new Mock<IElectionAuditWriter>();
            audit.Setup(x => x.VerifyAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(new ElectionAuditVerifyDto(false, 4, 3));
            var controller = new ElectionsController(_electionServiceMock.Object, _documentServiceMock.Object, _approvalServiceMock.Object, audit.Object);

            var ok = (OkObjectResult)await controller.VerifyAudit(1, CancellationToken.None);

            Assert.That(((ElectionAuditVerifyDto)ok.Value!).FirstBrokenSeq, Is.EqualTo(3));
        }

        [Test]
        [Category("FR-39")]
        public async Task Results_NotDeclared_Returns404()
        {
            _electionServiceMock.Setup(x => x.GetPublicResultsAsync(1, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync((PublicElectionResultsDto?)null);

            Assert.That(await _controller.Results(1, CancellationToken.None), Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        [Category("FR-39")]
        public async Task Results_TestElection_Returns404()
        {
            _electionServiceMock.Setup(x => x.IsTestAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            Assert.That(await _controller.Results(1, CancellationToken.None), Is.InstanceOf<NotFoundResult>());
            _electionServiceMock.Verify(x => x.GetPublicResultsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        [Category("FR-50")]
        public async Task Approvals_FlagsTheCallersOwnRequest()
        {
            SetUserContext(_controller, userId: 10, role: "ElectionOfficial");
            var own = new ElectionApprovalDto(5, 1, GHCAA.Domain.Enums.ElectionApprovalAction.Count, 10, "me", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), null);
            var other = new ElectionApprovalDto(6, 1, GHCAA.Domain.Enums.ElectionApprovalAction.Declare, 11, "them", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), null);
            _approvalServiceMock.Setup(x => x.ListOpenAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync([own, other]);

            var result = await _controller.Approvals(1, CancellationToken.None);

            var list = ((IEnumerable<ElectionApprovalDto>)((OkObjectResult)result).Value!).ToList();
            Assert.That(list.Single(a => a.Id == 5).RequestedByMe, Is.True);
            Assert.That(list.Single(a => a.Id == 6).RequestedByMe, Is.False);
        }

        [Test]
        [Category("FR-50")]
        public async Task KeyDestroyed_MapsServiceReplies()
        {
            SetUserContext(_controller, userId: 10, role: "ElectionOfficial");
            _electionServiceMock.SetupSequence(x => x.RecordKeyDestroyedAsync(1, 10, It.IsAny<CancellationToken>()))
                                .ReturnsAsync((string?)null)
                                .ReturnsAsync(Constants.Elections.Replies.AlreadyRecorded)
                                .ReturnsAsync(Constants.Elections.Replies.NotDeclared)
                                .ReturnsAsync(Constants.Elections.Replies.NotFound);

            Assert.That(await _controller.KeyDestroyed(1, CancellationToken.None), Is.InstanceOf<NoContentResult>());
            Assert.That(((ObjectResult)await _controller.KeyDestroyed(1, CancellationToken.None)).StatusCode, Is.EqualTo(409));
            Assert.That(((ObjectResult)await _controller.KeyDestroyed(1, CancellationToken.None)).StatusCode, Is.EqualTo(400));
            Assert.That(await _controller.KeyDestroyed(1, CancellationToken.None), Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        [Category("FR-39")]
        public async Task TrackingCodes_TestElection_Returns404()
        {
            _electionServiceMock.Setup(x => x.IsTestAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            Assert.That(await _controller.TrackingCodes(1, CancellationToken.None), Is.InstanceOf<NotFoundResult>());
            _electionServiceMock.Verify(x => x.GetTrackingCodesAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        [Category("FR-39")]
        public async Task Get_TestElection_Returns404()
        {
            _electionServiceMock.Setup(x => x.IsTestAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            Assert.That(await _controller.Get(1, CancellationToken.None), Is.InstanceOf<NotFoundResult>());
            _electionServiceMock.Verify(x => x.GetAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        [Category("FR-39")]
        public async Task Get_RealElection_ReturnsTheSummary()
        {
            _electionServiceMock.Setup(x => x.GetAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(Summary(1));

            Assert.That(await _controller.Get(1, CancellationToken.None), Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        [Category("FR-48")]
        public async Task AuditVerify_TestElection_Returns404()
        {
            _electionServiceMock.Setup(x => x.IsTestAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            var audit = new Mock<IElectionAuditWriter>();
            var controller = new ElectionsController(_electionServiceMock.Object, _documentServiceMock.Object, _approvalServiceMock.Object, audit.Object);

            Assert.That(await controller.VerifyAudit(1, CancellationToken.None), Is.InstanceOf<NotFoundResult>());
            audit.Verify(x => x.VerifyAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task Document_TestElection_StillServesAnOfficial()
        {
            _electionServiceMock.Setup(x => x.IsTestAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _documentServiceMock.Setup(x => x.GeneratePdfAsync(1, "form-1", It.IsAny<CancellationToken>())).ReturnsAsync([1, 2, 3]);

            Assert.That(await _controller.Document(1, "form-1", CancellationToken.None), Is.InstanceOf<FileContentResult>());
        }

        [Test]
        public async Task Document_UnknownForm_Returns404()
        {
            _documentServiceMock.Setup(x => x.GeneratePdfAsync(1, "nope", It.IsAny<CancellationToken>())).ReturnsAsync((byte[]?)null);

            Assert.That(await _controller.Document(1, "nope", CancellationToken.None), Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task GetLatestDeclared_ReturnsTheSummary()
        {
            _electionServiceMock.Setup(x => x.GetLatestDeclaredAsync(It.IsAny<CancellationToken>())).ReturnsAsync(Summary(4));

            var ok = (OkObjectResult)await _controller.GetLatestDeclared(CancellationToken.None);

            Assert.That(((ElectionSummaryDto)ok.Value!).Id, Is.EqualTo(4));
        }

        [Test]
        public async Task GetLatestDeclared_NoneDeclared_Returns404()
        {
            _electionServiceMock.Setup(x => x.GetLatestDeclaredAsync(It.IsAny<CancellationToken>())).ReturnsAsync((ElectionSummaryDto?)null);

            Assert.That(await _controller.GetLatestDeclared(CancellationToken.None), Is.InstanceOf<NotFoundResult>());
        }

        private static ElectionSummaryDto Summary(int id) => new(id, "Election", GHCAA.Domain.Enums.ElectionPhase.Declared, 1, 0, 0);

        // The route is a literal, so it cannot be read as an {id:int}.
        [Test]
        public void LatestDeclared_RouteIsALiteralSegment()
        {
            var route = typeof(ElectionsController).GetMethod(nameof(ElectionsController.GetLatestDeclared))!
                .GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.HttpGetAttribute), false).Cast<Microsoft.AspNetCore.Mvc.HttpGetAttribute>().Single();

            Assert.That(route.Template, Is.EqualTo("latest-declared"));
        }
    }
}

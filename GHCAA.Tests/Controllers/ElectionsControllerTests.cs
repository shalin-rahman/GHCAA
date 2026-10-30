using GHCAA.API.Controllers;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using static GHCAA.Domain.Enums;

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

            var result = await _controller.Vote(1, Ballot, NullLogger<ElectionsController>.Instance, CancellationToken.None);

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

            var result = await _controller.Vote(1, Ballot, NullLogger<ElectionsController>.Instance, CancellationToken.None);

            var obj = result as ObjectResult;
            Assert.That(obj!.StatusCode, Is.EqualTo(409));
            Assert.That(((ProblemDetails)obj.Value!).Extensions["code"], Is.EqualTo(GHCAA.Domain.Constants.ErrorCodes.AlreadyVoted));
        }

        [Test]
        [Category("FR-39")]
        public async Task Vote_Success_ReturnsTrackingCode()
        {
            using var cancellation = new CancellationTokenSource();
            _electionServiceMock.Setup(x => x.CastBallotAsync(1, 10, Ballot, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync((true, (string?)null, "ABCD-EFGH-JKMN"));
            _documentServiceMock.Setup(x => x.GenerateBallotReceiptPdfAsync(1, "ABCD-EFGH-JKMN", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                                .ReturnsAsync(new byte[] { 1, 2, 3 });

            var result = await _controller.Vote(1, Ballot, NullLogger<ElectionsController>.Instance, cancellation.Token);

            var ok = result as OkObjectResult;
            Assert.That(ok, Is.Not.Null);
            var reply = (CastBallotResultDto)ok!.Value!;
            Assert.That(reply.TrackingCode, Is.EqualTo("ABCD-EFGH-JKMN"));
            Assert.That(reply.ReceiptPdfBase64, Is.EqualTo(Convert.ToBase64String(new byte[] { 1, 2, 3 })));
            _documentServiceMock.Verify(x => x.GenerateBallotReceiptPdfAsync(
                1, "ABCD-EFGH-JKMN", It.IsAny<DateTime>(), cancellation.Token), Times.Once);
        }

        // 37.14o. The vote is saved by then, so the code must still reach the voter.
        [Test]
        [Category("FR-39")]
        public async Task Vote_ReceiptPdfFails_StillReturnsTheCode()
        {
            _electionServiceMock.Setup(x => x.CastBallotAsync(1, 10, Ballot, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync((true, (string?)null, "ABCD-EFGH-JKMN"));
            _documentServiceMock.Setup(x => x.GenerateBallotReceiptPdfAsync(1, "ABCD-EFGH-JKMN", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                                .ThrowsAsync(new InvalidOperationException("font missing"));

            var result = await _controller.Vote(1, Ballot, NullLogger<ElectionsController>.Instance, CancellationToken.None);

            var reply = (CastBallotResultDto)((OkObjectResult)result).Value!;
            Assert.That(reply.TrackingCode, Is.EqualTo("ABCD-EFGH-JKMN"));
            Assert.That(reply.ReceiptPdfBase64, Is.Null);
        }

        [Test]
        [Category("NFR-R5")]
        public async Task Vote_ClientDisconnectDuringReceiptPdf_DoesNotLogPdfFailure()
        {
            using var cancellation = new CancellationTokenSource();
            _electionServiceMock.Setup(x => x.CastBallotAsync(1, 10, Ballot, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync((true, (string?)null, "ABCD-EFGH-JKMN"));
            _documentServiceMock.Setup(x => x.GenerateBallotReceiptPdfAsync(1, "ABCD-EFGH-JKMN", It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
                                .ThrowsAsync(new OperationCanceledException(cancellation.Token));
            var logger = new Mock<ILogger<ElectionsController>>();
            cancellation.Cancel();

            var result = await _controller.Vote(1, Ballot, logger.Object, cancellation.Token);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            logger.Verify(x => x.Log(LogLevel.Error, It.IsAny<EventId>(), It.Is<It.IsAnyType>((_, _) => true),
                It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
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
        [Category("FR-051")]
        public async Task CandidateList_NotFinalized_Returns404()
        {
            _electionServiceMock.Setup(x => x.GetPublicCandidateListAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<PublicCandidateSeatDto>?)null);

            var result = await _controller.CandidateList(1, CancellationToken.None);

            Assert.That(result, Is.TypeOf<NotFoundResult>());
            _electionServiceMock.Verify(x => x.GetPublicCandidateListAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _electionServiceMock.Verify(x => x.IsTestAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        [Category("FR-051")]
        public async Task CandidateList_TestElection_Returns404FromService()
        {
            _electionServiceMock.Setup(x => x.GetPublicCandidateListAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<PublicCandidateSeatDto>?)null);

            var result = await _controller.CandidateList(1, CancellationToken.None);

            Assert.That(result, Is.TypeOf<NotFoundResult>());
            _electionServiceMock.Verify(x => x.GetPublicCandidateListAsync(1, It.IsAny<CancellationToken>()), Times.Once);
            _electionServiceMock.Verify(x => x.IsTestAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
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
                                .ReturnsAsync(Constants.Elections.Replies.NotFound)
                                .ReturnsAsync(Constants.Elections.Replies.RecountWindowOpen);

            Assert.That(await _controller.KeyDestroyed(1, CancellationToken.None), Is.InstanceOf<NoContentResult>());
            Assert.That(((ObjectResult)await _controller.KeyDestroyed(1, CancellationToken.None)).StatusCode, Is.EqualTo(409));
            Assert.That(((ObjectResult)await _controller.KeyDestroyed(1, CancellationToken.None)).StatusCode, Is.EqualTo(400));
            Assert.That(await _controller.KeyDestroyed(1, CancellationToken.None), Is.InstanceOf<NotFoundResult>());
            Assert.That(((ObjectResult)await _controller.KeyDestroyed(1, CancellationToken.None)).StatusCode, Is.EqualTo(400));
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
        public async Task Seats_TestElection_Returns404()
        {
            _electionServiceMock.Setup(x => x.IsTestAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            Assert.That(await _controller.Seats(1, CancellationToken.None), Is.InstanceOf<NotFoundResult>());
            _electionServiceMock.Verify(x => x.GetSeatsAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        [Category("FR-39")]
        public async Task Seats_ReturnsTheSeatsOr404()
        {
            _electionServiceMock.Setup(x => x.GetSeatsAsync(1, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { new ElectionSeatViewDto(5, "President", 1) });

            Assert.That(await _controller.Seats(1, CancellationToken.None), Is.InstanceOf<OkObjectResult>());
            Assert.That(await _controller.Seats(2, CancellationToken.None), Is.InstanceOf<NotFoundResult>());
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
            _documentServiceMock.Setup(x => x.GeneratePdfAsync(1, "form-1", It.IsAny<ElectionPrintMode?>(), It.IsAny<CancellationToken>())).ReturnsAsync([1, 2, 3]);

            Assert.That(await _controller.Document(1, "form-1", null, CancellationToken.None), Is.InstanceOf<FileContentResult>());
        }

        [Test]
        public async Task Document_UnknownForm_Returns404()
        {
            _documentServiceMock.Setup(x => x.GeneratePdfAsync(1, "nope", It.IsAny<ElectionPrintMode?>(), It.IsAny<CancellationToken>())).ReturnsAsync((byte[]?)null);

            Assert.That(await _controller.Document(1, "nope", null, CancellationToken.None), Is.InstanceOf<NotFoundResult>());
        }

        // 37.14s. The mode in the query reaches the service unchanged.
        [Test]
        public async Task Document_PassesThePrintMode()
        {
            _documentServiceMock.Setup(x => x.GeneratePdfAsync(1, "ER-10", ElectionPrintMode.Blank, It.IsAny<CancellationToken>())).ReturnsAsync([1]);

            Assert.That(await _controller.Document(1, "ER-10", ElectionPrintMode.Blank, CancellationToken.None), Is.InstanceOf<FileContentResult>());
        }

        // 37.14s. "mode=7" binds to an unnamed value; it must not fall through to a filled print.
        [Test]
        [Category("FR-25")]
        public async Task Document_UnknownPrintMode_Returns400()
        {
            var result = await _controller.Document(1, "ER-10", (ElectionPrintMode)7, CancellationToken.None);

            Assert.That((result as ObjectResult)!.StatusCode, Is.EqualTo(400));
            _documentServiceMock.Verify(x => x.GeneratePdfAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ElectionPrintMode?>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task BlankForm_KnownCode_ReturnsPdfNamedAfterTheCode()
        {
            _documentServiceMock.Setup(x => x.GenerateBlankPdfAsync("er-05", It.IsAny<CancellationToken>())).ReturnsAsync([1, 2]);

            var result = await _controller.BlankForm("er-05", CancellationToken.None) as FileContentResult;

            Assert.That(result!.ContentType, Is.EqualTo("application/pdf"));
            Assert.That(result.FileDownloadName, Is.EqualTo("ER-05_blank.pdf"));
        }

        [Test]
        public async Task BlankForm_UnknownCode_Returns404()
        {
            _documentServiceMock.Setup(x => x.GenerateBlankPdfAsync("nope", It.IsAny<CancellationToken>())).ReturnsAsync((byte[]?)null);

            Assert.That(await _controller.BlankForm("nope", CancellationToken.None), Is.InstanceOf<NotFoundResult>());
        }

        // 37.14n. The verify page behind a printed form's QR code.
        [Test]
        [Category("FR-25")]
        public async Task VerifyDocument_KnownForm_ReturnsTheRecord()
        {
            var record = new ElectionDocumentVerifyDto("E-000001", 1, "EC 2026", GHCAA.Domain.Enums.ElectionPhase.Declared, "ER-27C", "Final Result Sheet");
            _documentServiceMock.Setup(x => x.VerifyAsync(1, "ER-27C", It.IsAny<CancellationToken>())).ReturnsAsync(record);

            var result = await _controller.VerifyDocument(1, "ER-27C", CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            Assert.That(((OkObjectResult)result).Value, Is.EqualTo(record));
        }

        [Test]
        [Category("FR-25")]
        public async Task VerifyDocument_UnknownForm_Returns404()
        {
            _documentServiceMock.Setup(x => x.VerifyAsync(1, "ER-99", It.IsAny<CancellationToken>())).ReturnsAsync((ElectionDocumentVerifyDto?)null);

            Assert.That(await _controller.VerifyDocument(1, "ER-99", CancellationToken.None), Is.InstanceOf<NotFoundResult>());
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

        private static readonly ElectionReadinessDto PartlyReady = new(false,
        [
            new(Constants.Elections.Readiness.RollFrozen, true),
            new(Constants.Elections.Readiness.BallotKey, false),
            new(Constants.Elections.Readiness.PaperStock, false, Attestation: true),
            new(Constants.Elections.Readiness.Approvers, false, Advisory: true),
        ]);

        private static void AssertNamesUnmet(IActionResult result)
        {
            var obj = result as ObjectResult;
            Assert.That(obj?.StatusCode, Is.EqualTo(400));
            var problem = (ProblemDetails)obj!.Value!;
            Assert.That(problem.Extensions["code"], Is.EqualTo(Constants.ErrorCodes.PollingNotReady));
            Assert.That(problem.Detail, Is.EqualTo("Polling cannot open until these are met: the ballot key is set; paper stock is ready."));
            Assert.That(problem.Extensions["unmet"], Is.EqualTo(new[] { Constants.Elections.Readiness.BallotKey, Constants.Elections.Readiness.PaperStock }));
        }

        [Test]
        [Category("FR-37.1b")]
        public async Task Nominate_RuleBroken_Returns400WithTheRule()
        {
            var request = new NominationDto(4, 10, 0, 12, "Statement", null);
            _electionServiceMock.Setup(x => x.SubmitNominationAsync(3, It.IsAny<NominationDto>(), It.IsAny<CancellationToken>()))
                                .ThrowsAsync(new InvalidOperationException("Candidate cannot propose or second their own nomination."));

            var obj = await _controller.Nominate(3, request, CancellationToken.None) as ObjectResult;

            Assert.That(obj?.StatusCode, Is.EqualTo(400));
            Assert.That(((ProblemDetails)obj!.Value!).Detail, Is.EqualTo("Candidate cannot propose or second their own nomination."));
        }

        [Test]
        [Category("FR-37.1b")]
        public async Task Scrutinise_RejectWithoutReason_Returns400WithTheRule()
        {
            _electionServiceMock.Setup(x => x.DecideNominationAsync(9, It.IsAny<int>(), It.IsAny<ScrutinyDto>(), It.IsAny<CancellationToken>()))
                                .ThrowsAsync(new InvalidOperationException("Give a reason for rejecting the nomination."));

            var obj = await _controller.Scrutinise(9, new ScrutinyDto(false, " "), CancellationToken.None) as ObjectResult;

            Assert.That(obj?.StatusCode, Is.EqualTo(400));
            Assert.That(((ProblemDetails)obj!.Value!).Detail, Is.EqualTo("Give a reason for rejecting the nomination."));
        }

        [Test]
        [Category("FR-37.1b")]
        public async Task Withdraw_Refused_Returns400WithTheRule()
        {
            _electionServiceMock.Setup(x => x.WithdrawNominationAsync(9, 10, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var obj = await _controller.Withdraw(9, CancellationToken.None) as ObjectResult;

            Assert.That(obj?.StatusCode, Is.EqualTo(400));
            Assert.That(((ProblemDetails)obj!.Value!).Detail, Does.Contain("Withdrawal phase"));
        }

        [Test]
        [Category("FR-37.1b")]
        public async Task Nominate_SetsTheCallerAsProposer()
        {
            var request = new NominationDto(4, 11, 0, 12, "Statement", null);
            var view = new NominationViewDto(9, 4, 11, NominationStatus.Submitted, "Statement");
            var expected = request with { ProposerMemberId = 10 };
            _electionServiceMock.Setup(x => x.SubmitNominationAsync(3, expected, It.IsAny<CancellationToken>()))
                                .ReturnsAsync(view);

            var ok = await _controller.Nominate(3, request, CancellationToken.None) as OkObjectResult;

            Assert.That(ok?.Value, Is.EqualTo(view));
        }

        [Test]
        [Category("FR-39")]
        public async Task SetPhase_PollingNotReady_NamesTheUnmetItems_AndLeavesOutAdvisory()
        {
            SetUserContext(_controller, userId: 7);
            _approvalServiceMock.Setup(x => x.RunOrRequestAsync(3, Enums.ElectionApprovalAction.OpenPolling, 7, It.IsAny<IReadOnlyCollection<string>>(), null, It.IsAny<CancellationToken>()))
                                .ReturnsAsync(new ElectionApprovalRunResult(false, Constants.Elections.Replies.PollingNotReady, null));
            _electionServiceMock.Setup(x => x.GetReadinessAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(PartlyReady);

            AssertNamesUnmet(await _controller.SetPhase(3, Enums.ElectionPhase.Polling, CancellationToken.None));
        }

        // 37.16d. A period that already has members used to come back as "not ready".
        [TestCase("period-has-members", 409, "already has committee members")]
        [TestCase("not-ready", 400, "not ready for declaration")]
        [Category("FR-39")]
        public async Task Declare_Refused_SaysWhy(string error, int status, string detail)
        {
            SetUserContext(_controller, userId: 7);
            _approvalServiceMock.Setup(x => x.RunOrRequestAsync(3, Enums.ElectionApprovalAction.Declare, 7, It.IsAny<IReadOnlyCollection<string>>(), null, It.IsAny<CancellationToken>()))
                                .ReturnsAsync(new ElectionApprovalRunResult(false, error, null));

            var result = (ObjectResult)await _controller.Declare(3, CancellationToken.None);

            Assert.That(result.StatusCode, Is.EqualTo(status));
            Assert.That(((ProblemDetails)result.Value!).Detail, Does.Contain(detail));
        }

        [Test]
        [Category("FR-39")]
        public async Task Approve_PollingNotReady_ResolvesTheElection_AndNamesTheUnmetItems()
        {
            SetUserContext(_controller, userId: 7);
            _approvalServiceMock.Setup(x => x.ApproveAsync(55, 7, It.IsAny<IReadOnlyCollection<string>>(), It.IsAny<CancellationToken>()))
                                .ReturnsAsync((false, Constants.Elections.Replies.PollingNotReady));
            var access = new Mock<IElectionAccessService>();
            access.Setup(x => x.ElectionIdForAsync(ElectionIdLookup.Approval, 55, It.IsAny<CancellationToken>())).ReturnsAsync(3);
            _electionServiceMock.Setup(x => x.GetReadinessAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(PartlyReady);

            AssertNamesUnmet(await _controller.Approve(55, access.Object, CancellationToken.None));
        }
    }
}

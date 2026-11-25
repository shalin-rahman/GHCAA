using GHCAA.API.Controllers;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
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
            _documentServiceMock.Setup(x => x.GeneratePdfAsync(1, "form-1", It.IsAny<ElectionPrinter>(), It.IsAny<ElectionPrintMode?>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ElectionPdfResult(ElectionPrintOutcome.Printed, [1, 2, 3]));

            Assert.That(await _controller.Document(1, "form-1", null, CancellationToken.None), Is.InstanceOf<FileContentResult>());
        }

        [Test]
        public async Task Document_UnknownForm_Returns404()
        {
            _documentServiceMock.Setup(x => x.GeneratePdfAsync(1, "nope", It.IsAny<ElectionPrinter>(), It.IsAny<ElectionPrintMode?>(), It.IsAny<CancellationToken>())).ReturnsAsync(new ElectionPdfResult(ElectionPrintOutcome.NotFound));

            Assert.That(await _controller.Document(1, "nope", null, CancellationToken.None), Is.InstanceOf<NotFoundResult>());
        }

        // A print is recorded under the printer, so a caller with no user id gets no print.
        [Test]
        public async Task Document_NoUserId_Returns401()
        {
            SetUserContext(_controller, memberId: null, role: "ElectionOfficial");

            Assert.That(await _controller.Document(1, "ER-10", ElectionPrintMode.Filled, CancellationToken.None), Is.InstanceOf<UnauthorizedResult>());
            _documentServiceMock.Verify(x => x.GeneratePdfAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ElectionPrinter>(), It.IsAny<ElectionPrintMode?>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // 37.14s. The mode in the query reaches the service unchanged.
        [Test]
        public async Task Document_PassesThePrintMode()
        {
            _documentServiceMock.Setup(x => x.GeneratePdfAsync(1, "ER-10", It.IsAny<ElectionPrinter>(), ElectionPrintMode.Blank, It.IsAny<CancellationToken>())).ReturnsAsync(new ElectionPdfResult(ElectionPrintOutcome.Printed, [1]));

            Assert.That(await _controller.Document(1, "ER-10", ElectionPrintMode.Blank, CancellationToken.None), Is.InstanceOf<FileContentResult>());
        }

        // 37.14s. "mode=7" binds to an unnamed value; it must not fall through to a filled print.
        [Test]
        [Category("FR-25")]
        public async Task Document_UnknownPrintMode_Returns400()
        {
            var result = await _controller.Document(1, "ER-10", (ElectionPrintMode)7, CancellationToken.None);

            Assert.That((result as ObjectResult)!.StatusCode, Is.EqualTo(400));
            _documentServiceMock.Verify(x => x.GeneratePdfAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<ElectionPrinter>(), It.IsAny<ElectionPrintMode?>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // 37.14t. A filled print before the phase that produces the form is refused with a reason.
        [Test]
        [Category("FR-25")]
        public async Task Document_WrongPhase_Returns409()
        {
            _documentServiceMock.Setup(x => x.GeneratePdfAsync(1, "ER-29", It.IsAny<ElectionPrinter>(), It.IsAny<ElectionPrintMode?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ElectionPdfResult(ElectionPrintOutcome.WrongPhase));

            var result = await _controller.Document(1, "ER-29", ElectionPrintMode.Filled, CancellationToken.None) as ObjectResult;

            Assert.That(result!.StatusCode, Is.EqualTo(409));
        }

        // The printer's roles reach the service, so the record names who printed it.
        [Test]
        public async Task Document_PassesTheCallerRoles()
        {
            SetUserContext(_controller, memberId: null, role: "SuperAdmin", userId: 7);
            _documentServiceMock.Setup(x => x.GeneratePdfAsync(1, "ER-10", It.IsAny<ElectionPrinter>(), It.IsAny<ElectionPrintMode?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ElectionPdfResult(ElectionPrintOutcome.Printed, [1]));

            await _controller.Document(1, "ER-10", null, CancellationToken.None);

            _documentServiceMock.Verify(x => x.GeneratePdfAsync(1, "ER-10",
                It.Is<ElectionPrinter>(p => p.UserId == 7 && p.Roles.Contains("SuperAdmin")), null, It.IsAny<CancellationToken>()));
        }

        [Test]
        [Category("FR-25")]
        public async Task VerifyPrint_KnownId_ReturnsTheRecord()
        {
            var id = Guid.NewGuid();
            var dto = new ElectionFormPrintVerifyDto(id, "E-000001", "Election", "ER-10", "Nomination Paper", DateTime.UtcNow,
                "Chief Commissioner", ElectionPhase.Nomination, new string('a', 64), false, null);
            _documentServiceMock.Setup(x => x.VerifyPrintAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(dto);

            var result = await _controller.VerifyPrint(id, CancellationToken.None) as OkObjectResult;

            Assert.That(result!.Value, Is.EqualTo(dto));
        }

        [Test]
        public async Task VerifyPrint_UnknownId_Returns404()
        {
            _documentServiceMock.Setup(x => x.VerifyPrintAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((ElectionFormPrintVerifyDto?)null);

            Assert.That(await _controller.VerifyPrint(Guid.NewGuid(), CancellationToken.None), Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        [Category("FR-39")]
        public async Task ListPrints_WithPermissions_Returns200()
        {
            var rows = new List<ElectionFormPrintRowDto>
            {
                new(Guid.NewGuid(), "ER-10", DateTime.UtcNow, "Chief Commissioner", ElectionPhase.Nomination, null, null),
            };
            _documentServiceMock.Setup(x => x.ListPrintsAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(rows);

            var result = await _controller.ListPrints(1, CancellationToken.None) as OkObjectResult;

            Assert.That(result!.Value, Is.EqualTo(rows));
            _documentServiceMock.Setup(x => x.ListPrintsAsync(2, It.IsAny<CancellationToken>())).ReturnsAsync((IReadOnlyList<ElectionFormPrintRowDto>?)null);
            Assert.That(await _controller.ListPrints(2, CancellationToken.None), Is.InstanceOf<NotFoundResult>());
        }

        // The 403 comes from the action's filter, so the test runs that filter as MVC would.
        [Test]
        [Category("FR-39")]
        public async Task ListPrints_WithoutPermissions_Returns403()
        {
            var filter = typeof(ElectionsController).GetMethod(nameof(ElectionsController.ListPrints))!
                .GetCustomAttributes(typeof(GHCAA.API.Filters.RequireElectionPermissionAttribute), false)
                .Cast<GHCAA.API.Filters.RequireElectionPermissionAttribute>().Single();
            Assert.That(filter.Needed, Is.EqualTo(ElectionPermission.Count | ElectionPermission.ManageSetup));

            var access = new Mock<IElectionAccessService>();
            access.Setup(a => a.ElectionIdForAsync(ElectionIdLookup.Election, 1, It.IsAny<CancellationToken>())).ReturnsAsync(1);
            access.Setup(a => a.HasAsync(1, 5, It.IsAny<IReadOnlyCollection<string>>(), filter.Needed, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            var http = new Microsoft.AspNetCore.Http.DefaultHttpContext
            {
                User = new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity(
                    [new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "5")], "TestAuth")),
                RequestServices = new ServiceCollection().AddSingleton(access.Object).BuildServiceProvider(),
            };
            var context = new Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext(
                new ActionContext(http, new Microsoft.AspNetCore.Routing.RouteData(new Microsoft.AspNetCore.Routing.RouteValueDictionary { ["id"] = "1" }),
                    new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()),
                [], new Dictionary<string, object?>(), _controller);
            var ran = false;
            await filter.OnActionExecutionAsync(context, () => { ran = true; return Task.FromResult<Microsoft.AspNetCore.Mvc.Filters.ActionExecutedContext>(null!); });

            Assert.That(ran, Is.False);
            Assert.That((context.Result as ObjectResult)!.StatusCode, Is.EqualTo(403));
        }

        [Test]
        public async Task RevokePrint_Success_Returns204()
        {
            var id = Guid.NewGuid();
            _documentServiceMock.Setup(x => x.RevokePrintAsync(1, id, 10, "Misprint", It.IsAny<CancellationToken>())).ReturnsAsync((string?)null);

            Assert.That(await _controller.RevokePrint(1, id, new ElectionFormPrintRevokeDto("Misprint"), CancellationToken.None), Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task RevokePrint_AlreadyRevoked_Returns409()
        {
            _documentServiceMock.Setup(x => x.RevokePrintAsync(1, It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Constants.Elections.Replies.AlreadyRecorded);

            var result = await _controller.RevokePrint(1, Guid.NewGuid(), new ElectionFormPrintRevokeDto("Misprint"), CancellationToken.None) as ObjectResult;

            Assert.That(result!.StatusCode, Is.EqualTo(409));
        }

        [Test]
        public async Task RevokePrint_UnknownRecord_Returns404()
        {
            _documentServiceMock.Setup(x => x.RevokePrintAsync(1, It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Constants.Elections.Replies.NotFound);

            Assert.That(await _controller.RevokePrint(1, Guid.NewGuid(), new ElectionFormPrintRevokeDto("Misprint"), CancellationToken.None), Is.InstanceOf<NotFoundResult>());
        }

        [TestCase("Misprint", true)]
        [TestCase(" a b c ", true)]
        [TestCase("a  ", false)]
        [TestCase("     ", false)]
        [TestCase("", false)]
        public void RevokePrintDto_NeedsThreeVisibleCharacters(string reason, bool valid)
        {
            // MVC reads these attributes from the record's constructor parameter, so the test does too.
            var attrs = typeof(ElectionFormPrintRevokeDto).GetConstructors().Single().GetParameters().Single()
                .GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.ValidationAttribute), true)
                .Cast<System.ComponentModel.DataAnnotations.ValidationAttribute>().ToList();
            Assert.That(attrs, Has.Count.EqualTo(3));
            var ok = System.ComponentModel.DataAnnotations.Validator.TryValidateValue(
                reason, new System.ComponentModel.DataAnnotations.ValidationContext(new object()), null, attrs);
            Assert.That(ok, Is.EqualTo(valid));
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

        // 37.16k. The QR check on a print has its own limit, so scanning prints cannot use up the card check.
        [Test]
        [Category("FR-39")]
        public void PrintVerification_UsesItsOwnRateLimitPolicy()
        {
            static string Policy(string method) => typeof(ElectionsController).GetMethod(method)!
                .GetCustomAttributes(typeof(Microsoft.AspNetCore.RateLimiting.EnableRateLimitingAttribute), false)
                .Cast<Microsoft.AspNetCore.RateLimiting.EnableRateLimitingAttribute>().Single().PolicyName!;

            Assert.That(Policy(nameof(ElectionsController.VerifyPrint)), Is.EqualTo(Constants.RateLimitPolicies.PrintVerification));
            Assert.That(Policy(nameof(ElectionsController.VerifyDocument)), Is.EqualTo(Constants.RateLimitPolicies.CredentialVerification));
        }

        [TestCase(0, 0, 1, 1)]
        [TestCase(5000, 9999, 1000, 1440)]
        [TestCase(20, 30, 20, 30)]
        [Category("FR-39")]
        public void PrintVerifySettings_AreClampedToTheDtoRange(int limit, int minutes, int expectedLimit, int expectedMinutes)
        {
            var config = new Mock<IOrgConfigService>();
            config.Setup(c => c.GetConfigAsync()).ReturnsAsync(new OrgConfigDto
            {
                Elections = new ElectionSettingsDto { PrintVerifyLimit = limit, PrintVerifyWindowMinutes = minutes }
            });

            var result = GHCAA.API.Extensions.RateLimitingExtensions.ReadPrintVerifySettings(ContextWith(config.Object));

            Assert.That(result, Is.EqualTo((expectedLimit, expectedMinutes)));
        }

        [Test]
        [Category("FR-39")]
        public void PrintVerifySettings_ConfigReadFails_UsesTheDefaults()
        {
            var config = new Mock<IOrgConfigService>();
            config.Setup(c => c.GetConfigAsync()).ThrowsAsync(new InvalidOperationException("database down"));

            var result = GHCAA.API.Extensions.RateLimitingExtensions.ReadPrintVerifySettings(ContextWith(config.Object));

            Assert.That(result, Is.EqualTo((60, 15)));
        }

        private static Microsoft.AspNetCore.Http.DefaultHttpContext ContextWith(IOrgConfigService config) => new()
        {
            RequestServices = new ServiceCollection().AddSingleton(config).BuildServiceProvider()
        };

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

    // 37.16k. Runs the real pipeline in Production so the real limits apply. The print limit is
    // lowered to 2 through OrgConfig, which is where an admin would change it.
    [TestFixture]
    [NonParallelizable]
    public class ElectionsPrintVerificationRateLimitTests
    {
        private GHCAA.Tests.Integration.ErrorResponseShapeTestFactory _factory = null!;
        private IDisposable _app = null!;
        private HttpClient _client = null!;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _factory = new GHCAA.Tests.Integration.ErrorResponseShapeTestFactory();
            var app = _factory.WithWebHostBuilder(b => b.ConfigureTestServices(services =>
            {
                var real = services.Last(d => d.ServiceType == typeof(IOrgConfigService));
                services.Remove(real);
                services.AddScoped<IOrgConfigService>(sp => new LowPrintLimit(Resolve(sp, real)));
            }));
            _app = app;
            _client = app.CreateClient();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _client.Dispose();
            _app.Dispose();
            _factory.Dispose();
        }

        [Test]
        [Category("FR-39")]
        public async Task CardVerification_StillAnswers_AfterPrintLimitIsHit()
        {
            var printUrl = $"/api/elections/print-records/{Guid.NewGuid()}/verification";

            for (var i = 0; i < 2; i++)
                Assert.That((int)(await _client.GetAsync(printUrl)).StatusCode, Is.Not.EqualTo(429));
            Assert.That((int)(await _client.GetAsync(printUrl)).StatusCode, Is.EqualTo(429));

            Assert.That((int)(await _client.GetAsync("/api/verify/NOSUCHCODE")).StatusCode, Is.Not.EqualTo(429));
        }

        private static IOrgConfigService Resolve(IServiceProvider sp, ServiceDescriptor d) =>
            d.ImplementationFactory is not null ? (IOrgConfigService)d.ImplementationFactory(sp)
            : d.ImplementationInstance is not null ? (IOrgConfigService)d.ImplementationInstance
            : (IOrgConfigService)ActivatorUtilities.CreateInstance(sp, d.ImplementationType!);

        private sealed class LowPrintLimit(IOrgConfigService inner) : IOrgConfigService
        {
            public async Task<OrgConfigDto> GetConfigAsync()
            {
                var config = await inner.GetConfigAsync();
                return config with { Elections = config.Elections with { PrintVerifyLimit = 2 } };
            }

            public Task UpdateConfigAsync(OrgConfigDto dto, string updatedByAdminId) => inner.UpdateConfigAsync(dto, updatedByAdminId);
        }
    }
}

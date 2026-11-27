using GHCAA.API.Controllers;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GHCAA.Tests.Controllers
{
    [TestFixture]
    public class FinancialsControllerTests : ControllerTestBase
    {
        private Mock<IFinancialService> _financialServiceMock;
        private Mock<IFileValidationService> _fileValidationServiceMock;
        private FinancialsController _controller;

        [SetUp]
        public void Setup()
        {
            _financialServiceMock = new Mock<IFinancialService>();
            _fileValidationServiceMock = new Mock<IFileValidationService>();
            _fileValidationServiceMock.Setup(x => x.Validate(It.IsAny<System.IO.Stream>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<long>(), It.IsAny<FileCategory>(), It.IsAny<long>()))
                                      .Returns(new FileValidationResult { IsValid = true });
            _controller = new FinancialsController(_financialServiceMock.Object, _fileValidationServiceMock.Object);
        }

        [Test]
        public async Task GetMyPaymentHistory_ReturnsOk()
        {
            SetUserContext(_controller, 10, "Admin");
            _financialServiceMock.Setup(x => x.GetMemberPaymentHistoryAsync(10, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(new List<PaymentHistoryDto>());

            var result = await _controller.GetMyPaymentHistory(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetMyPaymentHistory_SuperAdmin_ReturnsEmptyList()
        {
            SetUserContext(_controller, null, "SuperAdmin");
            var result = await _controller.GetMyPaymentHistory(CancellationToken.None);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult!.Value, Is.Empty);
        }

        [Category("FR-22")]
        [Test]
        public async Task RecordPayment_ReturnsOk()
        {
            SetUserContext(_controller, 10, "Admin");
            var dto = new CreatePaymentHistoryDto { Amount = 100 };
            _financialServiceMock.Setup(x => x.RecordPaymentAsync(dto, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(new PaymentHistoryDto { Id = 1 });

            var result = await _controller.RecordPayment(dto, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Category("FR-07")]
        [Test]
        public async Task GetMyDues_ReturnsOk()
        {
            SetUserContext(_controller, 10, "Admin");
            _financialServiceMock.Setup(x => x.GetMemberDuesAsync(10, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(new List<MembershipDueDto>());

            var result = await _controller.GetMyDues(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetMyDues_SuperAdmin_ReturnsEmptyList()
        {
            SetUserContext(_controller, null, "SuperAdmin");
            var result = await _controller.GetMyDues(CancellationToken.None);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult!.Value, Is.Empty);
        }

        [Test]
        public async Task GenerateAnnualDues_ReturnsOk()
        {
            _financialServiceMock.Setup(x => x.GenerateAnnualDuesAsync(2023, It.IsAny<CancellationToken>()))
                                 .Returns(Task.CompletedTask);

            var result = await _controller.GenerateAnnualDues(2023, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        // 80.13: GetMyDues's system-admin branch (no MemberId claim, not SuperAdmin) resolves the
        // target memberId via IFinancialService.GetMemberIdForUserAsync instead of a raw EF read.
        [Test]
        public async Task GetMyDues_SystemAdminWithoutMemberIdClaim_ResolvesMemberIdViaService()
        {
            SetUserContext(_controller, memberId: null, role: "Admin", userId: 7);
            _financialServiceMock.Setup(x => x.GetMemberIdForUserAsync(7, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(42);
            _financialServiceMock.Setup(x => x.GetMemberDuesAsync(42, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(new List<MembershipDueDto>());

            var result = await _controller.GetMyDues(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            _financialServiceMock.Verify(x => x.GetMemberDuesAsync(42, It.IsAny<CancellationToken>()), Times.Once);
        }

        // 80.13: DownloadReceipt's non-admin ownership check now goes through
        // IFinancialService.GetPaymentOwnerMemberIdAsync instead of a raw PaymentHistories read.
        [Test]
        public async Task DownloadReceipt_NonAdminOwnsPayment_ReturnsFile()
        {
            SetUserContext(_controller, memberId: 10, role: "Member");
            _financialServiceMock.Setup(x => x.GetPaymentOwnerMemberIdAsync(5, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(10);
            _financialServiceMock.Setup(x => x.GenerateTaxReceiptAsync(5, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(new byte[] { 1, 2, 3 });

            var result = await _controller.DownloadReceipt(5, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<FileContentResult>());
        }

        [Test]
        public async Task DownloadReceipt_NonAdminDoesNotOwnPayment_Returns403Problem()
        {
            SetUserContext(_controller, memberId: 10, role: "Member");
            _financialServiceMock.Setup(x => x.GetPaymentOwnerMemberIdAsync(5, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(999);

            var result = await _controller.DownloadReceipt(5, CancellationToken.None);

            // 84.41: Forbid(string) used to throw at runtime because it takes a scheme name, not
            // a message. The fix returns a ProblemDetails 403 instead.
            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var obj = (ObjectResult)result;
            Assert.That(obj.StatusCode, Is.EqualTo(403));
            Assert.That(((ProblemDetails)obj.Value!).Detail, Is.EqualTo("You can only download your own receipts."));
        }

        [Test]
        public async Task DownloadReceipt_NonAdminPaymentDoesNotExist_ReturnsNotFound()
        {
            SetUserContext(_controller, memberId: 10, role: "Member");
            _financialServiceMock.Setup(x => x.GetPaymentOwnerMemberIdAsync(5, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync((int?)null);

            var result = await _controller.DownloadReceipt(5, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Category("FR-23")]
        [Test]
        public async Task Reject_BlankReason_Returns400()
        {
            SetUserContext(_controller, memberId: null, role: "Admin", userId: 7);
            _financialServiceMock.Setup(x => x.RejectPaymentAsync(5, 7, "   ", It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(new PaymentReviewResult { Outcome = PaymentReviewOutcome.InvalidReason });

            var result = await _controller.RejectPayment(5, new RejectPaymentDto { Reason = "   " }, CancellationToken.None);

            var obj = (ObjectResult)result;
            Assert.That(obj.StatusCode, Is.EqualTo(400));
            // The same shape [ApiController] gives a DataAnnotations failure.
            var problem = (ValidationProblemDetails)obj.Value!;
            Assert.That(problem.Errors.ContainsKey(nameof(RejectPaymentDto.Reason)), Is.True);
        }

        [Category("FR-23")]
        [Test]
        public async Task Verify_Settled_Returns409()
        {
            SetUserContext(_controller, memberId: null, role: "Admin", userId: 7);
            _financialServiceMock.Setup(x => x.VerifyPaymentAsync(5, 7, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(new PaymentReviewResult { Outcome = PaymentReviewOutcome.Settled });

            var result = await _controller.VerifyPayment(5, CancellationToken.None);

            var obj = (ObjectResult)result;
            Assert.That(obj.StatusCode, Is.EqualTo(409));
        }

        [Test]
        public async Task Verify_UnknownId_Returns404()
        {
            SetUserContext(_controller, memberId: null, role: "Admin", userId: 7);
            _financialServiceMock.Setup(x => x.VerifyPaymentAsync(99, 7, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(new PaymentReviewResult { Outcome = PaymentReviewOutcome.NotFound });

            var result = await _controller.VerifyPayment(99, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Category("FR-21")]
        [Test]
        public async Task Verify_Pending_ReturnsDueIdAndPassesAdminId()
        {
            SetUserContext(_controller, memberId: null, role: "Admin", userId: 7);
            _financialServiceMock.Setup(x => x.VerifyPaymentAsync(5, 7, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(new PaymentReviewResult { Outcome = PaymentReviewOutcome.Done, DueId = 3 });

            var result = await _controller.VerifyPayment(5, CancellationToken.None);

            var ok = (OkObjectResult)result;
            Assert.That(ok.Value!.GetType().GetProperty("dueId")!.GetValue(ok.Value), Is.EqualTo(3));
        }

        [Test]
        public async Task Reject_Pending_Returns204()
        {
            SetUserContext(_controller, memberId: null, role: "Admin", userId: 7);
            _financialServiceMock.Setup(x => x.RejectPaymentAsync(5, 7, "Wrong TrxID", It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(new PaymentReviewResult { Outcome = PaymentReviewOutcome.Done });

            var result = await _controller.RejectPayment(5, new RejectPaymentDto { Reason = "Wrong TrxID" }, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Category("FR-23")]
        [TestCase(nameof(FinancialsController.GetPendingPayments))]
        [TestCase(nameof(FinancialsController.VerifyPayment))]
        [TestCase(nameof(FinancialsController.RejectPayment))]
        public void Pending_AsMember_IsAdminOnly(string action)
        {
            // A Member token fails the AdminOnly policy, so the framework answers 403 before the action runs.
            var attr = typeof(FinancialsController).GetMethod(action)!
                .GetCustomAttributes(typeof(Microsoft.AspNetCore.Authorization.AuthorizeAttribute), false)
                .Cast<Microsoft.AspNetCore.Authorization.AuthorizeAttribute>()
                .Single();

            Assert.That(attr.Policy, Is.EqualTo(GHCAA.Domain.Constants.Policies.AdminOnly));
        }
    }
}

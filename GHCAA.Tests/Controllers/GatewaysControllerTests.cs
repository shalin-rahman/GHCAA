using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Controllers;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Controllers
{
    [TestFixture]
    public class GatewaysControllerTests : ControllerTestBase
    {
        private Mock<IPaymentGatewayFactory> _gatewayFactoryMock = null!;
        private Mock<IFinancialService> _financialServiceMock = null!;
        private Mock<IMemberService> _memberServiceMock = null!;
        private Mock<ILogger<GatewaysController>> _loggerMock = null!;
        private IConfiguration _gatewayTestConfig = null!;
        private GatewaysController _controller = null!;
        private Member _testMember = null!;

        [SetUp]
        public async Task Setup()
        {
            _gatewayFactoryMock = new Mock<IPaymentGatewayFactory>();
            _financialServiceMock = new Mock<IFinancialService>();
            _memberServiceMock = new Mock<IMemberService>();
            _loggerMock = new Mock<ILogger<GatewaysController>>();
            var json = """
{
  "PaymentGateways": { "EnabledMethods": [ "SSLCommerz", "BkashGateway" ] },
  "GeneralSettings": { "AssociationNamePrefix": "TEST-", "Currency": "BDT" },
  "AppSettings": { "PublicApiBaseUrl": "http://localhost:5000" }
}
""";
            _gatewayTestConfig = new ConfigurationBuilder()
                .AddJsonStream(new MemoryStream(Encoding.UTF8.GetBytes(json)))
                .Build();

            var defaultGatewayMock = new Mock<IPaymentGatewayService>();
            defaultGatewayMock.Setup(x => x.VerifyCallbackAsync(It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _gatewayFactoryMock.Setup(x => x.GetGateway(It.IsAny<Enums.PaymentGateway>())).Returns(defaultGatewayMock.Object);

            _controller = new GatewaysController(
                _gatewayFactoryMock.Object,
                _financialServiceMock.Object,
                _memberServiceMock.Object,
                _context,
                _loggerMock.Object,
                _gatewayTestConfig);

            _testMember = await CreateAndSaveTestMemberAsync("Test Member", "test@test.com", "123", "123");

            await EnsureSslCommerzPaymentConfigExistsAsync();

            SetMemberContext(_controller, _testMember.Id);
        }

        private async Task EnsureSslCommerzPaymentConfigExistsAsync()
        {
            var ssl = await _context.PaymentConfigurations.FirstOrDefaultAsync(p => p.Gateway == Enums.PaymentGateway.SSLCommerz);
            if (ssl == null)
            {
                _context.PaymentConfigurations.Add(new PaymentConfiguration
                {
                    Method = Enums.PaymentMethod.CreditCard,
                    DisplayName = "SSLCommerz",
                    Gateway = Enums.PaymentGateway.SSLCommerz,
                    IsEnabled = true,
                    GatewayPublicKey = "test_store",
                    GatewaySecretKey = "test_secret",
                    IsSandbox = true,
                    RequiresReceipt = false,
                    RequiresReference = false
                });
                await _context.SaveChangesAsync();
                return;
            }

            if (ssl.IsEnabled) return;
            ssl.IsEnabled = true;
            ssl.GatewayPublicKey ??= "test_store";
            ssl.GatewaySecretKey ??= "test_secret";
            await _context.SaveChangesAsync();
        }

        [TearDown]
        public void TearDownCleanup()
        {
            ApplicationDbContext.IsSeedDisabled = false;
        }

        [Test]
        public async Task InitiatePayment_ReturnsOk_WhenSuccessful()
        {
            var request = new GatewaysController.InitiatePaymentRequest
            {
                Amount = 100,
                Gateway = Enums.PaymentGateway.SSLCommerz,
                Reference = "Registration"
            };

            var gatewayMock = new Mock<IPaymentGatewayService>();
            gatewayMock.Setup(x => x.InitiatePaymentAsync(It.IsAny<PaymentGatewayInitiationDto>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PaymentGatewayResponseDto { Success = true, GatewayUrl = "http://pay.com" });

            _gatewayFactoryMock.Setup(x => x.GetGateway(request.Gateway)).Returns(gatewayMock.Object);

            var result = await _controller.InitiatePayment(request, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            _financialServiceMock.Verify(x => x.RecordPaymentAsync(It.IsAny<CreatePaymentHistoryDto>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task InitiatePayment_ReturnsUnauthorized_WhenMembershipPaymentAndAnonymous()
        {
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            var request = new GatewaysController.InitiatePaymentRequest
            {
                Amount = 100,
                Gateway = Enums.PaymentGateway.SSLCommerz,
                Reference = "Registration"
            };

            var result = await _controller.InitiatePayment(request, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
            _financialServiceMock.Verify(x => x.RecordPaymentAsync(It.IsAny<CreatePaymentHistoryDto>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task InitiatePayment_ReturnsBadRequest_WhenEventAmountDoesNotMatch()
        {
            _controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

            var ev = new AlumniEvent { Title = "Paid Event", Description = "D", Location = "L", RegistrationFee = 500 };
            _context.AlumniEvents.Add(ev);
            await _context.SaveChangesAsync();

            var registration = new EventRegistration
            {
                EventId = ev.Id,
                PaymentReference = "EVT-REG-AMT-TEST",
                Status = Enums.EventRegistrationStatus.Pending,
                IsNonMember = true
            };
            _context.EventRegistrations.Add(registration);
            await _context.SaveChangesAsync();

            var request = new GatewaysController.InitiatePaymentRequest
            {
                Amount = 99,
                Gateway = Enums.PaymentGateway.SSLCommerz,
                Reference = "EVT-REG-AMT-TEST"
            };

            var result = await _controller.InitiatePayment(request, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            _financialServiceMock.Verify(x => x.RecordPaymentAsync(It.IsAny<CreatePaymentHistoryDto>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task SSLCommerzCallback_AutoApprovesRegistration_WhenValid()
        {
            var txnId = "TXN123";
            var ev = new AlumniEvent { Title = "Event", Description = "Desc", Location = "Loc", RegistrationFee = 100 };
            _context.AlumniEvents.Add(ev);
            await _context.SaveChangesAsync();
            var eventId = ev.Id;

            var registration = new EventRegistration
            {
                EventId = eventId,
                PaymentReference = "EVT-REG-ABCD",
                Status = Enums.EventRegistrationStatus.Pending
            };
            _context.EventRegistrations.Add(registration);
            await _context.SaveChangesAsync();
            var registrationId = registration.Id;

            var payment = new PaymentHistory
            {
                MemberId = _testMember.Id,
                Amount = 100,
                TransactionId = txnId,
                Status = Enums.PaymentStatus.Pending,
                // 82.32: HandleSuccessfulPayment now discriminates an event-registration payment by
                // FinancialCategory (set at InitiatePayment for every real payment on this path),
                // not by parsing Notes case-sensitively.
                FinancialCategory = Enums.FinancialCategory.RegistrationFee,
                Notes = "Initiated via SSLCommerz. Ref: EVT-REG-ABCD"
            };
            _context.PaymentHistories.Add(payment);
            await _context.SaveChangesAsync();

            var callbackData = new Dictionary<string, string>
            {
                { "status", "VALID" },
                { "tran_id", txnId }
            };

            var result = await _controller.SSLCommerzCallback(callbackData, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<RedirectResult>());

            var updatedReg = await _context.EventRegistrations.FirstOrDefaultAsync(r => r.Id == registrationId);
            Assert.That(updatedReg!.Status, Is.EqualTo(Enums.EventRegistrationStatus.Approved));

            _financialServiceMock.Verify(x => x.UpdatePaymentStatusAsync(It.IsAny<int>(), Enums.PaymentStatus.Completed, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Callback_ShouldAutoApproveRegistration_EvenWithComplexNotes()
        {
            var txnId = "TXN456";
            var ev = new AlumniEvent { Title = "Event Complex", Description = "D", Location = "L", RegistrationFee = 500 };
            _context.AlumniEvents.Add(ev);
            await _context.SaveChangesAsync();

            var reg = new EventRegistration
            {
                EventId = ev.Id,
                PaymentReference = "EVT-REG-COMPLEX-99",
                Status = Enums.EventRegistrationStatus.Pending
            };
            _context.EventRegistrations.Add(reg);

            // Note the space and extra text after the unique prefix
            var payment = new PaymentHistory
            {
                MemberId = _testMember.Id,
                Amount = 500,
                TransactionId = txnId,
                Status = Enums.PaymentStatus.Pending,
                FinancialCategory = Enums.FinancialCategory.RegistrationFee,
                Notes = "Initiated via BkashGateway. Ref: EVT-REG-COMPLEX-99 (Optional extra text here)"
            };
            _context.PaymentHistories.Add(payment);
            await _context.SaveChangesAsync();

            // Simulate a callback that triggers HandleSuccessfulPayment
            var callbackData = new Dictionary<string, string> { { "status", "VALID" }, { "tran_id", txnId } };
            await _controller.SSLCommerzCallback(callbackData, CancellationToken.None);

            var updatedReg = await _context.EventRegistrations.FirstOrDefaultAsync(r => r.PaymentReference == "EVT-REG-COMPLEX-99");
            Assert.That(updatedReg!.Status, Is.EqualTo(Enums.EventRegistrationStatus.Approved));
        }

        [Test]
        public async Task SSLCommerzCallback_MarksFailedAndDoesNotApprove_WhenReportedAmountMismatches()
        {
            // 29B.2: a callback that REPORTS an amount which disagrees with the recorded payment must be
            // treated as a discrepancy (marked Failed) and must NOT auto-approve the registration —
            // even though VerifyCallbackAsync (the primary gate) is mocked to succeed.
            var txnId = "TXN-MISMATCH";
            var ev = new AlumniEvent { Title = "Event", Description = "Desc", Location = "Loc", RegistrationFee = 100 };
            _context.AlumniEvents.Add(ev);
            await _context.SaveChangesAsync();

            var registration = new EventRegistration
            {
                EventId = ev.Id,
                PaymentReference = "EVT-REG-MISMATCH",
                Status = Enums.EventRegistrationStatus.Pending
            };
            _context.EventRegistrations.Add(registration);
            await _context.SaveChangesAsync();

            var payment = new PaymentHistory
            {
                MemberId = _testMember.Id,
                Amount = 100,
                TransactionId = txnId,
                Status = Enums.PaymentStatus.Pending,
                Notes = "Initiated via SSLCommerz. Ref: EVT-REG-MISMATCH"
            };
            _context.PaymentHistories.Add(payment);
            await _context.SaveChangesAsync();

            var callbackData = new Dictionary<string, string>
            {
                { "status", "VALID" },
                { "tran_id", txnId },
                { "amount", "999" } // gateway reports a wildly different amount
            };

            await _controller.SSLCommerzCallback(callbackData, CancellationToken.None);

            var updatedReg = await _context.EventRegistrations.FirstOrDefaultAsync(r => r.Id == registration.Id);
            Assert.That(updatedReg!.Status, Is.EqualTo(Enums.EventRegistrationStatus.Pending));

            _financialServiceMock.Verify(x => x.UpdatePaymentStatusAsync(payment.Id, Enums.PaymentStatus.Failed, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
            _financialServiceMock.Verify(x => x.UpdatePaymentStatusAsync(It.IsAny<int>(), Enums.PaymentStatus.Completed, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task BkashCallbackGet_AutoApprovesRegistration_WhenValid()
        {
            var txnId = "BKASH123";
            var ev = new AlumniEvent { Title = "Bkash Event", Description = "Desc", Location = "Loc", RegistrationFee = 200 };
            _context.AlumniEvents.Add(ev);
            await _context.SaveChangesAsync();

            var registration = new EventRegistration
            {
                EventId = ev.Id,
                PaymentReference = "EVT-REG-BK",
                Status = Enums.EventRegistrationStatus.Pending
            };
            _context.EventRegistrations.Add(registration);
            await _context.SaveChangesAsync();

            var payment = new PaymentHistory
            {
                MemberId = _testMember.Id,
                Amount = 200,
                TransactionId = txnId,
                Status = Enums.PaymentStatus.Pending,
                FinancialCategory = Enums.FinancialCategory.RegistrationFee,
                Notes = "Initiated via BkashGateway. Ref: EVT-REG-BK"
            };
            _context.PaymentHistories.Add(payment);
            await _context.SaveChangesAsync();

            var result = await _controller.BkashCallbackGet(txnId, "success", CancellationToken.None);

            Assert.That(result, Is.InstanceOf<RedirectResult>());

            var updatedReg = await _context.EventRegistrations.FirstOrDefaultAsync(r => r.Id == registration.Id);
            Assert.That(updatedReg!.Status, Is.EqualTo(Enums.EventRegistrationStatus.Approved));
        }

        // Regression coverage for docs/TODO.md 80.11: a webhook used to be dead-ended at a bool,
        // so a real gateway confirmation could never reach HandleSuccessfulPayment. This proves the
        // webhook path now drives the same completion logic the redirect callback uses.
        [Test]
        public async Task GatewayWebhook_MarksPaymentCompleted_WhenGatewayConfirmsValid()
        {
            const string trxId = "SSL-WEBHOOK-1";
            var payment = new PaymentHistory
            {
                MemberId = _testMember.Id,
                Amount = 500,
                TransactionId = trxId,
                Status = Enums.PaymentStatus.Pending
            };
            _context.PaymentHistories.Add(payment);
            await _context.SaveChangesAsync();

            var webhookGatewayMock = new Mock<IPaymentGatewayService>();
            webhookGatewayMock
                .Setup(x => x.ProcessWebhookAsync(It.IsAny<Stream>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new PaymentWebhookResultDto
                {
                    IsValid = true,
                    TransactionId = trxId,
                    ConfirmedAmount = 500,
                    GatewayPaymentId = "gw-ref-1"
                });
            _gatewayFactoryMock.Setup(x => x.GetGateway(Enums.PaymentGateway.SSLCommerz)).Returns(webhookGatewayMock.Object);

            _controller.ControllerContext.HttpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("irrelevant-for-this-mock"));

            var result = await _controller.GatewayWebhook("SSLCommerz", CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            _financialServiceMock.Verify(x => x.UpdatePaymentStatusAsync(payment.Id, Enums.PaymentStatus.Completed, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GatewayWebhook_DoesNotTouchPayment_WhenGatewayReportsInvalid()
        {
            const string trxId = "SSL-WEBHOOK-2";
            var payment = new PaymentHistory
            {
                MemberId = _testMember.Id,
                Amount = 500,
                TransactionId = trxId,
                Status = Enums.PaymentStatus.Pending
            };
            _context.PaymentHistories.Add(payment);
            await _context.SaveChangesAsync();

            var webhookGatewayMock = new Mock<IPaymentGatewayService>();
            webhookGatewayMock
                .Setup(x => x.ProcessWebhookAsync(It.IsAny<Stream>(), It.IsAny<IDictionary<string, string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(PaymentWebhookResultDto.Invalid());
            _gatewayFactoryMock.Setup(x => x.GetGateway(Enums.PaymentGateway.SSLCommerz)).Returns(webhookGatewayMock.Object);

            _controller.ControllerContext.HttpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("irrelevant-for-this-mock"));

            var result = await _controller.GatewayWebhook("SSLCommerz", CancellationToken.None);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
            _financialServiceMock.Verify(x => x.UpdatePaymentStatusAsync(It.IsAny<int>(), It.IsAny<Enums.PaymentStatus>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}

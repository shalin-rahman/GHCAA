using System;
using System.Collections.Generic;
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
    [TestFixture]
    public class GatewaysControllerTests : ControllerTestBase
    {
        private Mock<IPaymentGatewayFactory> _gatewayFactoryMock;
        private Mock<IFinancialService> _financialServiceMock;
        private Mock<IMemberService> _memberServiceMock;
        private Mock<ILogger<GatewaysController>> _loggerMock;
        private GatewaysController _controller;
        private Member _testMember;

        [SetUp]
        public async Task Setup()
        {
            _gatewayFactoryMock = new Mock<IPaymentGatewayFactory>();
            _financialServiceMock = new Mock<IFinancialService>();
            _memberServiceMock = new Mock<IMemberService>();
            _loggerMock = new Mock<ILogger<GatewaysController>>();
            var configMock = new Mock<IConfiguration>();

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
                configMock.Object);

            _testMember = await CreateAndSaveTestMemberAsync("Test Member", "test@test.com", "123", "123");
            
            SetMemberContext(_controller, _testMember.Id);
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
                Reference = "Registration",
                BaseUrl = "http://api.com"
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
    }
}

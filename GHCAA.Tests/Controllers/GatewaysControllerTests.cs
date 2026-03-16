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
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Controllers
{
    [TestFixture]
    public class GatewaysControllerTests
    {
        private Mock<IPaymentGatewayFactory> _gatewayFactoryMock;
        private Mock<IFinancialService> _financialServiceMock;
        private Mock<IMemberService> _memberServiceMock;
        private ApplicationDbContext _dbContext;
        private Mock<ILogger<GatewaysController>> _loggerMock;
        private GatewaysController _controller;

        [SetUp]
        public void Setup()
        {
            ApplicationDbContext.IsSeedDisabled = true;
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _dbContext = new ApplicationDbContext(options);

            _gatewayFactoryMock = new Mock<IPaymentGatewayFactory>();
            _financialServiceMock = new Mock<IFinancialService>();
            _memberServiceMock = new Mock<IMemberService>();
            _loggerMock = new Mock<ILogger<GatewaysController>>();

            _controller = new GatewaysController(
                _gatewayFactoryMock.Object,
                _financialServiceMock.Object,
                _memberServiceMock.Object,
                _dbContext,
                _loggerMock.Object);

            SetUserContext(10); // Member 10
            
            // Add the member to the DB as well so filters/logic doesn't fail
            _dbContext.Members.Add(new Member 
            { 
                Id = 10, 
                FullName = "Test Member", 
                Email = "test@test.com", 
                NID = "123", 
                MobileNo = "123", 
                FatherName = "F", MotherName = "M", 
                PresentAddress = "A", PermanentAddress = "B",
                EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "P",
                HighestCertificate = "X", HighestCertificateGroup = "G", HighestCertificateSubject = "S",
                GHCLastCertificate = "GC", GHCLastCertificateGroup = "GG", GHCLastCertificateSubject = "GS",
                ProfessionalSector = "PS", Designation = "D"
            });
            _dbContext.SaveChanges();
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext?.Dispose();
        }

        private void SetUserContext(int memberId)
        {
            var claims = new List<Claim> {
                new Claim("MemberId", memberId.ToString())
            };
            var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthentication"));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
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
            var ev = new AlumniEvent { Title = "Event", Description = "Desc", Location = "Loc" };
            _dbContext.AlumniEvents.Add(ev);
            await _dbContext.SaveChangesAsync();
            var eventId = ev.Id;
            
            var registration = new EventRegistration 
            { 
                EventId = eventId, 
                PaymentReference = "EVT-REG-ABCD", 
                Status = Enums.EventRegistrationStatus.Pending 
            };
            _dbContext.EventRegistrations.Add(registration);
            await _dbContext.SaveChangesAsync();
            var registrationId = registration.Id;

            var payment = new PaymentHistory
            {
                MemberId = 10,
                Amount = 100,
                TransactionId = txnId,
                Status = Enums.PaymentStatus.Pending,
                Notes = "Initiated via SSLCommerz. Ref: EVT-REG-ABCD"
            };
            _dbContext.PaymentHistories.Add(payment);
            await _dbContext.SaveChangesAsync();

            var callbackData = new Dictionary<string, string>
            {
                { "status", "VALID" },
                { "tran_id", txnId }
            };

            var result = await _controller.SSLCommerzCallback(callbackData, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<RedirectResult>());
            
            var updatedReg = await _dbContext.EventRegistrations.FirstOrDefaultAsync(r => r.Id == registrationId);
            Assert.That(updatedReg.Status, Is.EqualTo(Enums.EventRegistrationStatus.Approved));
            
            _financialServiceMock.Verify(x => x.UpdatePaymentStatusAsync(It.IsAny<int>(), Enums.PaymentStatus.Completed, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Controllers;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Controllers
{
    [TestFixture]
    public class FinancialsControllerTests
    {
        private Mock<IFinancialService> _financialServiceMock;
        private ApplicationDbContext _dbContext;
        private FinancialsController _controller;

        [SetUp]
        public void Setup()
        {
            _financialServiceMock = new Mock<IFinancialService>();
            
            // Use InMemory database for DbContext dependency
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "FinancialsTestDb")
                .Options;
            _dbContext = new ApplicationDbContext(options);

            _controller = new FinancialsController(_financialServiceMock.Object, _dbContext);
        }

        private void SetUserContext(string? memberId = "10", string role = "Admin")
        {
            var claims = new List<Claim> {
                new Claim(ClaimTypes.Role, role)
            };
            if (memberId != null) 
                claims.Add(new Claim("MemberId", memberId));

            var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthentication"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }

        [Test]
        public async Task GetMyPaymentHistory_ReturnsOk()
        {
            SetUserContext();
            _financialServiceMock.Setup(x => x.GetMemberPaymentHistoryAsync(10, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(new List<PaymentHistoryDto>());

            var result = await _controller.GetMyPaymentHistory(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetMyPaymentHistory_SuperAdmin_ReturnsEmptyList()
        {
            SetUserContext(null, "SuperAdmin");
            var result = await _controller.GetMyPaymentHistory(CancellationToken.None);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.Value, Is.Empty);
        }

        [Test]
        public async Task RecordPayment_ReturnsOk()
        {
            SetUserContext();
            var dto = new CreatePaymentHistoryDto { Amount = 100 };
            _financialServiceMock.Setup(x => x.RecordPaymentAsync(dto, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(new PaymentHistoryDto { Id = 1 });

            var result = await _controller.RecordPayment(dto, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetMyDues_ReturnsOk()
        {
            SetUserContext();
            _financialServiceMock.Setup(x => x.GetMemberDuesAsync(10, It.IsAny<CancellationToken>()))
                                 .ReturnsAsync(new List<MembershipDueDto>());

            var result = await _controller.GetMyDues(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetMyDues_SuperAdmin_ReturnsEmptyList()
        {
            SetUserContext(null, "SuperAdmin");
            var result = await _controller.GetMyDues(CancellationToken.None);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.Value, Is.Empty);
        }

        [Test]
        public async Task GenerateAnnualDues_ReturnsOk()
        {
            _financialServiceMock.Setup(x => x.GenerateAnnualDuesAsync(2023, It.IsAny<CancellationToken>()))
                                 .Returns(Task.CompletedTask);

            var result = await _controller.GenerateAnnualDues(2023, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }
    }
}

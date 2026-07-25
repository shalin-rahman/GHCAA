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
            _controller = new FinancialsController(_financialServiceMock.Object, _context, _fileValidationServiceMock.Object);
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
    }
}

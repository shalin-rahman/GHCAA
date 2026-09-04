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
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Controllers
{
    [TestFixture]
    public class FinancialLedgerControllerTests
    {
        private Mock<IFinancialLedgerService> _ledgerServiceMock;
        private FinancialLedgerController _controller;

        [SetUp]
        public void Setup()
        {
            _ledgerServiceMock = new Mock<IFinancialLedgerService>();
            _controller = new FinancialLedgerController(_ledgerServiceMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Role, "Admin")
            }, "TestAuthentication"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Test]
        public async Task GetRecords_ReturnsOk()
        {
            var fakeResult = new { TotalItems = 0, Items = new List<FinancialRecord>() };
            _ledgerServiceMock.Setup(x => x.GetRecordsAsync(1, 10, null, null, null, false, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(fakeResult);

            var result = await _controller.GetRecords(1, 10, null, null, null, false, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetSummary_ReturnsOk()
        {
            var fakeSummary = new LedgerSummaryDto { Year = 2024 };
            _ledgerServiceMock.Setup(x => x.GetSummaryAsync(2024, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(fakeSummary);

            var result = await _controller.GetSummary(2024, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task AddRecord_ReturnsCreatedAtAction()
        {
            var record = new FinancialRecord { Description = "Test", Amount = 100, Year = 2024 };
            _ledgerServiceMock.Setup(x => x.AddRecordAsync(It.IsAny<FinancialRecord>(), It.IsAny<CancellationToken>()))
                              .ReturnsAsync(record);

            var result = await _controller.AddRecord(record, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
        }

        [Test]
        public async Task UpdateRecord_ReturnsOk()
        {
            var record = new FinancialRecord { Id = 1, Description = "Updated" };
            _ledgerServiceMock.Setup(x => x.UpdateRecordAsync(It.IsAny<FinancialRecord>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                              .ReturnsAsync(record);

            var result = await _controller.UpdateRecord(1, record, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task DeleteRecord_ReturnsOk_OnSuccess()
        {
            _ledgerServiceMock.Setup(x => x.DeleteRecordAsync(1, It.IsAny<int>(), It.IsAny<CancellationToken>()))
                              .ReturnsAsync(true);

            var result = await _controller.DeleteRecord(1, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkResult>());
        }

        [Test]
        public async Task ExportCsv_ReturnsFile()
        {
            var bytes = new byte[] { 1, 2, 3 };
            _ledgerServiceMock.Setup(x => x.ExportRecordsAsync(2024, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(bytes);

            var result = await _controller.ExportCsv(2024, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<FileContentResult>());
        }
    }
}

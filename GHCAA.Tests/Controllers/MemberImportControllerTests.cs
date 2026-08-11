using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Controllers;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Controllers
{
    [TestFixture]
    public class MemberImportControllerTests
    {
        private Mock<IMemberImportService> _importServiceMock;
        private MemberImportController _controller;

        [SetUp]
        public void Setup()
        {
            _importServiceMock = new Mock<IMemberImportService>();
            _controller = new MemberImportController(_importServiceMock.Object);
        }

        [Test]
        public async Task Import_ReturnsOk()
        {
            var request = new MemberImportRequestDto();
            var resultDto = new MemberImportResultDto { SuccessCount = 10, FailureCount = 0 };

            _importServiceMock.Setup(x => x.ImportMembersAsync(request, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(resultDto);

            var result = await _controller.Import(request, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult!.Value, Is.EqualTo(resultDto));
        }
    }
}

using System.IO;
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
        private Mock<IFileValidationService> _fileValidationServiceMock;
        private MemberImportController _controller;

        [SetUp]
        public void Setup()
        {
            _importServiceMock = new Mock<IMemberImportService>();
            _fileValidationServiceMock = new Mock<IFileValidationService>();
            _fileValidationServiceMock.Setup(x => x.Validate(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<long>(), It.IsAny<FileCategory>(), It.IsAny<long>()))
                                      .Returns(new FileValidationResult { IsValid = true });
            _controller = new MemberImportController(_importServiceMock.Object, _fileValidationServiceMock.Object);
        }

        private static IFormFile CreateFormFile(string fileName, string contentType)
        {
            var ms = new MemoryStream(new byte[] { 1, 2, 3 });
            return new FormFile(ms, 0, ms.Length, "ExcelFile", fileName) { Headers = new HeaderDictionary(), ContentType = contentType };
        }

        [Test]
        public async Task Import_ReturnsOk()
        {
            var request = new MemberImportRequestDto { ExcelFile = CreateFormFile("members.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") };
            var resultDto = new MemberImportResultDto { SuccessCount = 10, FailureCount = 0 };

            _importServiceMock.Setup(x => x.ImportMembersAsync(request, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(resultDto);

            var result = await _controller.Import(request, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult!.Value, Is.EqualTo(resultDto));
        }

        // 48.12: a renamed executable/script uploaded as "members.xlsx" must not reach
        // MemberImportService.OpenReadStream() - the same content-validation gate that
        // FinancialsController already applies to receipts.
        [Category("Security")]
        [Test]
        public async Task Import_WhenExcelFileFailsValidation_ReturnsBadRequestAndDoesNotImport()
        {
            _fileValidationServiceMock.Setup(x => x.Validate(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<long>(), It.IsAny<FileCategory>(), It.IsAny<long>()))
                                      .Returns(FileValidationResult.Fail("File content does not match its declared type."));

            var request = new MemberImportRequestDto { ExcelFile = CreateFormFile("members.xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet") };

            var result = await _controller.Import(request, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<ObjectResult>());
            var problemResult = result as ObjectResult;
            Assert.That(problemResult!.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
            _importServiceMock.Verify(x => x.ImportMembersAsync(It.IsAny<MemberImportRequestDto>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}

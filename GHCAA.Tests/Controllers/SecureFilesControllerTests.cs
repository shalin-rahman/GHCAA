using FluentAssertions;
using GHCAA.API.Controllers;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;

namespace GHCAA.Tests.Controllers
{
    // 80.13: first coverage for this controller — previously untested, and now rewritten to
    // depend on IFileUploadRepository instead of ApplicationDbContext directly.
    [TestFixture]
    public class SecureFilesControllerTests : ControllerTestBase
    {
        private Mock<IFileUploadRepository> _fileUploadsMock = null!;
        private Mock<ILogger<SecureFilesController>> _loggerMock = null!;
        private Mock<IFileStorageService> _storageMock = null!;
        private SecureFilesController _controller = null!;

        [SetUp]
        public void ControllerSetup()
        {
            _fileUploadsMock = new Mock<IFileUploadRepository>();
            _loggerMock = new Mock<ILogger<SecureFilesController>>();
            _storageMock = new Mock<IFileStorageService>();
            _controller = new SecureFilesController(_fileUploadsMock.Object, _loggerMock.Object, _storageMock.Object);
        }

        private void StoreHolds(string relativePath, string body) =>
            _storageMock.Setup(x => x.OpenReadAsync(relativePath, It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new StoredFile(new MemoryStream(Encoding.UTF8.GetBytes(body)), Modified, "\"v1\""));

        private static readonly DateTimeOffset Modified = new(2026, 10, 1, 8, 0, 0, TimeSpan.Zero);

        [Test]
        public async Task GetSecureFile_ReturnsNotFound_WhenNoDbRecord()
        {
            SetUserContext(_controller, memberId: 10, role: "Member");
            _fileUploadsMock.Setup(x => x.GetByFilePathAsync("no/such/path.pdf", It.IsAny<CancellationToken>())).ReturnsAsync((FileUpload?)null);

            var result = await _controller.GetSecureFile("no/such/path.pdf", CancellationToken.None);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task GetSecureFile_ReturnsForbid_WhenNonOwnerNonAdmin()
        {
            SetUserContext(_controller, memberId: 10, role: "Member");
            _fileUploadsMock.Setup(x => x.GetByFilePathAsync("secure/other.pdf", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FileUpload { Id = 1, MemberId = 999, FileName = "other.pdf", FilePath = "secure/other.pdf" });

            var result = await _controller.GetSecureFile("secure/other.pdf", CancellationToken.None);

            Assert.That(result, Is.InstanceOf<ForbidResult>());
            _storageMock.Verify(x => x.OpenReadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        [Category("NFR-P3")]
        public async Task GetSecureFile_ReturnsFile_WhenOwnerRequestsOwnFile()
        {
            SetUserContext(_controller, memberId: 10, role: "Member");
            var relativePath = "secure_uploads/10/mine.pdf";
            StoreHolds(relativePath, "pdf-bytes");
            _fileUploadsMock.Setup(x => x.GetByFilePathAsync(relativePath, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FileUpload { Id = 1, MemberId = 10, FileName = "mine.pdf", FilePath = relativePath });

            var result = await _controller.GetSecureFile(relativePath, CancellationToken.None);

            // The store's stream is handed straight to the response, not read into memory (98.9).
            var file = result.Should().BeOfType<FileStreamResult>().Subject;
            file.FileDownloadName.Should().Be("mine.pdf");
            file.ContentType.Should().Be("application/pdf");
            file.EnableRangeProcessing.Should().BeTrue();
        }

        // The validators let the browser revalidate a certificate with a 304 instead of a download.
        [Test]
        [Category("NFR-P4")]
        public async Task GetSecureFile_PassesOnTheStoresValidators()
        {
            SetUserContext(_controller, memberId: 10, role: "Member");
            var relativePath = "secure_uploads/10/mine.pdf";
            StoreHolds(relativePath, "pdf-bytes");
            _fileUploadsMock.Setup(x => x.GetByFilePathAsync(relativePath, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FileUpload { Id = 1, MemberId = 10, FileName = "mine.pdf", FilePath = relativePath });

            var result = await _controller.GetSecureFile(relativePath, CancellationToken.None);

            var file = result.Should().BeOfType<FileStreamResult>().Subject;
            file.EntityTag!.Tag.ToString().Should().Be("\"v1\"");
            file.LastModified.Should().Be(Modified);
        }

        [Test]
        [Category("NFR-R4")]
        public async Task GetSecureFile_ReturnsNotFound_WhenRecordExistsButStoreHasNoFile()
        {
            SetUserContext(_controller, memberId: 10, role: "Member");
            var relativePath = "secure_uploads/10/gone.pdf";
            _fileUploadsMock.Setup(x => x.GetByFilePathAsync(relativePath, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FileUpload { Id = 1, MemberId = 10, FileName = "gone.pdf", FilePath = relativePath });

            var result = await _controller.GetSecureFile(relativePath, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task GetSecureFile_ReturnsNotFound_WhenPathContainsTraversal()
        {
            SetUserContext(_controller, memberId: 10, role: "Member");
            _fileUploadsMock.Setup(x => x.GetByFilePathAsync("../../etc/passwd", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FileUpload { Id = 1, MemberId = 10, FileName = "passwd", FilePath = "../../etc/passwd" });

            var result = await _controller.GetSecureFile("../../etc/passwd", CancellationToken.None);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
            _storageMock.Verify(x => x.OpenReadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task GetSecureFile_AdminCanAccessAnyMembersFile()
        {
            SetUserContext(_controller, memberId: 1, role: "Admin");
            var relativePath = "secure_uploads/999/someone-elses.pdf";
            StoreHolds(relativePath, "pdf-bytes");
            _fileUploadsMock.Setup(x => x.GetByFilePathAsync(relativePath, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FileUpload { Id = 2, MemberId = 999, FileName = "someone-elses.pdf", FilePath = relativePath });

            var result = await _controller.GetSecureFile(relativePath, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<FileStreamResult>());
        }
    }
}

using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Controllers;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Controllers
{
    // 80.13: first coverage for this controller — previously untested, and now rewritten to
    // depend on IFileUploadRepository instead of ApplicationDbContext directly.
    [TestFixture]
    public class SecureFilesControllerTests : ControllerTestBase
    {
        private Mock<IFileUploadRepository> _fileUploadsMock = null!;
        private Mock<ILogger<SecureFilesController>> _loggerMock = null!;
        private IConfiguration _config = null!;
        private SecureFilesController _controller = null!;
        private string _basePath = null!;

        [SetUp]
        public void ControllerSetup()
        {
            _basePath = Path.Combine(Path.GetTempPath(), "ghcaa-secure-files-tests-" + System.Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_basePath);

            _fileUploadsMock = new Mock<IFileUploadRepository>();
            _loggerMock = new Mock<ILogger<SecureFilesController>>();
            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(new System.Collections.Generic.Dictionary<string, string?>
                {
                    ["FileStorage:BasePhysicalPath"] = _basePath
                })
                .Build();

            _controller = new SecureFilesController(_fileUploadsMock.Object, _loggerMock.Object, _config);
        }

        [TearDown]
        public void ControllerTearDown()
        {
            if (Directory.Exists(_basePath)) Directory.Delete(_basePath, recursive: true);
        }

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
        }

        [Test]
        public async Task GetSecureFile_ReturnsFile_WhenOwnerRequestsOwnFile()
        {
            SetUserContext(_controller, memberId: 10, role: "Member");
            var relativePath = "secure/mine.pdf";
            var fullPath = Path.Combine(_basePath, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllBytes(fullPath, Encoding.UTF8.GetBytes("pdf-bytes"));

            _fileUploadsMock.Setup(x => x.GetByFilePathAsync(relativePath, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FileUpload { Id = 1, MemberId = 10, FileName = "mine.pdf", FilePath = relativePath });

            var result = await _controller.GetSecureFile(relativePath, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<FileContentResult>());
        }

        [Test]
        public async Task GetSecureFile_ReturnsNotFound_WhenPathContainsTraversal()
        {
            SetUserContext(_controller, memberId: 10, role: "Member");
            _fileUploadsMock.Setup(x => x.GetByFilePathAsync("../../etc/passwd", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FileUpload { Id = 1, MemberId = 10, FileName = "passwd", FilePath = "../../etc/passwd" });

            var result = await _controller.GetSecureFile("../../etc/passwd", CancellationToken.None);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task GetSecureFile_AdminCanAccessAnyMembersFile()
        {
            SetUserContext(_controller, memberId: 1, role: "Admin");
            var relativePath = "secure/someone-elses.pdf";
            var fullPath = Path.Combine(_basePath, relativePath);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            File.WriteAllBytes(fullPath, Encoding.UTF8.GetBytes("pdf-bytes"));

            _fileUploadsMock.Setup(x => x.GetByFilePathAsync(relativePath, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new FileUpload { Id = 2, MemberId = 999, FileName = "someone-elses.pdf", FilePath = relativePath });

            var result = await _controller.GetSecureFile(relativePath, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<FileContentResult>());
        }
    }
}

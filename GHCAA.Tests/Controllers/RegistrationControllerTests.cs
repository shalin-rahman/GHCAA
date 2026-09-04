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
    public class RegistrationControllerTests
    {
        private Mock<IMemberService> _memberServiceMock;
        private Mock<IFileValidationService> _fileValidationServiceMock;
        private RegistrationController _controller;

        [SetUp]
        public void Setup()
        {
            _memberServiceMock = new Mock<IMemberService>();
            _fileValidationServiceMock = new Mock<IFileValidationService>();
            _fileValidationServiceMock.Setup(x => x.Validate(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<FileCategory>(), It.IsAny<long>()))
                                       .Returns(FileValidationResult.Ok());
            _controller = new RegistrationController(_memberServiceMock.Object, _fileValidationServiceMock.Object);
        }

        [Test]
        public async Task Register_ReturnsCreatedAtAction()
        {
            var dto = new MemberRegistrationDto { FullName = "Register Test" };
            _memberServiceMock.Setup(x => x.RegisterAsync(It.IsAny<MemberRegistrationDto>(), It.IsAny<UploadedFileDto>(), It.IsAny<UploadedFileDto>(), It.IsAny<UploadedFileDto>(), It.IsAny<CancellationToken>()))
                              .ReturnsAsync(100);

            var result = await _controller.Register(dto, null, null, null, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
        }

        [Test]
        public async Task GetStatus_ReturnsOk()
        {
            _memberServiceMock.Setup(x => x.GetStatusAsync(100, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                              .ReturnsAsync(new MemberRegistrationResultDto { MemberId = 100, Message = "Applied" });

            var result = await _controller.GetStatus(100, "test@test.com", CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [TestCase("123456", true, typeof(OkObjectResult))]
        [TestCase("wrong", false, typeof(BadRequestObjectResult))]
        public async Task VerifyEmail_ReturnsResultMatchingServiceOutcome(string otpCode, bool serviceResult, System.Type expectedResultType)
        {
            var dto = new VerifyEmailDto { Email = "test@test.com", OtpCode = otpCode };
            _memberServiceMock.Setup(x => x.VerifyEmailAsync("test@test.com", otpCode, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(serviceResult);

            var result = await _controller.VerifyEmail(dto, CancellationToken.None);

            Assert.That(result, Is.InstanceOf(expectedResultType));
        }
    }
}

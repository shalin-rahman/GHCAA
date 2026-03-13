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
        private RegistrationController _controller;

        [SetUp]
        public void Setup()
        {
            _memberServiceMock = new Mock<IMemberService>();
            _controller = new RegistrationController(_memberServiceMock.Object);
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
            _memberServiceMock.Setup(x => x.GetStatusAsync(100, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(new MemberRegistrationResultDto { MemberId = 100, Message = "Applied" });

            var result = await _controller.GetStatus(100, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task VerifyEmail_ReturnsOk_OnSuccess()
        {
            var dto = new VerifyEmailDto { Email = "test@test.com", OtpCode = "123456" };
            _memberServiceMock.Setup(x => x.VerifyEmailAsync("test@test.com", "123456", It.IsAny<CancellationToken>()))
                              .ReturnsAsync(true);

            var result = await _controller.VerifyEmail(dto, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task VerifyEmail_ReturnsBadRequest_OnFailure()
        {
            var dto = new VerifyEmailDto { Email = "test@test.com", OtpCode = "wrong" };
            _memberServiceMock.Setup(x => x.VerifyEmailAsync("test@test.com", "wrong", It.IsAny<CancellationToken>()))
                              .ReturnsAsync(false);

            var result = await _controller.VerifyEmail(dto, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }
    }
}

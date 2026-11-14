using GHCAA.API.Controllers;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using Microsoft.AspNetCore.Mvc;
using Moq;

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

        [Category("FR-01")]
        [Test]
        public async Task Register_ReturnsCreatedAtAction()
        {
            var dto = new MemberRegistrationDto { FullName = "Register Test" };
            _memberServiceMock.Setup(x => x.RegisterAsync(It.IsAny<MemberRegistrationDto>(), It.IsAny<UploadedFileDto>(), It.IsAny<UploadedFileDto>(), It.IsAny<UploadedFileDto>(), It.IsAny<CancellationToken>()))
                              .ReturnsAsync(100);

            var result = await _controller.Register(dto, null, null, null, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
        }

        // 84.102: a duplicate application answers 409 with the reason, not a bare 500.
        [Category("FR-01")]
        [Test]
        public async Task Register_Duplicate_Returns409WithTheReason()
        {
            var dto = new MemberRegistrationDto { FullName = "Duplicate Test" };
            _memberServiceMock.Setup(x => x.RegisterAsync(It.IsAny<MemberRegistrationDto>(), It.IsAny<UploadedFileDto>(), It.IsAny<UploadedFileDto>(), It.IsAny<UploadedFileDto>(), It.IsAny<CancellationToken>()))
                              .ThrowsAsync(new RequestConflictException("Member with same Email, NID, or Mobile already exists."));

            var result = await _controller.Register(dto, null, null, null, CancellationToken.None);

            var obj = result as ObjectResult;
            Assert.That(obj!.StatusCode, Is.EqualTo(409));
            Assert.That(((ProblemDetails)obj.Value!).Detail, Is.EqualTo("Member with same Email, NID, or Mobile already exists."));
        }

        [Test]
        public void Register_OtherInvalidOperation_IsRethrown()
        {
            _memberServiceMock.Setup(x => x.RegisterAsync(It.IsAny<MemberRegistrationDto>(), It.IsAny<UploadedFileDto>(), It.IsAny<UploadedFileDto>(), It.IsAny<UploadedFileDto>(), It.IsAny<CancellationToken>()))
                              .ThrowsAsync(new InvalidOperationException("Some other fault."));

            Assert.ThrowsAsync<InvalidOperationException>(() => _controller.Register(new MemberRegistrationDto(), null, null, null, CancellationToken.None));
        }

        [Category("FR-02")]
        [Test]
        public async Task GetStatus_ReturnsOk()
        {
            _memberServiceMock.Setup(x => x.GetStatusAsync(100, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                              .ReturnsAsync(new MemberRegistrationResultDto { MemberId = 100, Message = "Applied" });

            var result = await _controller.GetStatus(100, "test@test.com", CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [TestCase("123456", true, 200)]
        [TestCase("wrong", false, 400)]
        public async Task VerifyEmail_ReturnsResultMatchingServiceOutcome(string otpCode, bool serviceResult, int expectedStatusCode)
        {
            var dto = new VerifyEmailDto { Email = "test@test.com", OtpCode = otpCode };
            _memberServiceMock.Setup(x => x.VerifyEmailAsync("test@test.com", otpCode, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(serviceResult);

            var result = await _controller.VerifyEmail(dto, CancellationToken.None);

            // 82.4: OK still returns OkObjectResult; a failure now returns Problem()'s ObjectResult
            // rather than a BadRequestObjectResult, so status code is the shared shape to check.
            Assert.That(result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result!).StatusCode, Is.EqualTo(expectedStatusCode));
        }
    }
}

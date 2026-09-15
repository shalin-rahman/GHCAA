using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Controllers;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Controllers
{
    [TestFixture]
    public class ProfileControllerTests : ControllerTestBase
    {
        private Mock<IMemberService> _memberServiceMock;
        private Mock<IUserService> _userServiceMock;
        private Mock<IIDCardService> _idCardServiceMock;
        private Mock<IFileValidationService> _fileValidationServiceMock;
        private Mock<IAuthService> _authServiceMock;
        private Mock<ITokenService> _tokenServiceMock;
        private Mock<IWebHostEnvironment> _environmentMock;
        private ProfileController _controller;

        [SetUp]
        public void Setup()
        {
            _memberServiceMock = new Mock<IMemberService>();
            _userServiceMock = new Mock<IUserService>();
            _idCardServiceMock = new Mock<IIDCardService>();
            _fileValidationServiceMock = new Mock<IFileValidationService>();
            _authServiceMock = new Mock<IAuthService>();
            _tokenServiceMock = new Mock<ITokenService>();
            _environmentMock = new Mock<IWebHostEnvironment>();
            _environmentMock.Setup(x => x.EnvironmentName).Returns("Development");
            _fileValidationServiceMock.Setup(x => x.Validate(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<FileCategory>(), It.IsAny<long>()))
                                       .Returns(FileValidationResult.Ok());

            _controller = new ProfileController(
                _memberServiceMock.Object,
                _userServiceMock.Object,
                _authServiceMock.Object,
                _idCardServiceMock.Object,
                _fileValidationServiceMock.Object,
                _tokenServiceMock.Object,
                _environmentMock.Object);

            SetUserContext(_controller, 10, "Member", 1); // MemberId 10, UserId 1
            // Note: In old code NameIdentifier (UserId) was 1, and MemberId was 10.
            // If the tests strictly need UserId 1, we can adjust SetUserContext.
            // Looking at ChangePassword (line 75), it uses UserId from NameIdentifier.
        }

        [Test]
        public async Task GetProfile_ReturnsOk_WhenValidMember()
        {
            var profile = new MemberProfileDto { FullName = "Test Member" };
            _memberServiceMock.Setup(x => x.GetProfileAsync(10, true, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(profile);

            var result = await _controller.GetProfile(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task UpdateProfile_ReturnsOk_OnSuccess()
        {
            var dto = new UpdateProfileDto { FullName = "New Name" };
            _memberServiceMock.Setup(x => x.UpdateProfileAsync(10, dto, It.IsAny<CancellationToken>()))
                              .ReturnsAsync(true);

            var result = await _controller.UpdateProfile(dto, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task ChangePassword_ReturnsOk_OnSuccess()
        {
            var dto = new ChangePasswordDto { OldPassword = "old", NewPassword = "new" };
            _userServiceMock.Setup(x => x.ChangePasswordAsync(1, "old", "new", It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);
            _authServiceMock.Setup(x => x.GetUserWithRolesAsync(1, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new GHCAA.Domain.Models.User { Id = 1, Username = "member", SecurityStamp = "stamp" });
            _tokenServiceMock.Setup(x => x.CreateToken(It.IsAny<GHCAA.Domain.Models.User>()))
                             .Returns("access-token");
            _tokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("refresh-token");

            var result = await _controller.ChangePassword(dto, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Category("FR-06")]
        [Test]
        public async Task GetIDCard_ReturnsOk_OnSuccess()
        {
            _idCardServiceMock.Setup(x => x.GenerateIDCardDataUriAsync(10, It.IsAny<CancellationToken>()))
                              .ReturnsAsync("data:image/png;base64,...");

            var result = await _controller.GetIDCard(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetCertificate_ReturnsOk_OnSuccess()
        {
            _idCardServiceMock.Setup(x => x.GenerateCertificateDataUriAsync(10, It.IsAny<CancellationToken>()))
                              .ReturnsAsync("data:image/png;base64,...");

            var result = await _controller.GetCertificate(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }
    }
}

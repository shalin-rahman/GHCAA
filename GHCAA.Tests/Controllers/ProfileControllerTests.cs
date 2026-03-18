using System.Security.Claims;
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
    public class ProfileControllerTests
    {
        private Mock<IMemberService> _memberServiceMock;
        private Mock<IUserService> _userServiceMock;
        private Mock<IIDCardService> _idCardServiceMock;
        private ProfileController _controller;

        [SetUp]
        public void Setup()
        {
            _memberServiceMock = new Mock<IMemberService>();
            _userServiceMock = new Mock<IUserService>();
            _idCardServiceMock = new Mock<IIDCardService>();
            
            _controller = new ProfileController(_memberServiceMock.Object, _userServiceMock.Object, _idCardServiceMock.Object);
            
            SetUserContext(1, 10); // MemberId 10, UserId 1
        }

        private void SetUserContext(int userId, int memberId)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim("MemberId", memberId.ToString())
            }, "TestAuthentication"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
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

            var result = await _controller.ChangePassword(dto, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

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

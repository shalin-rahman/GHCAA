using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Controllers;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Controllers
{
    // 47.13.1: coverage for AuthController's non-Login actions (GoogleLogin, FacebookLogin,
    // Refresh, RefreshMobile, Logout, RequestStepUp, VerifyStepUp, ResetPassword). Login itself
    // is already covered by AuthControllerTests.cs.
    [TestFixture]
    public class AuthControllerMutationTests : ControllerTestBase
    {
        private Mock<IAuthService> _authServiceMock = null!;
        private Mock<ITokenService> _tokenServiceMock = null!;
        private Mock<IOtpService> _otpServiceMock = null!;
        private AuthController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _authServiceMock = new Mock<IAuthService>();
            _tokenServiceMock = new Mock<ITokenService>();
            _otpServiceMock = new Mock<IOtpService>();

            _tokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("dummy-refresh-token");
            _tokenServiceMock.Setup(x => x.StoreRefreshTokenAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var envMock = new Mock<IWebHostEnvironment>();
            envMock.SetupGet(x => x.EnvironmentName).Returns("Development");

            var configMock = new Mock<IConfiguration>();

            _controller = new AuthController(_authServiceMock.Object, _tokenServiceMock.Object, _context, envMock.Object, configMock.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        private void SetRequestCookie(string name, string value)
        {
            _controller.ControllerContext.HttpContext.Request.Headers["Cookie"] = $"{name}={value}";
        }

        private async Task<User> SeedUserAsync(bool isActive = true, bool isArchived = false, string? email = "member@example.com")
        {
            Member? member = null;
            if (email != null)
            {
                member = new Member
                {
                    FullName = "Test Member",
                    FatherName = "F",
                    MotherName = "M",
                    Email = email,
                    NID = "N1",
                    MobileNo = "M1",
                    PresentAddress = "A",
                    PermanentAddress = "A",
                    EmergencyContactName = "E",
                    EmergencyContactRelation = "R",
                    EmergencyContactPhone = "0"
                };
                _context.Members.Add(member);
                await _context.SaveChangesAsync();
            }

            var user = new User
            {
                Username = "user1",
                PasswordHash = "ph",
                SecurityStamp = "stamp",
                CreatedAt = System.DateTime.UtcNow,
                IsActive = isActive,
                IsArchived = isArchived,
                MemberId = member?.Id
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        // --- GoogleLogin ---

        [Test]
        public async Task GoogleLogin_ReturnsOk_OnSuccess()
        {
            var responseDto = new TokenResponseDto { Token = "token", MemberId = 1, Username = "user1", Role = "Member" };
            _authServiceMock.Setup(x => x.GoogleLoginAsync("good-token", It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseDto);

            var result = await _controller.GoogleLogin(new AuthController.SocialLoginRequest { Token = "good-token" }, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            Assert.That((result as OkObjectResult)!.Value, Is.EqualTo(responseDto));
        }

        [Test]
        public async Task GoogleLogin_ReturnsUnauthorized_OnFailure()
        {
            _authServiceMock.Setup(x => x.GoogleLoginAsync("bad-token", It.IsAny<CancellationToken>()))
                .ReturnsAsync((TokenResponseDto)null!);

            var result = await _controller.GoogleLogin(new AuthController.SocialLoginRequest { Token = "bad-token" }, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
        }

        // --- FacebookLogin ---

        [Test]
        public async Task FacebookLogin_ReturnsOk_OnSuccess()
        {
            var responseDto = new TokenResponseDto { Token = "token", MemberId = 1, Username = "user1", Role = "Member" };
            _authServiceMock.Setup(x => x.FacebookLoginAsync("good-token", It.IsAny<CancellationToken>()))
                .ReturnsAsync(responseDto);

            var result = await _controller.FacebookLogin(new AuthController.SocialLoginRequest { Token = "good-token" }, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            Assert.That((result as OkObjectResult)!.Value, Is.EqualTo(responseDto));
        }

        [Test]
        public async Task FacebookLogin_ReturnsUnauthorized_OnFailure()
        {
            _authServiceMock.Setup(x => x.FacebookLoginAsync("bad-token", It.IsAny<CancellationToken>()))
                .ReturnsAsync((TokenResponseDto)null!);

            var result = await _controller.FacebookLogin(new AuthController.SocialLoginRequest { Token = "bad-token" }, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
        }

        // --- Refresh ---

        [Test]
        public async Task Refresh_ReturnsOk_WhenRotationSucceedsAndUserIsActive()
        {
            var user = await SeedUserAsync();
            SetRequestCookie("refresh_token", "old-refresh-token");
            _tokenServiceMock.Setup(x => x.RotateRefreshTokenAsync("old-refresh-token", It.IsAny<CancellationToken>()))
                .ReturnsAsync(("new-refresh-token", user.Id));
            _tokenServiceMock.Setup(x => x.CreateToken(It.IsAny<User>())).Returns("new-access-token");

            var result = await _controller.Refresh(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task Refresh_ReturnsUnauthorized_WhenNoRefreshTokenCookie()
        {
            var result = await _controller.Refresh(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
        }

        [Test]
        public async Task Refresh_ReturnsUnauthorized_WhenRefreshTokenIsInvalidOrExpired()
        {
            SetRequestCookie("refresh_token", "stale-token");
            _tokenServiceMock.Setup(x => x.RotateRefreshTokenAsync("stale-token", It.IsAny<CancellationToken>()))
                .ReturnsAsync(((string NewToken, int UserId)?)null);

            var result = await _controller.Refresh(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
        }

        [Test]
        public async Task Refresh_ReturnsUnauthorized_WhenUserIsInactive()
        {
            var user = await SeedUserAsync(isActive: false);
            SetRequestCookie("refresh_token", "old-refresh-token");
            _tokenServiceMock.Setup(x => x.RotateRefreshTokenAsync("old-refresh-token", It.IsAny<CancellationToken>()))
                .ReturnsAsync(("new-refresh-token", user.Id));

            var result = await _controller.Refresh(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
        }

        // --- RefreshMobile ---

        [Test]
        public async Task RefreshMobile_ReturnsOk_WhenRotationSucceedsAndUserIsActive()
        {
            var user = await SeedUserAsync();
            _tokenServiceMock.Setup(x => x.RotateRefreshTokenAsync("old-refresh-token", It.IsAny<CancellationToken>()))
                .ReturnsAsync(("new-refresh-token", user.Id));
            _tokenServiceMock.Setup(x => x.CreateToken(It.IsAny<User>())).Returns("new-access-token");

            var result = await _controller.RefreshMobile(new RefreshRequestDto { RefreshToken = "old-refresh-token" }, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task RefreshMobile_ReturnsUnauthorized_WhenRefreshTokenIsInvalidOrExpired()
        {
            _tokenServiceMock.Setup(x => x.RotateRefreshTokenAsync("bad-token", It.IsAny<CancellationToken>()))
                .ReturnsAsync(((string NewToken, int UserId)?)null);

            var result = await _controller.RefreshMobile(new RefreshRequestDto { RefreshToken = "bad-token" }, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
        }

        [Test]
        public async Task RefreshMobile_ReturnsUnauthorized_WhenUserIsArchived()
        {
            var user = await SeedUserAsync(isArchived: true);
            _tokenServiceMock.Setup(x => x.RotateRefreshTokenAsync("old-refresh-token", It.IsAny<CancellationToken>()))
                .ReturnsAsync(("new-refresh-token", user.Id));

            var result = await _controller.RefreshMobile(new RefreshRequestDto { RefreshToken = "old-refresh-token" }, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
        }

        // --- Logout ---

        [Test]
        public async Task Logout_ReturnsOk_AndRevokesTokens_WhenUserIdClaimPresent()
        {
            SetUserContext(_controller, userId: 5);
            _tokenServiceMock.Setup(x => x.RevokeAllRefreshTokensAsync(5, It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var result = await _controller.Logout(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkResult>());
            _tokenServiceMock.Verify(x => x.RevokeAllRefreshTokensAsync(5, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Logout_ReturnsOk_ButSkipsRevocation_WhenNoUserIdClaim()
        {
            // No ControllerContext.User set up beyond the default anonymous principal from Setup().
            var result = await _controller.Logout(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkResult>());
            _tokenServiceMock.Verify(x => x.RevokeAllRefreshTokensAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // --- RequestStepUp ---

        [Test]
        public async Task RequestStepUp_ReturnsOk_WhenUserHasEmailOnFile()
        {
            var user = await SeedUserAsync(email: "admin@example.com");
            SetUserContext(_controller, userId: user.Id, role: "SuperAdmin");
            _otpServiceMock.Setup(x => x.GenerateAndSendOtpAsync("admin@example.com", GHCAA.Domain.Enums.OtpPurpose.AdminStepUp, It.IsAny<CancellationToken>()))
                .ReturnsAsync("otp-id");

            var result = await _controller.RequestStepUp(_otpServiceMock.Object, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            _otpServiceMock.Verify(x => x.GenerateAndSendOtpAsync("admin@example.com", GHCAA.Domain.Enums.OtpPurpose.AdminStepUp, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task RequestStepUp_ReturnsBadRequest_WhenNoEmailOnFile()
        {
            var user = await SeedUserAsync(email: null);
            SetUserContext(_controller, userId: user.Id, role: "SuperAdmin");

            var result = await _controller.RequestStepUp(_otpServiceMock.Object, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        // --- VerifyStepUp ---

        [Test]
        public async Task VerifyStepUp_ReturnsOk_WhenCodeIsValid()
        {
            var user = await SeedUserAsync(email: "admin@example.com");
            SetUserContext(_controller, userId: user.Id, role: "SuperAdmin");
            _otpServiceMock.Setup(x => x.VerifyOtpAsync("admin@example.com", "123456", GHCAA.Domain.Enums.OtpPurpose.AdminStepUp, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _tokenServiceMock.Setup(x => x.CreateStepUpToken(It.IsAny<User>())).Returns("step-up-token");

            var result = await _controller.VerifyStepUp(new StepUpVerifyDto { Code = "123456" }, _otpServiceMock.Object, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task VerifyStepUp_ReturnsBadRequest_WhenCodeIsInvalidOrExpired()
        {
            var user = await SeedUserAsync(email: "admin@example.com");
            SetUserContext(_controller, userId: user.Id, role: "SuperAdmin");
            _otpServiceMock.Setup(x => x.VerifyOtpAsync("admin@example.com", "000000", GHCAA.Domain.Enums.OtpPurpose.AdminStepUp, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _controller.VerifyStepUp(new StepUpVerifyDto { Code = "000000" }, _otpServiceMock.Object, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task VerifyStepUp_ReturnsBadRequest_WhenNoEmailOnFile()
        {
            var user = await SeedUserAsync(email: null);
            SetUserContext(_controller, userId: user.Id, role: "SuperAdmin");

            var result = await _controller.VerifyStepUp(new StepUpVerifyDto { Code = "123456" }, _otpServiceMock.Object, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        // --- ResetPassword ---

        [Test]
        public async Task ResetPassword_ReturnsOk_OnSuccess()
        {
            _authServiceMock.Setup(x => x.ResetPasswordAsync("user@example.com", "good-token", "NewPass123", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _controller.ResetPassword(new ResetPasswordDto { Email = "user@example.com", Token = "good-token", NewPassword = "NewPass123" }, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task ResetPassword_ReturnsBadRequest_WhenTokenIsInvalidOrExpired()
        {
            _authServiceMock.Setup(x => x.ResetPasswordAsync("user@example.com", "bad-token", "NewPass123", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _controller.ResetPassword(new ResetPasswordDto { Email = "user@example.com", Token = "bad-token", NewPassword = "NewPass123" }, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }
    }
}

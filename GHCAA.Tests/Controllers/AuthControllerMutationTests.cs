using GHCAA.API.Controllers;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;

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
        private Mock<IAuthenticatorService> _authenticatorMock = null!;
        private Mock<ISocialAuthConfigService> _socialAuthConfigServiceMock = null!;
        private Mock<IUserService> _userServiceMock = null!;
        private Mock<IActivityService> _activityServiceMock = null!;
        private AuthController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _authServiceMock = new Mock<IAuthService>();
            _tokenServiceMock = new Mock<ITokenService>();
            _otpServiceMock = new Mock<IOtpService>();
            _authenticatorMock = new Mock<IAuthenticatorService>();
            _userServiceMock = new Mock<IUserService>();
            _activityServiceMock = new Mock<IActivityService>();
            _authenticatorMock.Setup(x => x.GetStatusAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AuthenticatorStatusDto(false, false, 0));

            _tokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("dummy-refresh-token");
            _tokenServiceMock.Setup(x => x.StoreRefreshTokenAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            var envMock = new Mock<IWebHostEnvironment>();
            envMock.SetupGet(x => x.EnvironmentName).Returns("Development");

            var configMock = new Mock<IConfiguration>();

            _socialAuthConfigServiceMock = new Mock<ISocialAuthConfigService>();
            _controller = new AuthController(_authServiceMock.Object, _tokenServiceMock.Object, _socialAuthConfigServiceMock.Object, envMock.Object, configMock.Object);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        private void SetRequestCookie(string name, string value)
        {
            _controller.ControllerContext.HttpContext.Request.Headers["Cookie"] = $"{name}={value}";
        }

        private async Task<User> SeedUserAsync(bool isActive = true, bool isArchived = false, string? email = "member@example.com", string username = "user1")
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
                Username = username,
                PasswordHash = "ph",
                SecurityStamp = "stamp",
                CreatedAt = System.DateTime.UtcNow,
                IsActive = isActive,
                IsArchived = isArchived,
                MemberId = member?.Id
            };
            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // AuthController now resolves these through IAuthService rather than the DbContext
            // directly; wire the mock to return the same seeded user (with Member attached
            // in-memory, since a fresh SaveChangesAsync doesn't populate the navigation).
            user.Member = member;
            _authServiceMock.Setup(x => x.GetUserWithRolesAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);
            _authServiceMock.Setup(x => x.GetUserWithRolesAndMemberAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

            return user;
        }

        // --- GoogleLogin ---

        [Category("FR-10")]
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

        [Category("FR-10")]
        [Test]
        public async Task GoogleLogin_ReturnsUnauthorized_OnFailure()
        {
            _authServiceMock.Setup(x => x.GoogleLoginAsync("bad-token", It.IsAny<CancellationToken>()))
                .ReturnsAsync((TokenResponseDto)null!);

            var result = await _controller.GoogleLogin(new AuthController.SocialLoginRequest { Token = "bad-token" }, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result!).StatusCode, Is.EqualTo(401));
        }

        // --- FacebookLogin ---

        [Category("FR-10")]
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

        [Category("FR-10")]
        [Test]
        public async Task FacebookLogin_ReturnsUnauthorized_OnFailure()
        {
            _authServiceMock.Setup(x => x.FacebookLoginAsync("bad-token", It.IsAny<CancellationToken>()))
                .ReturnsAsync((TokenResponseDto)null!);

            var result = await _controller.FacebookLogin(new AuthController.SocialLoginRequest { Token = "bad-token" }, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result!).StatusCode, Is.EqualTo(401));
        }

        // --- Refresh ---

        [Category("FR-11")]
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

        [Category("FR-11")]
        [Test]
        public async Task Refresh_ReturnsUnauthorized_WhenNoRefreshTokenCookie()
        {
            var result = await _controller.Refresh(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result!).StatusCode, Is.EqualTo(401));
        }

        [Category("FR-11")]
        [Test]
        public async Task Refresh_ReturnsUnauthorized_WhenRefreshTokenIsInvalidOrExpired()
        {
            SetRequestCookie("refresh_token", "stale-token");
            _tokenServiceMock.Setup(x => x.RotateRefreshTokenAsync("stale-token", It.IsAny<CancellationToken>()))
                .ReturnsAsync(((string NewToken, int UserId)?)null);

            var result = await _controller.Refresh(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result!).StatusCode, Is.EqualTo(401));
        }

        [Category("FR-11")]
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

        [Category("FR-11")]
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

        [Category("FR-11")]
        [Test]
        public async Task RefreshMobile_ReturnsUnauthorized_WhenRefreshTokenIsInvalidOrExpired()
        {
            _tokenServiceMock.Setup(x => x.RotateRefreshTokenAsync("bad-token", It.IsAny<CancellationToken>()))
                .ReturnsAsync(((string NewToken, int UserId)?)null);

            var result = await _controller.RefreshMobile(new RefreshRequestDto { RefreshToken = "bad-token" }, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result!).StatusCode, Is.EqualTo(401));
        }

        [Category("FR-11")]
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

        [Category("FR-11")]
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

        [Category("FR-11")]
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
        public async Task RequestStepUp_Returns503_WhenTheCodeCannotBeEmailed()
        {
            var user = await SeedUserAsync(email: "admin@example.com");
            SetUserContext(_controller, userId: user.Id, role: "SuperAdmin");
            _otpServiceMock.Setup(x => x.GenerateAndSendOtpAsync("admin@example.com", GHCAA.Domain.Enums.OtpPurpose.AdminStepUp, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OtpDeliveryException("admin@example.com"));

            var result = await _controller.RequestStepUp(_otpServiceMock.Object, CancellationToken.None);

            var problem = result as ObjectResult;
            Assert.That(problem?.StatusCode, Is.EqualTo(StatusCodes.Status503ServiceUnavailable));
            Assert.That(((ProblemDetails)problem!.Value!).Detail, Does.Contain("adm***@example.com"));
        }

        [Test]
        public async Task RequestStepUp_ReturnsBadRequest_WhenNoEmailOnFile()
        {
            var user = await SeedUserAsync(email: null);
            SetUserContext(_controller, userId: user.Id, role: "SuperAdmin");

            var result = await _controller.RequestStepUp(_otpServiceMock.Object, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result!).StatusCode, Is.EqualTo(400));
        }

        // 37.1w: voting needs step-up, so a plain member must be able to get a code.
        [Test]
        [Category("FR-39")]
        public async Task RequestStepUp_SendsCodeToMemberEmail_ForPlainMember()
        {
            var user = await SeedUserAsync(email: "voter@example.com");
            SetUserContext(_controller, userId: user.Id, role: "Member");
            _otpServiceMock.Setup(x => x.GenerateAndSendOtpAsync("voter@example.com", GHCAA.Domain.Enums.OtpPurpose.AdminStepUp, It.IsAny<CancellationToken>()))
                .ReturnsAsync("otp-id");

            var result = await _controller.RequestStepUp(_otpServiceMock.Object, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        [Category("FR-39")]
        public async Task RequestStepUp_SendsCodeToUsername_WhenUserHasNoMemberAndEmailUsername()
        {
            var user = await SeedUserAsync(email: null, username: "official@example.com");
            SetUserContext(_controller, userId: user.Id, role: "Member");
            _otpServiceMock.Setup(x => x.GenerateAndSendOtpAsync("official@example.com", GHCAA.Domain.Enums.OtpPurpose.AdminStepUp, It.IsAny<CancellationToken>()))
                .ReturnsAsync("otp-id");

            var result = await _controller.RequestStepUp(_otpServiceMock.Object, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            _otpServiceMock.Verify(x => x.GenerateAndSendOtpAsync("official@example.com", GHCAA.Domain.Enums.OtpPurpose.AdminStepUp, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        [Category("FR-39")]
        public async Task VerifyStepUp_ReturnsOk_ForPlainMember()
        {
            var user = await SeedUserAsync(email: "voter@example.com");
            SetUserContext(_controller, userId: user.Id, role: "Member");
            _otpServiceMock.Setup(x => x.VerifyOtpAsync("voter@example.com", "123456", GHCAA.Domain.Enums.OtpPurpose.AdminStepUp, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            _tokenServiceMock.Setup(x => x.CreateStepUpToken(It.IsAny<User>())).Returns("step-up-token");

            var result = await _controller.VerifyStepUp(new StepUpVerifyDto { Code = "123456" }, _otpServiceMock.Object, _authenticatorMock.Object, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
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

            var result = await _controller.VerifyStepUp(new StepUpVerifyDto { Code = "123456" }, _otpServiceMock.Object, _authenticatorMock.Object, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task VerifyStepUp_ReturnsBadRequest_WhenCodeIsInvalidOrExpired()
        {
            var user = await SeedUserAsync(email: "admin@example.com");
            SetUserContext(_controller, userId: user.Id, role: "SuperAdmin");
            _otpServiceMock.Setup(x => x.VerifyOtpAsync("admin@example.com", "000000", GHCAA.Domain.Enums.OtpPurpose.AdminStepUp, It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _controller.VerifyStepUp(new StepUpVerifyDto { Code = "000000" }, _otpServiceMock.Object, _authenticatorMock.Object, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result!).StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task VerifyStepUp_ReturnsBadRequest_WhenNoEmailOnFile()
        {
            var user = await SeedUserAsync(email: null);
            SetUserContext(_controller, userId: user.Id, role: "SuperAdmin");

            var result = await _controller.VerifyStepUp(new StepUpVerifyDto { Code = "123456" }, _otpServiceMock.Object, _authenticatorMock.Object, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result!).StatusCode, Is.EqualTo(400));
        }

        // 95.6: the app path needs no email and never touches the email OTP store.
        [Test]
        [Category("FR-39")]
        public async Task VerifyStepUp_AppCode_ReturnsOk_WithNoEmailOnFile()
        {
            var user = await SeedUserAsync(email: null);
            SetUserContext(_controller, userId: user.Id, role: "SuperAdmin");
            _authenticatorMock.Setup(x => x.VerifyAsync(user.Id, "123456", It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _tokenServiceMock.Setup(x => x.CreateStepUpToken(It.IsAny<User>())).Returns("step-up-token");

            var result = await _controller.VerifyStepUp(new StepUpVerifyDto { Code = "123456", Method = "app" }, _otpServiceMock.Object, _authenticatorMock.Object, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            _otpServiceMock.Verify(x => x.VerifyOtpAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<GHCAA.Domain.Enums.OtpPurpose>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        [Category("FR-39")]
        public async Task VerifyStepUp_AppCode_ReturnsBadRequest_WhenRejected()
        {
            var user = await SeedUserAsync(email: "admin@example.com");
            SetUserContext(_controller, userId: user.Id, role: "SuperAdmin");
            _authenticatorMock.Setup(x => x.VerifyAsync(user.Id, "000000", It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = await _controller.VerifyStepUp(new StepUpVerifyDto { Code = "000000", Method = "app" }, _otpServiceMock.Object, _authenticatorMock.Object, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result!).StatusCode, Is.EqualTo(400));
        }

        [Test]
        [Category("FR-39")]
        // 95.6: the owner chose app or email, so an enrolled user can still get an emailed code.
        public async Task RequestStepUp_StillSendsEmail_WhenAppEnrolled()
        {
            var user = await SeedUserAsync(email: "admin@example.com");
            SetUserContext(_controller, userId: user.Id, role: "SuperAdmin");
            _authenticatorMock.Setup(x => x.GetStatusAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(new AuthenticatorStatusDto(true, false, 10));
            _otpServiceMock.Setup(x => x.GenerateAndSendOtpAsync("admin@example.com", GHCAA.Domain.Enums.OtpPurpose.AdminStepUp, It.IsAny<CancellationToken>()))
                .ReturnsAsync("otp-id");

            var result = await _controller.RequestStepUp(_otpServiceMock.Object, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            _otpServiceMock.Verify(x => x.GenerateAndSendOtpAsync("admin@example.com", GHCAA.Domain.Enums.OtpPurpose.AdminStepUp, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        [Category("FR-39")]
        public async Task VerifyStepUp_AcceptsEmailCode_WhenAppEnrolled()
        {
            var user = await SeedUserAsync(email: "admin@example.com");
            SetUserContext(_controller, userId: user.Id, role: "SuperAdmin");
            _authenticatorMock.Setup(x => x.GetStatusAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(new AuthenticatorStatusDto(true, false, 10));
            _otpServiceMock.Setup(x => x.VerifyOtpAsync("admin@example.com", "123456", GHCAA.Domain.Enums.OtpPurpose.AdminStepUp, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _tokenServiceMock.Setup(x => x.CreateStepUpToken(It.IsAny<User>())).Returns("step-up-token");

            var result = await _controller.VerifyStepUp(new StepUpVerifyDto { Code = "123456" }, _otpServiceMock.Object, _authenticatorMock.Object, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            _authenticatorMock.Verify(x => x.VerifyAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        [Category("FR-39")]
        public async Task GetAuthenticator_FlagsPasswordSetup_OnlyWhenNoEmailOnFile()
        {
            var noEmail = await SeedUserAsync(email: null);
            _authenticatorMock.Setup(x => x.GetStatusAsync(It.IsAny<int>(), It.IsAny<CancellationToken>())).ReturnsAsync(new AuthenticatorStatusDto(false, false, 0));
            SetUserContext(_controller, userId: noEmail.Id, role: "Member");

            var result = await _controller.GetAuthenticator(_authenticatorMock.Object, CancellationToken.None);

            Assert.That(((AuthenticatorStatusDto)((OkObjectResult)result).Value!).SetupNeedsPassword, Is.True);
        }

        [Test]
        [Category("FR-39")]
        public async Task SetupWithPassword_ReturnsSetup_WhenNoEmailAndPasswordMatches()
        {
            var user = await SeedUserAsync(email: null);
            SetUserContext(_controller, userId: user.Id, role: "Member");
            _authenticatorMock.Setup(x => x.PasswordMatchesAsync(user.Id, "right", It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _authenticatorMock.Setup(x => x.BeginSetupAsync(user.Id, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new AuthenticatorSetupDto("SECRET", "otpauth://totp/x", "data:image/png;base64,"));

            var result = await _controller.SetupAuthenticatorWithPassword(new AuthenticatorPasswordDto { Password = "right" }, _authenticatorMock.Object, _userServiceMock.Object, _activityServiceMock.Object, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            _userServiceMock.Verify(x => x.ResetFailedPasswordsAsync(user.Id, It.IsAny<CancellationToken>()), Times.Once);
            _activityServiceMock.Verify(x => x.LogActivityAsync(user.MemberId, Constants.Authenticator.PasswordSetupAuditType,
                It.IsAny<string>(), user.Id, It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<CancellationToken>(), It.IsAny<DateTime?>()), Times.Once);
        }

        [Test]
        [Category("FR-39")]
        public async Task SetupWithPassword_Refuses_WrongPassword()
        {
            var user = await SeedUserAsync(email: null);
            SetUserContext(_controller, userId: user.Id, role: "Member");
            _authenticatorMock.Setup(x => x.PasswordMatchesAsync(user.Id, It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = await _controller.SetupAuthenticatorWithPassword(new AuthenticatorPasswordDto { Password = "wrong" }, _authenticatorMock.Object, _userServiceMock.Object, _activityServiceMock.Object, CancellationToken.None);

            Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(400));
            _authenticatorMock.Verify(x => x.BeginSetupAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _userServiceMock.Verify(x => x.RecordFailedPasswordAsync(user.Id, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        [Category("FR-39")]
        public async Task SetupWithPassword_RefusesLockedUserWithoutCheckingPassword()
        {
            var user = await SeedUserAsync(email: null);
            user.LockoutUntil = DateTime.UtcNow.AddMinutes(15);
            SetUserContext(_controller, userId: user.Id, role: "Member");

            var result = await _controller.SetupAuthenticatorWithPassword(new AuthenticatorPasswordDto { Password = "right" },
                _authenticatorMock.Object, _userServiceMock.Object, _activityServiceMock.Object, CancellationToken.None);

            var response = (ObjectResult)result;
            Assert.That(response.StatusCode, Is.EqualTo(StatusCodes.Status423Locked));
            Assert.That(((ProblemDetails)response.Value!).Extensions["code"], Is.EqualTo(Constants.ErrorCodes.AccountLocked));
            _authenticatorMock.Verify(x => x.PasswordMatchesAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
            _userServiceMock.Verify(x => x.RecordFailedPasswordAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // A user with an email must use step-up, so a stolen session plus a guessed password is not enough.
        [Test]
        [Category("FR-39")]
        public async Task SetupWithPassword_Refuses_WhenEmailOnFile()
        {
            var user = await SeedUserAsync(email: "admin@example.com");
            SetUserContext(_controller, userId: user.Id, role: "SuperAdmin");
            _authenticatorMock.Setup(x => x.PasswordMatchesAsync(user.Id, It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await _controller.SetupAuthenticatorWithPassword(new AuthenticatorPasswordDto { Password = "right" }, _authenticatorMock.Object, _userServiceMock.Object, _activityServiceMock.Object, CancellationToken.None);

            Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(400));
            _authenticatorMock.Verify(x => x.BeginSetupAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        // --- ResetPassword ---

        [Category("FR-09")]
        [Test]
        public async Task ResetPassword_ReturnsOk_OnSuccess()
        {
            _authServiceMock.Setup(x => x.ResetPasswordAsync("user@example.com", "good-token", "NewPass123", It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _controller.ResetPassword(new ResetPasswordDto { Email = "user@example.com", Token = "good-token", NewPassword = "NewPass123" }, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Category("FR-09")]
        [Test]
        public async Task ResetPassword_ReturnsBadRequest_WhenTokenIsInvalidOrExpired()
        {
            _authServiceMock.Setup(x => x.ResetPasswordAsync("user@example.com", "bad-token", "NewPass123", It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _controller.ResetPassword(new ResetPasswordDto { Email = "user@example.com", Token = "bad-token", NewPassword = "NewPass123" }, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result!).StatusCode, Is.EqualTo(400));
        }
    }
}

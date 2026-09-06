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
    [TestFixture]
    public class AuthControllerTests : ControllerTestBase
    {
        private Mock<IAuthService> _authServiceMock = null!;
        private Mock<ITokenService> _tokenServiceMock = null!;
        private Mock<ISocialAuthConfigService> _socialAuthConfigServiceMock = null!;
        private AuthController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _authServiceMock = new Mock<IAuthService>();
            _tokenServiceMock = new Mock<ITokenService>();

            // Return a dummy refresh token so SetAuthCookiesAsync doesn't throw.
            _tokenServiceMock.Setup(x => x.GenerateRefreshToken()).Returns("dummy-refresh-token");
            _tokenServiceMock.Setup(x => x.StoreRefreshTokenAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);

            // AuthController uses IWebHostEnvironment only for `Secure = !_env.IsDevelopment()` on the
            // auth cookies; a Development env keeps Secure=false so cookie append works over the test's http context.
            var envMock = new Mock<IWebHostEnvironment>();
            envMock.SetupGet(x => x.EnvironmentName).Returns("Development");

            var configMock = new Mock<IConfiguration>();

            _socialAuthConfigServiceMock = new Mock<ISocialAuthConfigService>();
            _controller = new AuthController(_authServiceMock.Object, _tokenServiceMock.Object, _socialAuthConfigServiceMock.Object, envMock.Object, configMock.Object);

            // Provide a real DefaultHttpContext so Response.Cookies.Append works.
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        [Category("FR-08")]
        [Test]
        public async Task Login_ReturnsOk_OnSuccess()
        {
            // Stub the username fallback lookup that SetAuthCookiesAsync uses when it can't
            // resolve the user id from the JWT claim.
            var user = new User { Id = 1, Username = "user", PasswordHash = "ph", SecurityStamp = "stamp", CreatedAt = DateTime.UtcNow };
            _authServiceMock.Setup(x => x.GetUserByUsernameAsync("user", It.IsAny<CancellationToken>())).ReturnsAsync(user);

            var loginDto = new LoginDto { Username = "user", Password = "password" };
            var responseDto = new TokenResponseDto { Token = "token", MemberId = 1, Username = "user", Role = "Member" };

            _authServiceMock.Setup(x => x.LoginAsync(loginDto, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(responseDto);

            var result = await _controller.Login(loginDto, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult!.Value, Is.EqualTo(responseDto));
        }

        [Category("FR-08")]
        [Test]
        public async Task Login_ReturnsUnauthorized_OnFailure()
        {
            var loginDto = new LoginDto { Username = "user", Password = "wrongpassword" };

            _authServiceMock.Setup(x => x.LoginAsync(loginDto, It.IsAny<CancellationToken>()))
                            .ReturnsAsync((TokenResponseDto)null!);

            var result = await _controller.Login(loginDto, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result!).StatusCode, Is.EqualTo(401));
        }
    }
}

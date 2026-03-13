using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Controllers;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Controllers
{
    [TestFixture]
    public class AuthControllerTests
    {
        private Mock<IAuthService> _authServiceMock;
        private AuthController _controller;

        [SetUp]
        public void Setup()
        {
            _authServiceMock = new Mock<IAuthService>();
            _controller = new AuthController(_authServiceMock.Object);
        }

        [Test]
        public async Task Login_ReturnsOk_OnSuccess()
        {
            var loginDto = new LoginDto { Username = "user", Password = "password" };
            var responseDto = new TokenResponseDto { Token = "token", MemberId = 1, Username = "user", Role = "Member" };

            _authServiceMock.Setup(x => x.LoginAsync(loginDto, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(responseDto);

            var result = await _controller.Login(loginDto, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            var okResult = result as OkObjectResult;
            Assert.That(okResult.Value, Is.EqualTo(responseDto));
        }

        [Test]
        public async Task Login_ReturnsUnauthorized_OnFailure()
        {
            var loginDto = new LoginDto { Username = "user", Password = "wrongpassword" };

            _authServiceMock.Setup(x => x.LoginAsync(loginDto, It.IsAny<CancellationToken>()))
                            .ReturnsAsync((TokenResponseDto)null);

            var result = await _controller.Login(loginDto, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<UnauthorizedObjectResult>());
        }
    }
}

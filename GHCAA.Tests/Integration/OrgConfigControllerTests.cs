using System.Security.Claims;
using FluentAssertions;
using GHCAA.API.Controllers;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Integration
{
    [TestFixture]
    public class OrgConfigControllerTests
    {
        [Test]
        public async Task GetConfig_Returns_OkObjectResult_WithConfig()
        {
            // Arrange
            var mockService = new Mock<IOrgConfigService>();
            var expectedConfig = new OrgConfigDto { OrgId = "test_org" };
            mockService.Setup(s => s.GetConfigAsync()).ReturnsAsync(expectedConfig);

            var controller = new OrgConfigController(mockService.Object);

            // Act
            var result = await controller.GetConfig();

            // Assert
            var okResult = result as OkObjectResult;
            okResult.Should().NotBeNull();
            okResult!.StatusCode.Should().Be(200);
            okResult.Value.Should().BeEquivalentTo(expectedConfig);
        }

        [Test]
        public async Task UpdateConfig_NullDto_ReturnsBadRequest()
        {
            // Arrange
            var mockService = new Mock<IOrgConfigService>();
            var controller = new OrgConfigController(mockService.Object);

            // Act
            var result = await controller.UpdateConfig(null!);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            badRequestResult.Should().NotBeNull();
            badRequestResult!.StatusCode.Should().Be(400);
        }

        [Test]
        public async Task UpdateConfig_ValidDto_CallsService_AndReturnsNoContent()
        {
            // Arrange
            var mockService = new Mock<IOrgConfigService>();
            var expectedConfig = new OrgConfigDto { OrgId = "test_org" };

            var claims = new[] { new Claim("MemberId", "admin_123") };
            var identity = new ClaimsIdentity(claims, "TestAuth");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            var controller = new OrgConfigController(mockService.Object)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = claimsPrincipal }
                }
            };

            // Act
            var result = await controller.UpdateConfig(expectedConfig);

            // Assert
            var noContentResult = result as NoContentResult;
            noContentResult.Should().NotBeNull();
            noContentResult!.StatusCode.Should().Be(204);

            mockService.Verify(s => s.UpdateConfigAsync(expectedConfig, "admin_123"), Times.Once);
        }
    }
}

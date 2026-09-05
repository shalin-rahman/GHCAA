using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Controllers;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Controllers
{
    [TestFixture]
    public class RolesControllerTests
    {
        private Mock<IRoleService> _roleServiceMock;
        private Mock<IUserService> _userServiceMock;
        private RolesController _controller;

        [SetUp]
        public void Setup()
        {
            _roleServiceMock = new Mock<IRoleService>();
            _userServiceMock = new Mock<IUserService>();
            _controller = new RolesController(_roleServiceMock.Object, _userServiceMock.Object, Mock.Of<ILogger<RolesController>>());
        }

        [Test]
        public async Task CreateAdmin_ValidDto_ReturnsOk()
        {
            var dto = new RolesController.CreateAdminDto { Username = "newadmin", Password = "Passw0rd!", Role = "Admin" };
            _userServiceMock.Setup(x => x.CreateSystemAdminAsync(dto.Username, dto.Password, dto.Role, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User { Id = 5, Username = dto.Username });

            var result = await _controller.CreateAdmin(dto, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task CreateAdmin_ServiceThrows_ReturnsBadRequest()
        {
            var dto = new RolesController.CreateAdminDto { Username = "dup", Password = "Passw0rd!", Role = "Admin" };
            _userServiceMock.Setup(x => x.CreateSystemAdminAsync(dto.Username, dto.Password, dto.Role, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new System.InvalidOperationException("Username 'dup' is already taken"));

            var result = await _controller.CreateAdmin(dto, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task CreateRole_ReturnsOk()
        {
            _roleServiceMock.Setup(x => x.CreateRoleAsync("Moderator", It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Role { Id = 3, Name = "Moderator" });

            var result = await _controller.CreateRole("Moderator", CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task AssignRole_Success_ReturnsOk()
        {
            _roleServiceMock.Setup(x => x.AssignRoleToUserAsync(7, "Admin", It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await _controller.AssignRole(7, "Admin", CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task AssignRole_UserOrRoleNotFound_ReturnsBadRequest()
        {
            _roleServiceMock.Setup(x => x.AssignRoleToUserAsync(999, "Admin", It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = await _controller.AssignRole(999, "Admin", CancellationToken.None);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task RemoveRole_Success_ReturnsOk()
        {
            _roleServiceMock.Setup(x => x.RemoveRoleFromUserAsync(7, "Admin", It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await _controller.RemoveRole(7, "Admin", CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task RemoveRole_UserNotFound_ReturnsBadRequest()
        {
            _roleServiceMock.Setup(x => x.RemoveRoleFromUserAsync(999, "Admin", It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = await _controller.RemoveRole(999, "Admin", CancellationToken.None);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task DeleteUser_Success_ReturnsOk()
        {
            _userServiceMock.Setup(x => x.DeleteSystemAdminAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await _controller.DeleteUser(5, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task DeleteUser_ProtectedOrMissing_ReturnsBadRequest()
        {
            _userServiceMock.Setup(x => x.DeleteSystemAdminAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = await _controller.DeleteUser(1, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task DisableUser_Success_ReturnsOk()
        {
            _userServiceMock.Setup(x => x.SetUserActiveAsync(5, false, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await _controller.DisableUser(5, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task DisableUser_ProtectedOrMissing_ReturnsBadRequest()
        {
            _userServiceMock.Setup(x => x.SetUserActiveAsync(1, false, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = await _controller.DisableUser(1, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task EnableUser_Success_ReturnsOk()
        {
            _userServiceMock.Setup(x => x.SetUserActiveAsync(5, true, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await _controller.EnableUser(5, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task EnableUser_UserNotFound_ReturnsBadRequest()
        {
            _userServiceMock.Setup(x => x.SetUserActiveAsync(999, true, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = await _controller.EnableUser(999, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task ResetPasswordAdmin_Success_ReturnsOk()
        {
            _userServiceMock.Setup(x => x.SendAdminPasswordResetLinkAsync(5, It.IsAny<CancellationToken>()))
                .ReturnsAsync((true, "http://localhost:4200/reset-password?email=admin&token=abc"));

            var result = await _controller.ResetPasswordAdmin(5, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task ResetPasswordAdmin_UserNotFound_ReturnsBadRequest()
        {
            _userServiceMock.Setup(x => x.SendAdminPasswordResetLinkAsync(999, It.IsAny<CancellationToken>()))
                .ReturnsAsync((false, (string?)null));

            var result = await _controller.ResetPasswordAdmin(999, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task GetUsers_ReturnsOk()
        {
            _userServiceMock.Setup(x => x.GetAllUsersAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<User>());

            var result = await _controller.GetUsers(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetRoles_ReturnsOk()
        {
            _roleServiceMock.Setup(x => x.GetAllRolesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(new List<Role>());

            var result = await _controller.GetRoles(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }
    }
}

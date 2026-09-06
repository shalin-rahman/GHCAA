using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Controllers;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Controllers
{
    // Coverage for the two [RequireStepUp]-protected delete actions flagged by this session's
    // mutation-coverage audit as having zero test coverage despite being the highest-risk class
    // of endpoint (destructive + step-up gated). The filter itself is covered by
    // RequireStepUpAttributeTests; this covers the underlying controller/service contract.
    [TestFixture]
    public class DestructiveStepUpActionsTests : ControllerTestBase
    {
        [Test]
        public async Task RolesController_DeleteUser_ReturnsOk_WhenServiceSucceeds()
        {
            var userService = new Mock<IUserService>();
            userService.Setup(s => s.DeleteSystemAdminAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            var controller = new RolesController(Mock.Of<IRoleService>(), userService.Object, Mock.Of<ILogger<RolesController>>());

            var result = await controller.DeleteUser(5, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task RolesController_DeleteUser_ReturnsBadRequest_WhenTargetIsAMemberAccount()
        {
            var userService = new Mock<IUserService>();
            userService.Setup(s => s.DeleteSystemAdminAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            var controller = new RolesController(Mock.Of<IRoleService>(), userService.Object, Mock.Of<ILogger<RolesController>>());

            var result = await controller.DeleteUser(5, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result!).StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task AdminGovernanceController_DeleteECMember_ReturnsOk_WhenServiceSucceeds()
        {
            var governanceService = new Mock<IGovernanceService>();
            governanceService.Setup(s => s.DeleteECMemberAsync(7, 42, false, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            var controller = new AdminGovernanceController(governanceService.Object);
            SetUserContext(controller, memberId: null, role: "Admin", userId: 42);

            var result = await controller.DeleteECMember(7, false, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task AdminGovernanceController_DeleteECMember_ReturnsNotFound_WhenRecordMissing()
        {
            var governanceService = new Mock<IGovernanceService>();
            governanceService.Setup(s => s.DeleteECMemberAsync(7, 42, false, It.IsAny<CancellationToken>())).ReturnsAsync(false);
            var controller = new AdminGovernanceController(governanceService.Object);
            SetUserContext(controller, memberId: null, role: "Admin", userId: 42);

            var result = await controller.DeleteECMember(7, false, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task AdminGovernanceController_DeleteECMember_ReturnsUnauthorized_WhenCallerIdMissing()
        {
            var governanceService = new Mock<IGovernanceService>();
            var controller = new AdminGovernanceController(governanceService.Object);
            // No SetUserContext call — simulates a token with no resolvable NameIdentifier claim.
            controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext
            {
                HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext()
            };

            var result = await controller.DeleteECMember(7, false, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
            governanceService.Verify(s => s.DeleteECMemberAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()), Times.Never);
        }
    }
}

using System.Reflection;
using GHCAA.API.Controllers;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GHCAA.Tests.Controllers
{
    [TestFixture]
    public class AdminDevTrackerControllerTests : ControllerTestBase
    {
        [Test]
        public async Task GetOpenItems_PassesPriorityFilter_AndReturnsOk()
        {
            var service = new Mock<IDevTrackerService>();
            service.Setup(s => s.GetOpenItemsAsync(It.Is<DevTrackerFilterDto>(f => f.Priority == "P0"), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<DevTrackerItemDto>());
            var controller = new AdminDevTrackerController(service.Object);

            var result = await controller.GetOpenItems("P0", CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            service.VerifyAll();
        }

        [Test]
        public void Controller_RequiresSuperAdminOnlyPolicy()
        {
            var attr = typeof(AdminDevTrackerController).GetCustomAttribute<AuthorizeAttribute>();

            Assert.That(attr?.Policy, Is.EqualTo(Constants.Policies.SuperAdminOnly));
        }
    }
}

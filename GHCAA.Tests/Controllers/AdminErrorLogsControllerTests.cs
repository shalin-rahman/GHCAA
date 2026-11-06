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
    public class AdminErrorLogsControllerTests : ControllerTestBase
    {
        [Test]
        public async Task GetErrorLogs_MapsQueryToFilter_AndReturnsOk()
        {
            var service = new Mock<IErrorLogService>();
            ErrorLogFilterDto? seen = null;
            service.Setup(s => s.GetPagedAsync(It.IsAny<ErrorLogFilterDto>(), It.IsAny<CancellationToken>()))
                .Callback<ErrorLogFilterDto, CancellationToken>((f, _) => seen = f)
                .ReturnsAsync(new PagedResult<ErrorLogDto>());
            var controller = new AdminErrorLogsController(service.Object);
            var from = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc);

            var result = await controller.GetErrorLogs(from, null, "Error", "timeout", 2, 50);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            Assert.That(seen, Is.Not.Null);
            Assert.That(seen!.FromDate, Is.EqualTo(from));
            Assert.That(seen.Level, Is.EqualTo("Error"));
            Assert.That(seen.Query, Is.EqualTo("timeout"));
            Assert.That(seen.Page, Is.EqualTo(2));
            Assert.That(seen.PageSize, Is.EqualTo(50));
        }

        [Test]
        public void Controller_RequiresSuperAdminOnlyPolicy()
        {
            var attr = typeof(AdminErrorLogsController).GetCustomAttribute<AuthorizeAttribute>();

            Assert.That(attr?.Policy, Is.EqualTo(Constants.Policies.SuperAdminOnly));
        }
    }
}

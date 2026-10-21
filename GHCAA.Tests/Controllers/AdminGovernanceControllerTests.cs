using GHCAA.API.Controllers;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using static GHCAA.Domain.Enums;

namespace GHCAA.Tests.Controllers
{
    // 37.14m. The ER-34 handover checklist endpoints.
    [TestFixture]
    [Category("FR-34")]
    public class AdminGovernanceControllerTests : ControllerTestBase
    {
        private Mock<IGovernanceService> _governance = null!;
        private AdminGovernanceController _controller = null!;
        private readonly SetHandoverCheckRequest _tick = new() { Item = HandoverCheckItem.OfficialSeal, IsChecked = true };

        [SetUp]
        public void Setup()
        {
            _governance = new Mock<IGovernanceService>();
            _controller = new AdminGovernanceController(_governance.Object);
            SetUserContext(_controller, memberId: null, role: "Admin", userId: 42);
        }

        [Test]
        public async Task GetHandoverChecks_MissingTerm_Returns404()
        {
            _governance.Setup(s => s.GetHandoverChecksAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync((IReadOnlyList<HandoverCheckDto>?)null);

            Assert.That(await _controller.GetHandoverChecks(7, CancellationToken.None), Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task SetHandoverCheck_Saved_ReturnsTheList()
        {
            var list = new List<HandoverCheckDto> { new(HandoverCheckItem.OfficialSeal, true, 42, DateTime.UtcNow) };
            _governance.Setup(s => s.SetHandoverCheckAsync(7, _tick, 42, It.IsAny<CancellationToken>())).ReturnsAsync(true);
            _governance.Setup(s => s.GetHandoverChecksAsync(7, It.IsAny<CancellationToken>())).ReturnsAsync(list);

            var result = await _controller.SetHandoverCheck(7, _tick, CancellationToken.None) as OkObjectResult;

            Assert.That(result!.Value, Is.SameAs(list));
        }

        [Test]
        public async Task SetHandoverCheck_MissingTerm_Returns404()
        {
            _governance.Setup(s => s.SetHandoverCheckAsync(7, _tick, 42, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            Assert.That(await _controller.SetHandoverCheck(7, _tick, CancellationToken.None), Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task SetHandoverCheck_SameItemSavedTwiceAtOnce_Returns409()
        {
            _governance.Setup(s => s.SetHandoverCheckAsync(7, _tick, 42, It.IsAny<CancellationToken>()))
                       .ThrowsAsync(new InvalidOperationException("Another admin changed this item at the same time. Reload and try again."));

            var obj = await _controller.SetHandoverCheck(7, _tick, CancellationToken.None) as ObjectResult;

            Assert.That(obj!.StatusCode, Is.EqualTo(409));
        }

        [Test]
        public async Task SetHandoverCheck_UnknownItem_Returns400()
        {
            _governance.Setup(s => s.SetHandoverCheckAsync(7, _tick, 42, It.IsAny<CancellationToken>())).ThrowsAsync(new ArgumentException("Unknown handover item."));

            var obj = await _controller.SetHandoverCheck(7, _tick, CancellationToken.None) as ObjectResult;

            Assert.That(obj!.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task SetHandoverCheck_NoCallerId_Returns401()
        {
            _controller.ControllerContext = new ControllerContext { HttpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext() };

            Assert.That(await _controller.SetHandoverCheck(7, _tick, CancellationToken.None), Is.InstanceOf<UnauthorizedResult>());
        }
    }
}

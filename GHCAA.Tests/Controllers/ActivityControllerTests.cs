using GHCAA.API.Controllers;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GHCAA.Tests.Controllers
{
    // TODO 84.29. Spec 020 FR-041 to FR-043. The policies themselves are pinned in
    // AuthorizationPolicyReflectionTests.
    [TestFixture]
    [Category("FR-39")]
    public class ActivityControllerTests : ControllerTestBase
    {
        private Mock<IActivityService> _activity = null!;
        private ActivityController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _activity = new Mock<IActivityService>();
            _activity.Setup(x => x.GetMemberActivityAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ActivityLog>());
            _activity.Setup(x => x.GetRecentGlobalActivityAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<ActivityLog>());
            _controller = new ActivityController(_activity.Object);
        }

        [Test]
        public async Task GetMyActivity_ReadsCallersMemberId_Last20()
        {
            SetUserContext(_controller, 7, "Member");

            var result = await _controller.GetMyActivity(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            _activity.Verify(x => x.GetMemberActivityAsync(7, 20, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetMyActivity_AccountWithoutMember_Returns400()
        {
            SetUserContext(_controller, null, "Admin", 1);

            var result = await _controller.GetMyActivity(CancellationToken.None);

            Assert.That((result as ObjectResult)?.StatusCode, Is.EqualTo(400));
            _activity.Verify(x => x.GetMemberActivityAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Test]
        public async Task GetMemberActivity_AsksForThatMember_Last50()
        {
            SetUserContext(_controller, 1, "Admin");

            var result = await _controller.GetMemberActivity(42, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            _activity.Verify(x => x.GetMemberActivityAsync(42, 50, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetGlobalActivity_AsksForLast50()
        {
            SetUserContext(_controller, 1, "SuperAdmin");

            var result = await _controller.GetGlobalActivity(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            _activity.Verify(x => x.GetRecentGlobalActivityAsync(50, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}

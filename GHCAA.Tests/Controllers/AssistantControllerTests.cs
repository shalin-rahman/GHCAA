using System.Reflection;
using GHCAA.API.Controllers;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GHCAA.Tests.Controllers
{
    [TestFixture]
    public class AssistantControllerTests : ControllerTestBase
    {
        private Mock<IAssistantService> _service = null!;
        private AssistantController _controller = null!;

        [SetUp]
        public void Setup()
        {
            _service = new Mock<IAssistantService>();
            _controller = new AssistantController(_service.Object);
            SetMemberContext(_controller);
        }

        [Category("FR-001")]
        [Test]
        public async Task Ask_ReturnsOk_WithServiceAnswer()
        {
            var answer = new AssistantResponseDto { Answer = "Found 2 members." };
            _service.Setup(s => s.AskAsync("doctors in Dhaka", It.IsAny<CancellationToken>())).ReturnsAsync(answer);

            var result = await _controller.Ask(new AssistantQueryDto { Query = "doctors in Dhaka" }, CancellationToken.None);

            Assert.That((result as OkObjectResult)?.Value, Is.SameAs(answer));
        }

        [Category("FR-002")]
        [TestCase("")]
        [TestCase("   ")]
        public async Task Ask_BlankQuery_Returns400(string query)
        {
            var result = await _controller.Ask(new AssistantQueryDto { Query = query }, CancellationToken.None);

            Assert.That((result as ObjectResult)?.StatusCode, Is.EqualTo(400));
            _service.Verify(s => s.AskAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Category("FR-001")]
        [Test]
        public void Controller_RequiresSignedInUser()
        {
            Assert.That(typeof(AssistantController).GetCustomAttribute<AuthorizeAttribute>(), Is.Not.Null);
        }
    }
}

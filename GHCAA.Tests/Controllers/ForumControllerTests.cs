using System.Threading.Tasks;
using GHCAA.API.Controllers;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Controllers
{
    [TestFixture]
    public class ForumControllerTests : ControllerTestBase
    {
        private Mock<IForumService> _forumServiceMock;
        private ForumController _controller;

        [SetUp]
        public void Setup()
        {
            _forumServiceMock = new Mock<IForumService>();
            _controller = new ForumController(_forumServiceMock.Object);
            SetUserContext(_controller, memberId: 10, role: "Member");
        }

        [Test]
        public async Task GetTopic_UnknownTopic_Returns404Problem()
        {
            _forumServiceMock.Setup(x => x.GetTopicByIdAsync(99)).ReturnsAsync((GHCAA.Application.DTOs.ForumTopicDto?)null);

            var result = await _controller.GetTopic(99);

            var obj = result.Result as ObjectResult;
            Assert.That(obj, Is.Not.Null);
            Assert.That(obj!.StatusCode, Is.EqualTo(404));
            Assert.That(((ProblemDetails)obj.Value!).Detail, Is.EqualTo("Topic not found."));
        }
    }
}

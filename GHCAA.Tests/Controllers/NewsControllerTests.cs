using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Controllers;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Controllers
{
    [TestFixture]
    public class NewsControllerTests
    {
        private Mock<INewsService> _newsServiceMock;
        private NewsController _controller;

        [SetUp]
        public void Setup()
        {
            _newsServiceMock = new Mock<INewsService>();
            _controller = new NewsController(_newsServiceMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim(ClaimTypes.NameIdentifier, "1")
            }, "TestAuthentication"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Test]
        public async Task GetActiveNews_ReturnsOk()
        {
            _newsServiceMock.Setup(x => x.GetActiveNewsAsync(It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new List<NewsPostDto>());

            var result = await _controller.GetActiveNews(CancellationToken.None);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetNewsById_ReturnsOk_IfFound()
        {
            _newsServiceMock.Setup(x => x.GetNewsByIdAsync(1, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new NewsPostDto { Id = 1 });

            var result = await _controller.GetNewsById(1, CancellationToken.None);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetAllNewsForAdmin_ReturnsOk()
        {
            _newsServiceMock.Setup(x => x.GetAllNewsForAdminAsync(It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new List<NewsPostDto>());

            var result = await _controller.GetAllNewsForAdmin(CancellationToken.None);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task CreateNews_ReturnsCreatedAtAction()
        {
            var dto = new CreateNewsDto { Title = "Test" };
            _newsServiceMock.Setup(x => x.CreateNewsAsync(dto, 1, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new NewsPostDto { Id = 2 });

            var result = await _controller.CreateNews(dto, CancellationToken.None);
            Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
        }

        [Test]
        public async Task UpdateNews_ReturnsOk()
        {
            var dto = new UpdateNewsDto { Title = "Edit" };
            _newsServiceMock.Setup(x => x.UpdateNewsAsync(It.IsAny<UpdateNewsDto>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new NewsPostDto { Id = 1 });

            var result = await _controller.UpdateNews(1, dto, CancellationToken.None);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task DeleteNews_ReturnsOk_OnSuccess()
        {
            _newsServiceMock.Setup(x => x.DeleteNewsAsync(1, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);

            var result = await _controller.DeleteNews(1, CancellationToken.None);
            Assert.That(result, Is.InstanceOf<OkResult>());
        }
    }
}

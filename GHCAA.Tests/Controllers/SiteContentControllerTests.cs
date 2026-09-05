using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Controllers;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Controllers
{
    [TestFixture]
    public class SiteContentControllerTests : ControllerTestBase
    {
        private Mock<ISiteContentService> _serviceMock;
        private SiteContentController _controller;

        [SetUp]
        public void Setup()
        {
            _serviceMock = new Mock<ISiteContentService>();
            _controller = new SiteContentController(_serviceMock.Object);
            SetUserContext(_controller, null, "Admin", 1);
        }

        [Category("FR-45")]
        [Test]
        public async Task GetByGroup_ReturnsOk()
        {
            _serviceMock.Setup(x => x.GetActiveByGroupAsync("about", It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new List<SiteContentDto> { new() { Id = 1, Key = "about-origin" } });

            var result = await _controller.GetByGroup("about", CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            _serviceMock.Verify(x => x.GetActiveByGroupAsync("about", It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetById_ReturnsNotFound_WhenMissing()
        {
            _serviceMock.Setup(x => x.GetByIdAsync(42, It.IsAny<CancellationToken>())).ReturnsAsync((SiteContentDto?)null);

            var result = await _controller.GetById(42, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Category("FR-45")]
        [Category("FR-43")]
        [Test]
        public async Task Create_ReturnsCreated_AndPassesAdminId()
        {
            _serviceMock.Setup(x => x.CreateAsync(It.IsAny<UpsertSiteContentDto>(), 1, It.IsAny<CancellationToken>()))
                        .ReturnsAsync(new SiteContentDto { Id = 7 });

            var result = await _controller.Create(new UpsertSiteContentDto { Key = "k", Title = "t", BodyHtml = "<p>x</p>" }, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
            _serviceMock.Verify(x => x.CreateAsync(It.IsAny<UpsertSiteContentDto>(), 1, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task Update_ReturnsNotFound_WhenServiceThrowsKeyNotFound()
        {
            _serviceMock.Setup(x => x.UpdateAsync(5, It.IsAny<UpsertSiteContentDto>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                        .ThrowsAsync(new KeyNotFoundException());

            var result = await _controller.Update(5, new UpsertSiteContentDto { Key = "k", Title = "t", BodyHtml = "<p>x</p>" }, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task Delete_ReturnsNotFound_WhenMissing()
        {
            _serviceMock.Setup(x => x.DeleteAsync(3, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = await _controller.Delete(3, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }
    }
}

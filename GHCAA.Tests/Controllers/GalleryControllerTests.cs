using System.Collections.Generic;
using System.IO;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Controllers;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Controllers
{
    [TestFixture]
    public class GalleryControllerTests
    {
        private Mock<IGalleryService> _galleryServiceMock;
        private Mock<IFileStorageService> _fileStorageMock;
        private Mock<IFileValidationService> _fileValidationServiceMock;
        private GalleryController _controller;

        [SetUp]
        public void Setup()
        {
            _galleryServiceMock = new Mock<IGalleryService>();
            _fileStorageMock = new Mock<IFileStorageService>();
            _fileValidationServiceMock = new Mock<IFileValidationService>();
            _fileValidationServiceMock.Setup(x => x.Validate(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<FileCategory>(), It.IsAny<long>()))
                                       .Returns(FileValidationResult.Ok());
            _controller = new GalleryController(_galleryServiceMock.Object, _fileStorageMock.Object, _fileValidationServiceMock.Object);

            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[] {
                new Claim("MemberId", "10")
            }, "TestAuthentication"));

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Test]
        public async Task GetGalleries_ReturnsOk()
        {
            _galleryServiceMock.Setup(x => x.GetAllGalleriesAsync(true, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new List<EventGallery>());

            var result = await _controller.GetGalleries(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetGallery_ReturnsOk_IfFound()
        {
            _galleryServiceMock.Setup(x => x.GetGalleryByIdAsync(1, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new EventGallery { Id = 1 });

            var result = await _controller.GetGallery(1, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task CreateGallery_ReturnsCreatedAtAction()
        {
            var gallery = new EventGallery { Title = "Test" };
            _galleryServiceMock.Setup(x => x.CreateEventGalleryAsync(gallery, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new EventGallery { Id = 10 });

            var result = await _controller.CreateGallery(gallery, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
        }

        [Test]
        public async Task AddPhotos_ReturnsOk_OnSuccess()
        {
            var paths = new List<string> { "path1" };
            _galleryServiceMock.Setup(x => x.AddPhotosToGalleryAsync(1, paths, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(true);

            var result = await _controller.AddPhotos(1, paths, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkResult>());
        }

        [Test]
        public async Task DeleteGallery_ReturnsOk_OnSuccess()
        {
            _galleryServiceMock.Setup(x => x.DeleteGalleryAsync(1, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(true);

            var result = await _controller.DeleteGallery(1, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkResult>());
        }
    }
}

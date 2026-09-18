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

        [Category("FR-53")]
        [Test]
        public async Task CreateAlbum_ReturnsCreatedAtAction()
        {
            var request = new GalleryController.CreateAlbumRequest { Title = "My Album", Description = "Desc" };
            _galleryServiceMock.Setup(x => x.CreateMemberAlbumAsync(10, "My Album", "Desc", It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new EventGallery { Id = 5, Title = "My Album" });

            var result = await _controller.CreateAlbum(request, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
        }

        [Category("FR-53")]
        [Test]
        public async Task GetMyAlbums_ReturnsOk()
        {
            _galleryServiceMock.Setup(x => x.GetMemberAlbumsAsync(10, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new List<EventGallery>());

            var result = await _controller.GetMyAlbums(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Category("FR-53")]
        [Test]
        public async Task AddPhotoToAlbum_ReturnsForbid_WhenNotOwnerAndNotAdmin()
        {
            _galleryServiceMock.Setup(x => x.GetGalleryByIdAsync(5, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new EventGallery { Id = 5, OwnerMemberId = 999 });

            var file = new Mock<IFormFile>();
            var result = await _controller.AddPhotoToAlbum(5, file.Object, "caption", CancellationToken.None);

            Assert.That(result, Is.InstanceOf<ForbidResult>());
        }

        [Category("FR-53")]
        [Test]
        public async Task ApproveGallery_ReturnsOk_OnSuccess()
        {
            _galleryServiceMock.Setup(x => x.ApproveGalleryAsync(5, true, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(true);

            var result = await _controller.ApproveGallery(5, true, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Category("FR-53")]
        [Test]
        public async Task RejectGallery_ReturnsOk_OnSuccess()
        {
            _galleryServiceMock.Setup(x => x.RejectGalleryAsync(5, "Not appropriate", true, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(true);

            var result = await _controller.RejectGallery(5, new GalleryController.RejectRequest { Reason = "Not appropriate" }, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task ApprovePhoto_ReturnsNotFound_WhenMissing()
        {
            _galleryServiceMock.Setup(x => x.ApprovePhotoAsync(99, true, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(false);

            var result = await _controller.ApprovePhoto(99, true, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        // 82.52 batch 3: notifyMember flows straight through to the service.
        [Category("FR-53")]
        [Test]
        public async Task ApproveGallery_PassesNotifyMemberFalse_ToService()
        {
            _galleryServiceMock.Setup(x => x.ApproveGalleryAsync(5, false, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(true);

            var result = await _controller.ApproveGallery(5, false, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Category("FR-53")]
        [Test]
        public async Task RejectGallery_PassesNotifyMemberFalse_ToService()
        {
            _galleryServiceMock.Setup(x => x.RejectGalleryAsync(5, "Not appropriate", false, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(true);

            var result = await _controller.RejectGallery(5, new GalleryController.RejectRequest { Reason = "Not appropriate", NotifyMember = false }, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task UploadPhoto_ReturnsOk_OnSuccess()
        {
            var file = Mock.Of<IFormFile>(f => f.FileName == "photo.jpg" && f.Length == 1024);
            _fileStorageMock.Setup(x => x.SaveFileAsync(It.IsAny<Stream>(), "photo.jpg", 10, It.IsAny<GHCAA.Domain.Enums.FileUploadType>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync("/uploads/gallery/photo.jpg");

            var result = await _controller.UploadPhoto(file, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task UploadPhoto_ReturnsBadRequest_WhenValidationFails()
        {
            _fileValidationServiceMock.Setup(x => x.Validate(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<FileCategory>(), It.IsAny<long>()))
                                       .Returns(FileValidationResult.Fail("File too large."));
            var file = Mock.Of<IFormFile>(f => f.FileName == "photo.jpg" && f.Length == 1024);

            var result = await _controller.UploadPhoto(file, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }

        [Test]
        public async Task ToggleActive_ReturnsOk_WithFlippedFlag()
        {
            var gallery = new EventGallery { Id = 5, IsActive = true };
            _galleryServiceMock.Setup(x => x.GetGalleryByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(gallery);
            _galleryServiceMock.Setup(x => x.UpdateEventGalleryAsync(gallery, It.IsAny<CancellationToken>())).ReturnsAsync(gallery);

            var result = await _controller.ToggleActive(5, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            Assert.That(gallery.IsActive, Is.False);
        }

        [Test]
        public async Task ToggleActive_ReturnsNotFound_WhenGalleryMissing()
        {
            _galleryServiceMock.Setup(x => x.GetGalleryByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((EventGallery?)null);

            var result = await _controller.ToggleActive(99, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task ToggleFeatured_ReturnsOk_WithFlippedFlag()
        {
            var gallery = new EventGallery { Id = 5, IsFeatured = false };
            _galleryServiceMock.Setup(x => x.GetGalleryByIdAsync(5, It.IsAny<CancellationToken>())).ReturnsAsync(gallery);
            _galleryServiceMock.Setup(x => x.UpdateEventGalleryAsync(gallery, It.IsAny<CancellationToken>())).ReturnsAsync(gallery);

            var result = await _controller.ToggleFeatured(5, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            Assert.That(gallery.IsFeatured, Is.True);
        }

        [Test]
        public async Task ToggleFeatured_ReturnsNotFound_WhenGalleryMissing()
        {
            _galleryServiceMock.Setup(x => x.GetGalleryByIdAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync((EventGallery?)null);

            var result = await _controller.ToggleFeatured(99, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task SubmitMemberPhoto_ReturnsCreatedAtAction_OnSuccess()
        {
            var file = Mock.Of<IFormFile>(f => f.FileName == "photo.jpg" && f.Length == 1024);
            var gallery = new EventGallery { Id = 7 };
            _fileStorageMock.Setup(x => x.SaveFileAsync(It.IsAny<Stream>(), "photo.jpg", 10, It.IsAny<GHCAA.Domain.Enums.FileUploadType>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync("/uploads/gallery/photo.jpg");
            _galleryServiceMock.Setup(x => x.CreateMemberAlbumAsync(10, "My Album", null, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(gallery);
            _galleryServiceMock.Setup(x => x.AddMemberPhotoToAlbumAsync(10, 7, "/uploads/gallery/photo.jpg", null, It.IsAny<CancellationToken>()))
                               .ReturnsAsync(new EventPhoto());

            var result = await _controller.SubmitMemberPhoto("My Album", null, file, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
        }

        [Test]
        public async Task SubmitMemberPhoto_ReturnsBadRequest_WhenValidationFails()
        {
            _fileValidationServiceMock.Setup(x => x.Validate(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<FileCategory>(), It.IsAny<long>()))
                                       .Returns(FileValidationResult.Fail("Unsupported format."));
            var file = Mock.Of<IFormFile>(f => f.FileName == "photo.jpg" && f.Length == 1024);

            var result = await _controller.SubmitMemberPhoto("My Album", null, file, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result).StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
        }
    }
}

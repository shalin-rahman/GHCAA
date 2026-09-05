using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
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
using GHCAA.Domain;

namespace GHCAA.Tests.Controllers
{
    [TestFixture]
    public class NewsControllerTests : ControllerTestBase
    {
        private Mock<INewsService> _newsServiceMock;
        private Mock<IFileStorageService> _fileStorageServiceMock;
        private Mock<IFileValidationService> _fileValidationServiceMock;
        private NewsController _controller;

        [SetUp]
        public void Setup()
        {
            _newsServiceMock = new Mock<INewsService>();
            _fileStorageServiceMock = new Mock<IFileStorageService>();
            _fileValidationServiceMock = new Mock<IFileValidationService>();
            _fileValidationServiceMock.Setup(x => x.Validate(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<long>(), It.IsAny<FileCategory>(), It.IsAny<long>()))
                                       .Returns(FileValidationResult.Ok());
            _controller = new NewsController(_newsServiceMock.Object, _fileStorageServiceMock.Object, _fileValidationServiceMock.Object);

            SetUserContext(_controller, null, "Admin", 1); // UserId 1, no specific MemberId
        }

        [Test]
        public async Task GetActiveNews_ReturnsOk()
        {
            _newsServiceMock.Setup(x => x.GetActiveNewsAsync(It.IsAny<Enums.ArticleCategory?>(), It.IsAny<Enums.PostType?>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new List<NewsPostDto>());

            var result = await _controller.GetActiveNews(null, null, CancellationToken.None);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task GetActiveNews_PassesPostTypeFilterToService()
        {
            _newsServiceMock.Setup(x => x.GetActiveNewsAsync(It.IsAny<Enums.ArticleCategory?>(), Enums.PostType.Notice, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new List<NewsPostDto>());

            var result = await _controller.GetActiveNews(null, Enums.PostType.Notice, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            _newsServiceMock.Verify(x => x.GetActiveNewsAsync(null, Enums.PostType.Notice, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Category("FR-27")]
        [Test]
        public async Task SubmitArticle_Forbids_WhenMemberSubmitsNotice()
        {
            SetUserContext(_controller, null, "Member", 5);

            var result = await _controller.SubmitArticle(
                new CreateNewsDto { Title = "Notice", Content = "Body", PostType = Enums.PostType.Notice },
                CancellationToken.None);

            Assert.That(result, Is.InstanceOf<ForbidResult>());
            _newsServiceMock.Verify(x => x.CreateNewsAsync(It.IsAny<CreateNewsDto>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
        }

        [Category("FR-27")]
        [Test]
        public async Task SubmitArticle_Allows_WhenAdminSubmitsNotice()
        {
            _newsServiceMock.Setup(x => x.CreateNewsAsync(It.IsAny<CreateNewsDto>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new NewsPostDto { Id = 9, PostType = Enums.PostType.Notice });

            var result = await _controller.SubmitArticle(
                new CreateNewsDto { Title = "Notice", Content = "Body", PostType = Enums.PostType.Notice },
                CancellationToken.None);

            Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
        }

        [Test]
        public async Task UploadDocument_ReturnsOk_WithStoredUrl()
        {
            _fileStorageServiceMock.Setup(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<int>(), Enums.FileUploadType.NoticeDocument, It.IsAny<CancellationToken>()))
                                   .ReturnsAsync("uploads/members/1/notice_1.pdf");

            var file = new FormFile(new MemoryStream(new byte[] { 1, 2, 3 }), 0, 3, "file", "notice.pdf")
            {
                Headers = new HeaderDictionary(),
                ContentType = "application/pdf"
            };

            var result = await _controller.UploadDocument(file, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            _fileStorageServiceMock.Verify(x => x.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<int>(), Enums.FileUploadType.NoticeDocument, It.IsAny<CancellationToken>()), Times.Once);
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

        [Category("FR-28")]
        [Test]
        public async Task UpdateNews_ReturnsOk()
        {
            var dto = new UpdateNewsDto { Title = "Edit" };
            _newsServiceMock.Setup(x => x.UpdateNewsAsync(It.IsAny<UpdateNewsDto>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(new NewsPostDto { Id = 1 });

            var result = await _controller.UpdateNews(1, dto, CancellationToken.None);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Category("FR-28")]
        [Test]
        public async Task DeleteNews_ReturnsOk_OnSuccess()
        {
            _newsServiceMock.Setup(x => x.DeleteNewsAsync(1, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);

            var result = await _controller.DeleteNews(1, CancellationToken.None);
            Assert.That(result, Is.InstanceOf<OkResult>());
        }

        [Category("FR-27")]
        [Test]
        public async Task ApproveArticle_ReturnsOk_OnSuccess()
        {
            _newsServiceMock.Setup(x => x.ApproveArticleAsync(1, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);

            var result = await _controller.ApproveArticle(1, CancellationToken.None);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Category("FR-27")]
        [Test]
        public async Task RejectArticle_ReturnsOk_OnSuccess()
        {
            _newsServiceMock.Setup(x => x.RejectArticleAsync(1, It.IsAny<CancellationToken>()))
                            .ReturnsAsync(true);

            var result = await _controller.RejectArticle(1, CancellationToken.None);
            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        // Regression coverage for preprod bug: POST /api/news returned 400 whenever the admin
        // used the "upload image" flow first, because the upload endpoint returns an app-relative
        // path (e.g. "/uploads/news/x.jpg") and [Url] rejects anything without a scheme.
        [TestCase(null, true)]
        [TestCase("", true)]
        [TestCase("/uploads/news/news_123_abcd.jpg", true)]
        [TestCase("https://example.com/image.jpg", true)]
        [TestCase("http://example.com/image.jpg", true)]
        [TestCase("javascript:alert(1)", false)]
        [TestCase("not a url", false)]
        public void CreateNewsDto_ImageUrl_AcceptsRelativePathsAndAbsoluteHttpUrls(string? imageUrl, bool expectedValid)
        {
            var dto = new CreateNewsDto { Title = "Valid Title", Content = "Valid content long enough.", ImageUrl = imageUrl };
            var context = new ValidationContext(dto);
            var results = new List<ValidationResult>();

            var isValid = Validator.TryValidateObject(dto, context, results, validateAllProperties: true);

            var imageUrlHasError = results.Exists(r => r.MemberNames.Contains(nameof(CreateNewsDto.ImageUrl)));
            Assert.That(!imageUrlHasError, Is.EqualTo(expectedValid));
        }
    }
}

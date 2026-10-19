using System.Text;
using FluentAssertions;
using GHCAA.API.Extensions;
using GHCAA.Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace GHCAA.Tests.Extensions;

// /api/uploads when FileStorage:Provider is S3: the bucket stands in for the uploads folder.
[TestFixture]
public class UploadProxyTests
{
    private const string ETag = "\"abc123\"";
    private static readonly DateTimeOffset Modified = new(2026, 10, 1, 8, 0, 0, TimeSpan.Zero);

    private Mock<IFileStorageService> _storage = null!;
    private bool _nextCalled;

    [SetUp]
    public void Setup()
    {
        _storage = new Mock<IFileStorageService>();
        _nextCalled = false;
    }

    private DefaultHttpContext Request(string method, string path)
    {
        var ctx = new DefaultHttpContext
        {
            RequestServices = new ServiceCollection().AddLogging().AddSingleton(_storage.Object).BuildServiceProvider()
        };
        ctx.Request.Method = method;
        ctx.Request.Path = path;
        ctx.Response.Body = new MemoryStream();
        return ctx;
    }

    private Task Next(HttpContext _)
    {
        _nextCalled = true;
        return Task.CompletedTask;
    }

    private void StoreHolds(string key, string body) =>
        _storage.Setup(x => x.OpenReadAsync(key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new StoredFile(new MemoryStream(Encoding.UTF8.GetBytes(body)), Modified, ETag));

    private static string BodyOf(HttpContext ctx)
    {
        ctx.Response.Body.Position = 0;
        return new StreamReader(ctx.Response.Body).ReadToEnd();
    }

    [Test]
    public async Task Get_ServesTheStoredFile()
    {
        StoreHolds("uploads/members/3/photo/a.jpg", "jpeg");
        var ctx = Request("GET", "/api/uploads/members/3/photo/a.jpg");

        await StaticFilesExtensions.ServeUploadFromStore(ctx, Next);

        _nextCalled.Should().BeFalse();
        BodyOf(ctx).Should().Be("jpeg");
        ctx.Response.ContentType.Should().Be("image/jpeg");
        ctx.Response.ContentLength.Should().Be(4);
        ctx.Response.Headers.CacheControl.ToString().Should().Be(StaticFilesExtensions.UploadCacheControl);
        ctx.Response.Headers.XContentTypeOptions.ToString().Should().Be("nosniff");
    }

    [Category("NFR-P4")]
    [Test]
    public async Task Get_SendsTheValidators()
    {
        StoreHolds("uploads/members/3/photo/a.jpg", "jpeg");
        var ctx = Request("GET", "/api/uploads/members/3/photo/a.jpg");

        await StaticFilesExtensions.ServeUploadFromStore(ctx, Next);

        ctx.Response.Headers.ETag.ToString().Should().Be(ETag);
        ctx.Response.Headers.LastModified.ToString().Should().Be(Modified.ToString("R"));
    }

    [Category("NFR-P4")]
    [Test]
    public async Task IfNoneMatch_WithTheSameTag_GivesNotModified()
    {
        StoreHolds("uploads/members/3/photo/a.jpg", "jpeg");
        var ctx = Request("GET", "/api/uploads/members/3/photo/a.jpg");
        ctx.Request.Headers.IfNoneMatch = ETag;

        await StaticFilesExtensions.ServeUploadFromStore(ctx, Next);

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status304NotModified);
        BodyOf(ctx).Should().BeEmpty();
    }

    [Category("NFR-P4")]
    [Test]
    public async Task Range_GivesPartialContent()
    {
        StoreHolds("uploads/members/3/photo/a.jpg", "0123456789");
        var ctx = Request("GET", "/api/uploads/members/3/photo/a.jpg");
        ctx.Request.Headers.Range = "bytes=2-5";

        await StaticFilesExtensions.ServeUploadFromStore(ctx, Next);

        ctx.Response.StatusCode.Should().Be(StatusCodes.Status206PartialContent);
        ctx.Response.Headers.ContentRange.ToString().Should().Be("bytes 2-5/10");
        BodyOf(ctx).Should().Be("2345");
    }

    [Test]
    public async Task Head_SendsHeadersWithoutTheBody()
    {
        StoreHolds("uploads/members/3/photo/a.jpg", "jpeg");
        var ctx = Request("HEAD", "/api/uploads/members/3/photo/a.jpg");

        await StaticFilesExtensions.ServeUploadFromStore(ctx, Next);

        ctx.Response.ContentLength.Should().Be(4);
        BodyOf(ctx).Should().BeEmpty();
    }

    [Category("NFR-P4")]
    [Test]
    public async Task MissingThumbnail_FallsBackToTheFullPhoto()
    {
        StoreHolds("uploads/members/3/galleryphoto/a.jpg", "full");
        var ctx = Request("GET", "/api/uploads/members/3/galleryphoto/thumbs/a.jpg");

        await StaticFilesExtensions.ServeUploadFromStore(ctx, Next);

        BodyOf(ctx).Should().Be("full");
    }

    [Category("NFR-R4")]
    [Test]
    public async Task MissingFile_PassesOnToThePlaceholderStep()
    {
        var ctx = Request("GET", "/api/uploads/members/3/photo/gone.jpg");

        await StaticFilesExtensions.ServeUploadFromStore(ctx, Next);

        _nextCalled.Should().BeTrue();
    }

    [TestCase("GET", "/api/members/3")]
    [TestCase("GET", "/api/uploads")]
    [TestCase("POST", "/api/uploads/members/3/photo/a.jpg")]
    public async Task OtherRequests_AreNotTouched(string method, string path)
    {
        var ctx = Request(method, path);

        await StaticFilesExtensions.ServeUploadFromStore(ctx, Next);

        _nextCalled.Should().BeTrue();
        _storage.Verify(x => x.OpenReadAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // The proxy only builds the key. An encoded "%2e%2e" arrives decoded, and the store refuses a
    // key that climbs out of uploads/ (see S3FileStorageServiceTests), so secure_uploads/ stays shut.
    [Category("NFR-S7")]
    [TestCase("/api/uploads/../secure_uploads/members/3/certificate/c.pdf")]
    [TestCase("/api/uploads/%2e%2e/secure_uploads/members/3/certificate/c.pdf")]
    public async Task ClimbingOutOfUploads_IsLeftToTheStoreToRefuse(string path)
    {
        var ctx = Request("GET", path);

        await StaticFilesExtensions.ServeUploadFromStore(ctx, Next);

        _storage.Verify(x => x.OpenReadAsync("uploads/../secure_uploads/members/3/certificate/c.pdf", It.IsAny<CancellationToken>()), Times.Once);
        _nextCalled.Should().BeTrue();
        BodyOf(ctx).Should().BeEmpty();
    }

    [Test]
    public async Task StreamWithoutLength_IsStillServed()
    {
        _storage.Setup(x => x.OpenReadAsync("uploads/members/3/photo/a.jpg", It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new StoredFile(new UnseekableStream("jpeg"), null, null));
        var ctx = Request("GET", "/api/uploads/members/3/photo/a.jpg");

        await StaticFilesExtensions.ServeUploadFromStore(ctx, Next);

        ctx.Response.ContentLength.Should().BeNull();
        BodyOf(ctx).Should().Be("jpeg");
    }

    private sealed class UnseekableStream(string body) : MemoryStream(Encoding.UTF8.GetBytes(body))
    {
        public override bool CanSeek => false;
        public override long Length => throw new NotSupportedException();
    }
}

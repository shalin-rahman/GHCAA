using System.Net;
using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
using FluentAssertions;
using GHCAA.Domain;
using GHCAA.Infrastructure.Options;
using GHCAA.Infrastructure.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SkiaSharp;

namespace GHCAA.Tests.Services;

[TestFixture]
public class S3FileStorageServiceTests
{
    private const string Bucket = "test-bucket";
    private Mock<IAmazonS3> _s3 = null!;
    private List<(string Key, byte[] Body, string ContentType)> _puts = null!;
    private S3FileStorageService _service = null!;

    [SetUp]
    public void Setup()
    {
        _s3 = new Mock<IAmazonS3>();
        _puts = new();
        _s3.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
            .Callback<PutObjectRequest, CancellationToken>((r, _) =>
            {
                using var copy = new MemoryStream();
                r.InputStream.CopyTo(copy);
                _puts.Add((r.Key, copy.ToArray(), r.ContentType));
            })
            .ReturnsAsync(new PutObjectResponse());
        _service = Build(new FileStorageOptions { Provider = FileStorageOptions.S3Provider, S3 = { BucketName = Bucket } });
    }

    private S3FileStorageService Build(FileStorageOptions options) =>
        new(_s3.Object, Options.Create(options), new Mock<ILogger<S3FileStorageService>>().Object);

    private static MemoryStream CreateJpeg(int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Jpeg, 90);
        return new MemoryStream(data.ToArray());
    }

    private void BucketHolds(string key, string body) =>
        _s3.Setup(x => x.GetObjectAsync(Bucket, key, It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new GetObjectResponse
            {
                ResponseStream = new MemoryStream(Encoding.UTF8.GetBytes(body)),
                ContentLength = Encoding.UTF8.GetByteCount(body),
                ETag = "\"v1\"",
                LastModified = new DateTime(2026, 10, 1, 8, 0, 0, DateTimeKind.Unspecified)
            });

    [Test]
    public void Constructor_Throws_WhenBucketNameIsMissing()
    {
        var act = () => Build(new FileStorageOptions { Provider = FileStorageOptions.S3Provider });

        act.Should().Throw<InvalidOperationException>().WithMessage("*BucketName*");
    }

    [Test]
    public async Task SaveFileAsync_PutsTheFileAtItsStoredPath()
    {
        var path = await _service.SaveFileAsync(new MemoryStream(Encoding.UTF8.GetBytes("pdf")), "notice.pdf", 7, Enums.FileUploadType.NoticeDocument);

        _puts.Should().ContainSingle();
        _puts[0].Key.Should().Be(path);
        _puts[0].ContentType.Should().Be("application/pdf");
        Encoding.UTF8.GetString(_puts[0].Body).Should().Be("pdf");
    }

    [Test]
    public async Task SaveFileAsync_SecureType_GoesUnderTheSecureTree()
    {
        var path = await _service.SaveFileAsync(new MemoryStream(Encoding.UTF8.GetBytes("pdf")), "cert.pdf", 7, Enums.FileUploadType.Certificate);

        path.Should().StartWith("secure_uploads/members/7/");
        _puts.Single().Key.Should().Be(path);
    }

    [Category("NFR-P4")]
    [Test]
    public async Task SaveFileAsync_GalleryPhoto_PutsPhotoAndThumbnail()
    {
        var path = await _service.SaveFileAsync(CreateJpeg(1600, 1200), "album.jpg", 3, Enums.FileUploadType.GalleryPhoto);

        _puts.Select(p => p.Key).Should().BeEquivalentTo(new[]
        {
            path,
            path.Replace("/galleryphoto/", "/galleryphoto/" + Constants.Defaults.ThumbnailFolder + "/")
        });
        _puts.Should().OnlyContain(p => p.ContentType == "image/jpeg");
    }

    // The bucket is not asked per photo. The /api/uploads proxy falls back to the full photo.
    [Test]
    public void GetThumbnailPath_OffersThePathWithoutAskingTheBucket()
    {
        _service.GetThumbnailPath("/uploads/members/3/galleryphoto/a.jpg")
            .Should().Be("/uploads/members/3/galleryphoto/thumbs/a.jpg");
        _s3.Verify(x => x.GetObjectMetadataAsync(It.IsAny<GetObjectMetadataRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task OpenReadAsync_ReturnsTheObjectBody()
    {
        BucketHolds("uploads/members/3/photo/a.jpg", "jpeg-bytes");

        var file = await _service.OpenReadAsync("/uploads/members/3/photo/a.jpg");

        file.Should().NotBeNull();
        await using var stream = file!.Content;
        stream.CanSeek.Should().BeTrue();
        stream.Length.Should().Be(10);
        file.ETag.Should().Be("\"v1\"");
        file.LastModified.Should().Be(new DateTimeOffset(2026, 10, 1, 8, 0, 0, TimeSpan.Zero));
        using var reader = new StreamReader(stream);
        (await reader.ReadToEndAsync()).Should().Be("jpeg-bytes");
    }

    // A range request seeks before reading. The rest of the object is fetched from that point,
    // pinned to the same version by its ETag.
    [Category("NFR-P4")]
    [Test]
    public async Task OpenReadAsync_SeekThenRead_AsksForARangeOfTheSameVersion()
    {
        BucketHolds("uploads/members/3/photo/a.jpg", "0123456789");
        GetObjectRequest? ranged = null;
        _s3.Setup(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()))
            .Callback<GetObjectRequest, CancellationToken>((r, _) => ranged = r)
            .ReturnsAsync(() => new GetObjectResponse { ResponseStream = new MemoryStream("23456789"u8.ToArray()), ContentLength = 8 });

        var file = await _service.OpenReadAsync("uploads/members/3/photo/a.jpg");
        await using var stream = file!.Content;
        stream.Seek(2, SeekOrigin.Begin);
        var buffer = new byte[4];
        var read = await stream.ReadAsync(buffer);

        Encoding.UTF8.GetString(buffer, 0, read).Should().Be("2345");
        ranged.Should().NotBeNull();
        ranged!.ByteRange.Start.Should().Be(2);
        ranged.ByteRange.End.Should().Be(9);
        ranged.EtagToMatch.Should().Be("\"v1\"");
    }

    [Test]
    public async Task OpenReadAsync_ReadFromTheStart_UsesTheFirstResponseOnly()
    {
        BucketHolds("uploads/members/3/photo/a.jpg", "0123456789");

        var file = await _service.OpenReadAsync("uploads/members/3/photo/a.jpg");
        await using var stream = file!.Content;
        stream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(stream);
        (await reader.ReadToEndAsync()).Should().Be("0123456789");

        _s3.Verify(x => x.GetObjectAsync(It.IsAny<GetObjectRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Category("NFR-R4")]
    [Test]
    public async Task OpenReadAsync_ReturnsNull_WhenTheObjectIsMissing()
    {
        _s3.Setup(x => x.GetObjectAsync(Bucket, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonS3Exception("missing") { StatusCode = HttpStatusCode.NotFound });

        (await _service.OpenReadAsync("uploads/members/3/photo/gone.jpg")).Should().BeNull();
    }

    [Test]
    public async Task OpenReadAsync_Throws_WhenTheBucketIsMissing()
    {
        _s3.Setup(x => x.GetObjectAsync(Bucket, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonS3Exception("no bucket") { StatusCode = HttpStatusCode.NotFound, ErrorCode = "NoSuchBucket" });

        var act = () => _service.OpenReadAsync("uploads/members/3/photo/a.jpg");

        await act.Should().ThrowAsync<AmazonS3Exception>();
    }

    [Category("NFR-S7")]
    [TestCase("")]
    [TestCase("uploads/members/../../secret.jpg")]
    [TestCase("uploads/../secure_uploads/members/3/certificate/c.pdf")]
    [TestCase("assets/images/logo.png")]
    [TestCase("other/members/3/a.jpg")]
    public async Task OpenReadAsync_RefusesPathsOutsideTheUploadTrees(string path)
    {
        (await _service.OpenReadAsync(path)).Should().BeNull();
        _s3.Verify(x => x.GetObjectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task DeleteFileAsync_RemovesThePhotoAndItsThumbnail()
    {
        await _service.DeleteFileAsync("/uploads/members/3/galleryphoto/a.jpg");

        _s3.Verify(x => x.DeleteObjectAsync(Bucket, "uploads/members/3/galleryphoto/a.jpg", It.IsAny<CancellationToken>()), Times.Once);
        _s3.Verify(x => x.DeleteObjectAsync(Bucket, "uploads/members/3/galleryphoto/thumbs/a.jpg", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Category("NFR-R4")]
    [Test]
    public async Task DeleteFileAsync_ThumbnailThatWillNotDelete_DoesNotFailThePhotoDelete()
    {
        _s3.Setup(x => x.DeleteObjectAsync(Bucket, "uploads/members/3/galleryphoto/thumbs/a.jpg", It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonS3Exception("denied") { StatusCode = HttpStatusCode.Forbidden });

        var act = () => _service.DeleteFileAsync("uploads/members/3/galleryphoto/a.jpg");

        await act.Should().NotThrowAsync();
        _s3.Verify(x => x.DeleteObjectAsync(Bucket, "uploads/members/3/galleryphoto/a.jpg", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteFileAsync_Thumbnail_DeletesOnlyItself()
    {
        await _service.DeleteFileAsync("uploads/members/3/galleryphoto/thumbs/a.jpg");

        _s3.Verify(x => x.DeleteObjectAsync(Bucket, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    // The decode-or-store-as-is fallback must not catch a failed write and send the file again.
    [Category("NFR-R4")]
    [Test]
    public async Task SaveFileAsync_PhotoWriteFails_ThrowsAfterOnePut()
    {
        _s3.Setup(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new AmazonS3Exception("down") { StatusCode = HttpStatusCode.ServiceUnavailable });

        var act = () => _service.SaveFileAsync(CreateJpeg(1600, 1200), "album.jpg", 3, Enums.FileUploadType.GalleryPhoto);

        await act.Should().ThrowAsync<AmazonS3Exception>();
        _s3.Verify(x => x.PutObjectAsync(It.IsAny<PutObjectRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task DeleteFileAsync_SecureFile_HasNoThumbnailToRemove()
    {
        await _service.DeleteFileAsync("secure_uploads/members/3/certificate/c.pdf");

        _s3.Verify(x => x.DeleteObjectAsync(Bucket, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Category("NFR-S7")]
    [Test]
    public async Task DeleteFileAsync_PathThatClimbsOut_DeletesNothing()
    {
        await _service.DeleteFileAsync("uploads/members/../../index.html");

        _s3.Verify(x => x.DeleteObjectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Test]
    public async Task IsAvailableAsync_IsTrue_WhenTheBucketCanBeListed()
    {
        _s3.Setup(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ListObjectsV2Response());

        (await _service.IsAvailableAsync()).Should().BeTrue();
    }

    [Category("NFR-R4")]
    [Test]
    public async Task IsAvailableAsync_IsFalse_WhenTheBucketCannotBeReached()
    {
        _s3.Setup(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("no route"));

        (await _service.IsAvailableAsync()).Should().BeFalse();
    }

    // An SDK timeout is a TaskCanceledException, which is still an unreachable bucket.
    [Category("NFR-R4")]
    [Test]
    public async Task IsAvailableAsync_IsFalse_WhenTheProbeTimesOut()
    {
        _s3.Setup(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new TaskCanceledException("timed out"));

        (await _service.IsAvailableAsync()).Should().BeFalse();
    }

    [Test]
    public async Task IsAvailableAsync_CallerGivingUp_IsNotReportedAsUnhealthy()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        _s3.Setup(x => x.ListObjectsV2Async(It.IsAny<ListObjectsV2Request>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException(cts.Token));

        var act = () => _service.IsAvailableAsync(cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    // Every upload is saved under uploads/members or secure_uploads/members. Anything else in the
    // tree is not the store's to serve.
    [Category("NFR-S7")]
    [TestCase("uploads/receipts/seed_reg.png")]
    [TestCase("assets/images/a.jpg")]
    public async Task OpenReadAsync_OutsideTheMemberUploadTrees_IsNotAsked(string path)
    {
        (await _service.OpenReadAsync(path)).Should().BeNull();

        _s3.Verify(x => x.GetObjectAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

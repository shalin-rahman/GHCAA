using FluentAssertions;
using GHCAA.Domain;
using GHCAA.Infrastructure.Options;
using GHCAA.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SkiaSharp;

namespace GHCAA.Tests.Services;

[TestFixture]
public class LocalFileStorageServiceTests
{
    private Mock<ILogger<LocalFileStorageService>> _mockLogger = null!;
    private Mock<IWebHostEnvironment> _mockWebHostEnvironment = null!;
    private LocalFileStorageService _service = null!;
    private string _testDirectory = null!;

    // Binds through the real ConfigurationBinder (same path services.Configure<T> uses), so a
    // value that can't convert to its property type throws here exactly like it would at
    // IOptions<T>.Value in production, instead of silently keeping the property's default.
    private static IOptions<FileStorageOptions> BuildOptions(Dictionary<string, string?> settings)
    {
        var config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        var options = config.GetSection("FileStorage").Get<FileStorageOptions>() ?? new FileStorageOptions();
        return Options.Create(options);
    }

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<LocalFileStorageService>>();

        _testDirectory = Path.Combine(Path.GetTempPath(), "GHCAATests", Guid.NewGuid().ToString());

        // A web root well away from _testDirectory so the 82.51 containment check never fires
        // for tests that aren't specifically exercising it.
        _mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
        _mockWebHostEnvironment.Setup(e => e.WebRootPath)
            .Returns(Path.Combine(Path.GetTempPath(), "GHCAATests", "wwwroot-" + Guid.NewGuid()));

        var inMemorySettings = new Dictionary<string, string?> {
            {"FileStorage:BasePhysicalPath", _testDirectory},
            {"FileStorage:UploadsRelativePath", "uploads/members"},
            {"FileStorage:MaxFileSizeBytes", "1048576"},
            {"Storage:EnableCompression", "false"}
        };

        var options = BuildOptions(inMemorySettings);

        _service = new LocalFileStorageService(options, _mockLogger.Object, _mockWebHostEnvironment.Object);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }

        // Clean up wwwroot/uploads if created during tests
        var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
        if (Directory.Exists(wwwrootPath))
        {
            try
            {
                Directory.Delete(wwwrootPath, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }

    [Test]
    public void GetRelativeFilePath_ShouldReturnCorrectPath()
    {
        // Arrange
        var memberId = 123;
        var fileName = "test.jpg";
        var uploadType = Enums.FileUploadType.Photo;

        // Act
        var result = _service.GetRelativeFilePath(memberId, uploadType, fileName);

        // Assert
        result.Should().Be("uploads/members/123/photo/photo_test.jpg");
    }

    [Test]
    public void GetRelativeFilePath_WithDifferentUploadTypes_ShouldReturnCorrectPaths()
    {
        // Arrange
        var memberId = 456;
        var fileName = "document.pdf";

        // Act & Assert
        _service.GetRelativeFilePath(memberId, Enums.FileUploadType.Certificate, fileName)
            .Should().Be("secure_uploads/members/456/certificate/certificate_document.pdf");

        _service.GetRelativeFilePath(memberId, Enums.FileUploadType.PaymentProof, fileName)
            .Should().Be("secure_uploads/members/456/paymentproof/paymentproof_document.pdf");
    }

    [Test]
    public void GetRelativeFilePath_WithPathInFileName_ShouldExtractOnlyFileName()
    {
        // Arrange
        var memberId = 789;
        // Use a cross-platform path that works on both Windows and Linux
        var fileName = Path.Combine("Users", "Test", "file.jpg");
        var uploadType = Enums.FileUploadType.Photo;

        // Act
        var result = _service.GetRelativeFilePath(memberId, uploadType, fileName);

        // Assert
        // Assert
        result.Should().Be("uploads/members/789/photo/photo_file.jpg");
    }

    [Test]
    public async Task SaveFileAsync_WithValidFile_ShouldSaveFile()
    {
        // Arrange
        var memberId = 1;
        var fileName = "test.jpg";
        var fileContent = new byte[] { 1, 2, 3, 4, 5 };
        var stream = new MemoryStream(fileContent);
        var uploadType = Enums.FileUploadType.Photo;

        // Act
        var result = await _service.SaveFileAsync(stream, fileName, memberId, uploadType);

        // Assert
        result.Should().NotBeNullOrEmpty();
        result.Should().StartWith("uploads/members/1/photo/");
        result.Should().EndWith("_test.jpg");

        // Verify file was actually created (cross-platform path)
        var fullPath = Path.Combine(_testDirectory, result.Replace("/", Path.DirectorySeparatorChar.ToString()));
        File.Exists(fullPath).Should().BeTrue();
    }

    [Test]
    public async Task SaveFileAsync_WithNullStream_ShouldThrowException()
    {
        // Arrange
        var memberId = 1;
        var fileName = "test.jpg";
        var uploadType = Enums.FileUploadType.Photo;

        // Act & Assert
        var act = async () => await _service.SaveFileAsync(null!, fileName, memberId, uploadType);
        await act.Should().ThrowAsync<ArgumentNullException>();
    }

    [Test]
    public async Task SaveFileAsync_WithFileTooLarge_ShouldThrowException()
    {
        // Arrange
        var memberId = 1;
        var fileName = "large.jpg";
        var fileContent = new byte[2 * 1024 * 1024]; // 2MB, exceeds default 1MB limit
        var stream = new MemoryStream(fileContent);
        var uploadType = Enums.FileUploadType.Photo;

        // Act & Assert
        var act = async () => await _service.SaveFileAsync(stream, fileName, memberId, uploadType);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("File exceeds maximum size * bytes.");
    }

    [Test]
    public async Task SaveFileAsync_ShouldCreateDirectoryIfNotExists()
    {
        // Arrange
        var memberId = 999;
        var fileName = "test.jpg";
        var fileContent = new byte[] { 1, 2, 3 };
        var stream = new MemoryStream(fileContent);
        var uploadType = Enums.FileUploadType.Photo;

        // Act
        var result = await _service.SaveFileAsync(stream, fileName, memberId, uploadType);

        // Assert
        var directoryPath = Path.Combine(_testDirectory, "uploads", "members", "999", "photo");
        Directory.Exists(directoryPath).Should().BeTrue();
    }

    [Test]
    public async Task SaveFileAsync_ShouldGenerateUniqueFileName()
    {
        // Arrange
        var memberId = 1;
        var fileName = "test.jpg";
        var fileContent = new byte[] { 1, 2, 3 };
        var uploadType = Enums.FileUploadType.Photo;

        // Act
        var result1 = await _service.SaveFileAsync(new MemoryStream(fileContent), fileName, memberId, uploadType);
        var result2 = await _service.SaveFileAsync(new MemoryStream(fileContent), fileName, memberId, uploadType);

        // Assert
        result1.Should().NotBe(result2);
    }

    [Test]
    public async Task DeleteFileAsync_WithExistingFile_ShouldDeleteFile()
    {
        // Arrange
        var memberId = 1;
        var fileName = "test.jpg";
        var fileContent = new byte[] { 1, 2, 3 };
        var stream = new MemoryStream(fileContent);
        var uploadType = Enums.FileUploadType.Photo;

        var relativePath = await _service.SaveFileAsync(stream, fileName, memberId, uploadType);
        var fullPath = Path.Combine(_testDirectory, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
        File.Exists(fullPath).Should().BeTrue();

        // Act
        await _service.DeleteFileAsync(relativePath);

        // Assert
        File.Exists(fullPath).Should().BeFalse();
    }

    [Test]
    public async Task DeleteFileAsync_WithNonExistentFile_ShouldNotThrowException()
    {
        // Arrange
        var relativePath = "uploads/members/999/photo/nonexistent.jpg";

        // Act & Assert
        var act = async () => await _service.DeleteFileAsync(relativePath);
        await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task DeleteFileAsync_WithLeadingSlash_ShouldHandleCorrectly()
    {
        // Arrange
        var memberId = 1;
        var fileName = "test.jpg";
        var fileContent = new byte[] { 1, 2, 3 };
        var stream = new MemoryStream(fileContent);
        var uploadType = Enums.FileUploadType.Photo;

        var relativePath = await _service.SaveFileAsync(stream, fileName, memberId, uploadType);
        var pathWithLeadingSlash = "/" + relativePath;

        // Act
        await _service.DeleteFileAsync(pathWithLeadingSlash);

        // Assert
        var fullPath = Path.Combine(_testDirectory, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
        File.Exists(fullPath).Should().BeFalse();
    }

    [Test]
    public void Constructor_WithCustomMaxFileSize_ShouldUseCustomValue()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?> {
            {"FileStorage:MaxFileSizeBytes", "500000"}
        };
        var service = new LocalFileStorageService(BuildOptions(inMemorySettings), _mockLogger.Object, _mockWebHostEnvironment.Object);
        var fileContent = new byte[600000]; // 600KB
        var stream = new MemoryStream(fileContent);

        // Act & Assert
        var act = async () => await service.SaveFileAsync(stream, "test.jpg", 1, Enums.FileUploadType.Photo);
        act.Should().ThrowAsync<InvalidOperationException>();
    }

    // Left half red, right half blue, with an EXIF tag saying the camera was turned. The tag goes
    // in an APP1 segment straight after the JPEG start marker.
    // A grayscale copy keeps the halves apart: red comes out lighter than blue.
    private static MemoryStream CreateSidewaysJpeg(int width, int height, byte orientation, bool grayscale = false)
    {
        using var colour = new SKBitmap(width, height);
        using (var canvas = new SKCanvas(colour))
        using (var red = new SKPaint { Color = SKColors.Red })
        {
            canvas.Clear(SKColors.Blue);
            canvas.DrawRect(0, 0, width / 2f, height, red);
        }
        using var bitmap = grayscale ? colour.Copy(SKColorType.Gray8) : colour.Copy();
        using var skImage = SKImage.FromBitmap(bitmap);
        using var encoded = skImage.Encode(SKEncodedImageFormat.Jpeg, 90);
        var jpeg = encoded.ToArray();
        byte[] exif =
        [
            0xFF, 0xE1, 0x00, 0x22, 0x45, 0x78, 0x69, 0x66, 0x00, 0x00,
            0x49, 0x49, 0x2A, 0x00, 0x08, 0x00, 0x00, 0x00,
            0x01, 0x00,
            0x12, 0x01, 0x03, 0x00, 0x01, 0x00, 0x00, 0x00, orientation, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
        ];
        return new MemoryStream([.. jpeg[..2], .. exif, .. jpeg[2..]]);
    }

    // Orientation 6 means turn clockwise to view, so the red left half ends up on top.
    private static void ShouldBeUprightPortrait(string physicalPath)
    {
        using var image = SKBitmap.Decode(physicalPath);
        image.Height.Should().BeGreaterThan(image.Width);
        var top = image.GetPixel(image.Width / 2, image.Height / 8);
        var bottom = image.GetPixel(image.Width / 2, image.Height * 7 / 8);
        top.Red.Should().BeGreaterThan(top.Blue);
        bottom.Blue.Should().BeGreaterThan(bottom.Red);
    }

    [Category("NFR-P4")]
    [Test]
    public async Task SaveFileAsync_SidewaysPhoto_CompressedCopyAndThumbnailAreUpright()
    {
        var relativePath = await _service.SaveFileAsync(CreateSidewaysJpeg(1600, 1200, 6), "phone.jpg", 1, Enums.FileUploadType.GalleryPhoto);

        ShouldBeUprightPortrait(Path.Combine(_testDirectory, relativePath));
        ShouldBeUprightPortrait(Path.Combine(_testDirectory, _service.GetThumbnailPath(relativePath)!));
    }

    // A grayscale JPEG decodes to a color type a canvas cannot draw into.
    [Category("NFR-P4")]
    [Test]
    public async Task SaveFileAsync_SidewaysGrayscalePhoto_ThumbnailIsUpright()
    {
        var service = ServiceWithCompressionOff();

        var relativePath = await service.SaveFileAsync(CreateSidewaysJpeg(1600, 1200, 6, grayscale: true), "grey.jpg", 1, Enums.FileUploadType.GalleryPhoto);

        using var image = SKBitmap.Decode(Path.Combine(_testDirectory, service.GetThumbnailPath(relativePath)!));
        image.Height.Should().BeGreaterThan(image.Width);
        image.GetPixel(image.Width / 2, image.Height / 8).Red
            .Should().BeGreaterThan(image.GetPixel(image.Width / 2, image.Height * 7 / 8).Red);
    }

    // Skia cannot write a GIF, so no thumbnail beats JPEG bytes under a .gif name.
    [Category("NFR-P4")]
    [Test]
    public async Task SaveFileAsync_GalleryPhotoWithAnExtensionSkiaCannotWrite_HasNoThumbnail()
    {
        var service = ServiceWithCompressionOff();

        var relativePath = await service.SaveFileAsync(CreateJpeg(600, 600), "odd.gif", 1, Enums.FileUploadType.GalleryPhoto);

        File.Exists(Path.Combine(_testDirectory, relativePath)).Should().BeTrue();
        service.GetThumbnailPath(relativePath).Should().BeNull();
    }

    [Category("NFR-P4")]
    [Test]
    public async Task SaveFileAsync_SidewaysPhoto_CompressionOff_ThumbnailIsUpright()
    {
        var service = ServiceWithCompressionOff();

        var relativePath = await service.SaveFileAsync(CreateSidewaysJpeg(1600, 1200, 6), "phone.jpg", 1, Enums.FileUploadType.GalleryPhoto);

        ShouldBeUprightPortrait(Path.Combine(_testDirectory, service.GetThumbnailPath(relativePath)!));
    }

    [Category("NFR-S7")]
    [Test]
    public async Task DeleteFileAsync_PathThatClimbsOutOfTheUploads_DeletesNothing()
    {
        Directory.CreateDirectory(_testDirectory);
        var outside = Path.Combine(_testDirectory, "outside.txt");
        await File.WriteAllTextAsync(outside, "keep");

        await _service.DeleteFileAsync("uploads/members/../../outside.txt");

        File.Exists(outside).Should().BeTrue();
    }

    private static MemoryStream CreateJpeg(int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        using var skImage = SKImage.FromBitmap(bitmap);
        using var data = skImage.Encode(SKEncodedImageFormat.Jpeg, 90);
        var stream = new MemoryStream(data.ToArray());
        return stream;
    }

    [Test]
    public async Task SaveFileAsync_WithOversizedGalleryPhoto_ShouldResizeToMaxDimension()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?> {
            {"FileStorage:BasePhysicalPath", _testDirectory},
            {"FileStorage:ImageCompression:MaxDimensionPx", "800"}
        };
        var service = new LocalFileStorageService(BuildOptions(inMemorySettings), _mockLogger.Object, _mockWebHostEnvironment.Object);
        var stream = CreateJpeg(3000, 2000); // 3:2 landscape, both dims exceed the 800px cap

        // Act
        var relativePath = await service.SaveFileAsync(stream, "album.jpg", 1, Enums.FileUploadType.GalleryPhoto);

        // Assert
        Path.GetFileName(relativePath).Should().StartWith("galleryphoto_");
        var fullPath = Path.Combine(_testDirectory, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
        using var saved = SKBitmap.Decode(fullPath);
        saved.Width.Should().Be(800);
        saved.Height.Should().Be(533); // 2000/3000 * 800, rounded to the nearest pixel
    }

    [Test]
    public async Task SaveFileAsync_WithConfiguredImageCap_ShouldWriteWithinCap()
    {
        var inMemorySettings = new Dictionary<string, string?> {
            {"FileStorage:BasePhysicalPath", _testDirectory},
            {"FileStorage:ImageCompression:TargetSizeKB", "8"},
            {"FileStorage:ImageCompression:MaxDimensionPx", "800"},
            {"FileStorage:ImageCompression:FallbackQuality", "20"}
        };
        var service = new LocalFileStorageService(BuildOptions(inMemorySettings), _mockLogger.Object, _mockWebHostEnvironment.Object);
        var stream = CreateJpeg(800, 800);

        var relativePath = await service.SaveFileAsync(stream, "photo.jpg", 1, Enums.FileUploadType.Photo);

        var fullPath = Path.Combine(_testDirectory, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
        new FileInfo(fullPath).Length.Should().BeLessThanOrEqualTo(8 * 1024);
    }

    [Test]
    public async Task SaveFileAsync_WithSmallGalleryPhoto_ShouldNotUpscale()
    {
        // Arrange
        var stream = CreateJpeg(200, 150); // well under the default 1920px cap

        // Act
        var relativePath = await _service.SaveFileAsync(stream, "album.jpg", 1, Enums.FileUploadType.GalleryPhoto);

        // Assert
        var fullPath = Path.Combine(_testDirectory, relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
        using var saved = SKBitmap.Decode(fullPath);
        saved.Width.Should().Be(200);
        saved.Height.Should().Be(150);
    }

    [Category("NFR-P4")]
    [Test]
    public async Task SaveFileAsync_WithGalleryPhoto_ShouldWriteThumbnailBesideIt()
    {
        var stream = CreateJpeg(1600, 1200);

        var relativePath = await _service.SaveFileAsync(stream, "album.jpg", 1, Enums.FileUploadType.GalleryPhoto);
        var thumbPath = _service.GetThumbnailPath(relativePath);

        thumbPath.Should().Be(relativePath.Replace("/galleryphoto/", "/galleryphoto/thumbs/"));
        using var thumb = SKBitmap.Decode(Path.Combine(_testDirectory, thumbPath!));
        thumb.Width.Should().Be(Constants.Defaults.GalleryThumbnailMaxPx);
        thumb.Height.Should().Be(360);
    }

    [Category("NFR-P4")]
    [Test]
    public async Task GetThumbnailPath_KeepsLeadingSlashOfInput()
    {
        var relativePath = await _service.SaveFileAsync(CreateJpeg(600, 600), "album.jpg", 1, Enums.FileUploadType.GalleryPhoto);

        _service.GetThumbnailPath("/" + relativePath).Should().StartWith("/uploads/members/1/galleryphoto/thumbs/");
    }

    [Category("NFR-P4")]
    [TestCase(Enums.FileUploadType.Photo)]
    [TestCase(Enums.FileUploadType.NewsImage)]
    public async Task SaveFileAsync_WithOtherImageTypes_ShouldNotWriteThumbnail(Enums.FileUploadType type)
    {
        var relativePath = await _service.SaveFileAsync(CreateJpeg(1600, 1200), "pic.jpg", 1, type);

        _service.GetThumbnailPath(relativePath).Should().BeNull();
    }

    [TestCase("")]
    [TestCase("uploads/members/1/galleryphoto/missing.jpg")]
    [TestCase("uploads/members/../../secret.jpg")]
    [TestCase("secure_uploads/1/certificate/cert.jpg")]
    [TestCase("/assets/gallery/iftar/01.jpg")]
    public void GetThumbnailPath_ReturnsNull_ForMissingOrOutsideUploads(string path)
    {
        _service.GetThumbnailPath(path).Should().BeNull();
    }

    private LocalFileStorageService ServiceWithCompressionOff() => new(
        BuildOptions(new Dictionary<string, string?> {
            {"FileStorage:BasePhysicalPath", _testDirectory},
            {"FileStorage:ImageCompression:Enabled", "false"}
        }),
        _mockLogger.Object, _mockWebHostEnvironment.Object);

    [Category("NFR-P4")]
    [Test]
    public async Task SaveFileAsync_WithCompressionOff_GalleryPhotoStillGetsThumbnail()
    {
        var service = ServiceWithCompressionOff();

        var relativePath = await service.SaveFileAsync(CreateJpeg(1600, 1200), "album.jpg", 1, Enums.FileUploadType.GalleryPhoto);

        var thumbPath = service.GetThumbnailPath(relativePath);
        thumbPath.Should().NotBeNull();
        using var thumb = SKBitmap.Decode(Path.Combine(_testDirectory, thumbPath!));
        thumb.Width.Should().Be(Constants.Defaults.GalleryThumbnailMaxPx);
    }

    [Category("NFR-P4")]
    [Test]
    public async Task SaveFileAsync_PngGalleryPhoto_ThumbnailIsPngToo()
    {
        var service = ServiceWithCompressionOff();
        using var bitmap = new SKBitmap(1000, 800);
        using var image = SKImage.FromBitmap(bitmap);
        using var png = image.Encode(SKEncodedImageFormat.Png, 100);

        var relativePath = await service.SaveFileAsync(new MemoryStream(png.ToArray()), "album.png", 1, Enums.FileUploadType.GalleryPhoto);

        using var codec = SKCodec.Create(Path.Combine(_testDirectory, service.GetThumbnailPath(relativePath)!));
        codec.EncodedFormat.Should().Be(SKEncodedImageFormat.Png);
    }

    [Category("NFR-P4")]
    [Test]
    public async Task SaveFileAsync_GalleryPhotoThatIsNotAnImage_SavesWithoutThumbnail()
    {
        var service = ServiceWithCompressionOff();

        var relativePath = await service.SaveFileAsync(new MemoryStream(new byte[] { 1, 2, 3 }), "album.jpg", 1, Enums.FileUploadType.GalleryPhoto);

        File.Exists(Path.Combine(_testDirectory, relativePath)).Should().BeTrue();
        service.GetThumbnailPath(relativePath).Should().BeNull();
    }

    // 144 MP of black in an 18 KB file. Decoding it would take hundreds of megabytes.
    [Category("NFR-P4")]
    [Test]
    public async Task SaveFileAsync_GalleryPhotoPastThePixelCap_SavesWithoutThumbnail()
    {
        var service = ServiceWithCompressionOff();

        var relativePath = await service.SaveFileAsync(CreateBlankPng(12000, 12000), "huge.png", 1, Enums.FileUploadType.GalleryPhoto);

        File.Exists(Path.Combine(_testDirectory, relativePath)).Should().BeTrue();
        service.GetThumbnailPath(relativePath).Should().BeNull();
    }

    // A 1-bit grayscale PNG built by hand, since Skia would need the full bitmap in memory to write one.
    private static MemoryStream CreateBlankPng(int width, int height)
    {
        var rows = new byte[((width + 7) / 8 + 1) * height];
        using var deflated = new MemoryStream();
        using (var zlib = new System.IO.Compression.ZLibStream(deflated, System.IO.Compression.CompressionLevel.Optimal, leaveOpen: true))
            zlib.Write(rows);

        var header = new byte[13];
        System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(header, width);
        System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(header.AsSpan(4), height);
        header[8] = 1; // bit depth; color type 0 (grayscale) and the rest stay 0

        var png = new MemoryStream();
        png.Write(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 });
        WriteChunk(png, "IHDR", header);
        WriteChunk(png, "IDAT", deflated.ToArray());
        WriteChunk(png, "IEND", []);
        png.Position = 0;
        return png;
    }

    private static void WriteChunk(Stream png, string type, byte[] body)
    {
        var typeAndBody = System.Text.Encoding.ASCII.GetBytes(type).Concat(body).ToArray();
        var length = new byte[4];
        System.Buffers.Binary.BinaryPrimitives.WriteInt32BigEndian(length, body.Length);
        png.Write(length);
        png.Write(typeAndBody);

        var crc = 0xFFFFFFFFu;
        foreach (var b in typeAndBody)
        {
            crc ^= b;
            for (var i = 0; i < 8; i++) crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320u : crc >> 1;
        }
        var crcBytes = new byte[4];
        System.Buffers.Binary.BinaryPrimitives.WriteUInt32BigEndian(crcBytes, ~crc);
        png.Write(crcBytes);
    }

    [Category("NFR-P4")]
    [Test]
    public async Task DeleteFileAsync_GalleryPhoto_RemovesItsThumbnail()
    {
        var relativePath = await _service.SaveFileAsync(CreateJpeg(1600, 1200), "album.jpg", 1, Enums.FileUploadType.GalleryPhoto);
        var thumbPath = Path.Combine(_testDirectory, _service.GetThumbnailPath(relativePath)!);
        File.Exists(thumbPath).Should().BeTrue();

        await _service.DeleteFileAsync(relativePath);

        File.Exists(thumbPath).Should().BeFalse();
    }

    [Test]
    public void Constructor_WithInvalidMaxFileSize_ShouldFailFastInsteadOfSilentlyDefaulting()
    {
        // 82.17: MaxFileSizeBytes now binds through FileStorageOptions, so a value that can't
        // convert to long surfaces as a binding error instead of the old int/long.TryParse
        // fallback silently keeping the default — that silent-default behavior is exactly what
        // 82.17 was written to remove.
        var inMemorySettings = new Dictionary<string, string?> {
            {"FileStorage:MaxFileSizeBytes", "invalid"}
        };

        var act = () => new LocalFileStorageService(BuildOptions(inMemorySettings), _mockLogger.Object, _mockWebHostEnvironment.Object);

        act.Should().Throw<InvalidOperationException>();
    }

    // 82.51: BasePhysicalPath is the single knob both roots are computed from, so a future value
    // that puts the secure root inside the web root must fail construction instead of silently
    // serving certificates/payment proofs/signatures through the unauthenticated static-files route.
    [Test]
    public void Constructor_ThrowsWhenSecureRootResolvesInsideWebRoot()
    {
        // Arrange: BasePhysicalPath drives both roots, so this collides _secureRoot with WebRootPath.
        var inMemorySettings = new Dictionary<string, string?> {
            {"FileStorage:BasePhysicalPath", _testDirectory},
            {"FileStorage:SecureRelativePath", ""}
        };
        var collidingWebHost = new Mock<IWebHostEnvironment>();
        collidingWebHost.Setup(e => e.WebRootPath).Returns(_testDirectory);

        // Act
        var act = () => new LocalFileStorageService(BuildOptions(inMemorySettings), _mockLogger.Object, collidingWebHost.Object);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*secure uploads root*web root*");
    }
}

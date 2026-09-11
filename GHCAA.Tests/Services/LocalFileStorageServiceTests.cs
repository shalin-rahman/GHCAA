using FluentAssertions;
using GHCAA.Domain;
using GHCAA.Infrastructure.Options;
using GHCAA.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;

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

    private static MemoryStream CreateJpeg(int width, int height)
    {
        using var image = new Image<Rgba32>(width, height);
        var stream = new MemoryStream();
        image.SaveAsJpeg(stream, new JpegEncoder { Quality = 90 });
        stream.Position = 0;
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
        using var saved = await Image.LoadAsync(fullPath);
        saved.Width.Should().Be(800);
        saved.Height.Should().Be(533); // 2000/3000 * 800, rounded by ImageSharp's Max resize mode
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
        using var saved = await Image.LoadAsync(fullPath);
        saved.Width.Should().Be(200);
        saved.Height.Should().Be(150);
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

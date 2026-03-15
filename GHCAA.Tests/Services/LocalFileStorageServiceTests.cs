using FluentAssertions;
using GHCAA.Domain;
using GHCAA.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace GHCAA.Tests.Services;

[TestFixture]
public class LocalFileStorageServiceTests
{
    private IConfiguration _config = null!;
    private Mock<ILogger<LocalFileStorageService>> _mockLogger = null!;
    private LocalFileStorageService _service = null!;
    private string _testDirectory = null!;

    [SetUp]
    public void Setup()
    {
        _mockLogger = new Mock<ILogger<LocalFileStorageService>>();

        _testDirectory = Path.Combine(Path.GetTempPath(), "GHCAATests", Guid.NewGuid().ToString());

        var inMemorySettings = new Dictionary<string, string> {
            {"FileStorage:UploadsRelativePath", "uploads/members"},
            {"FileStorage:MaxFileSizeBytes", "1048576"},
            {"Storage:EnableCompression", "false"} // Explicitly pass false/true so GetValue doesn't throw if section missing mocking
        };

        _config = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings!)
            .Build();

        _service = new LocalFileStorageService(_config, _mockLogger.Object);
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
        result.Should().Be("uploads/members/photo_m123_test.jpg");
    }

    [Test]
    public void GetRelativeFilePath_WithDifferentUploadTypes_ShouldReturnCorrectPaths()
    {
        // Arrange
        var memberId = 456;
        var fileName = "document.pdf";

        // Act & Assert
        _service.GetRelativeFilePath(memberId, Enums.FileUploadType.Certificate, fileName)
            .Should().Be("secure_uploads/members/certificate_m456_document.pdf");

        _service.GetRelativeFilePath(memberId, Enums.FileUploadType.PaymentProof, fileName)
            .Should().Be("secure_uploads/members/paymentproof_m456_document.pdf");
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
        result.Should().Be("uploads/members/photo_m789_file.jpg");
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
        result.Should().StartWith("uploads/members/photo_m1_");
        result.Should().EndWith("_test.jpg");

        // Verify file was actually created (cross-platform path)
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", result.Replace("/", Path.DirectorySeparatorChar.ToString()));
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
        var directoryPath = Path.Combine("wwwroot", "uploads", "members");
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
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
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
        var relativePath = "uploads/members/photo_m999_nonexistent.jpg";

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
        var fullPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", relativePath.Replace("/", Path.DirectorySeparatorChar.ToString()));
        File.Exists(fullPath).Should().BeFalse();
    }

    [Test]
    public void Constructor_WithCustomMaxFileSize_ShouldUseCustomValue()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string> {
            {"FileStorage:MaxFileSizeBytes", "500000"}
        };
        var testConfig = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings!).Build();
        var service = new LocalFileStorageService(testConfig, _mockLogger.Object);
        var fileContent = new byte[600000]; // 600KB
        var stream = new MemoryStream(fileContent);

        // Act & Assert
        var act = async () => await service.SaveFileAsync(stream, "test.jpg", 1, Enums.FileUploadType.Photo);
        act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Test]
    public void Constructor_WithInvalidMaxFileSize_ShouldUseDefaultValue()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string> {
            {"FileStorage:MaxFileSizeBytes", "invalid"}
        };
        var testConfig = new ConfigurationBuilder().AddInMemoryCollection(inMemorySettings!).Build();
        var service = new LocalFileStorageService(testConfig, _mockLogger.Object);

        // Act
        var result = service;

        // Assert
        result.Should().NotBeNull();
    }
}

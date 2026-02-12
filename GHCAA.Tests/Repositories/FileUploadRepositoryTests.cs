using FluentAssertions;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Tests.Repositories;

[TestFixture]
public class FileUploadRepositoryTests
{
    private ApplicationDbContext _context = null!;
    private FileUploadRepository _repository = null!;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ApplicationDbContext(options);
        _repository = new FileUploadRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task AddAsync_ShouldAddFileUpload()
    {
        // Arrange
        var fileUpload = new FileUpload
        {
            MemberId = 1,
            UploadType = Enums.FileUploadType.Photo,
            FileName = "test.jpg",
            FilePath = "uploads/members/1/photo/test.jpg",
            SizeBytes = 1024
        };

        // Act
        await _repository.AddAsync(fileUpload);

        // Assert
        var result = await _context.FileUploads.FindAsync(fileUpload.Id);
        result.Should().NotBeNull();
        result!.FileName.Should().Be("test.jpg");
        result.MemberId.Should().Be(1);
        result.UploadType.Should().Be(Enums.FileUploadType.Photo);
    }

    [Test]
    public async Task GetByIdAsync_WithValidId_ShouldReturnFileUpload()
    {
        // Arrange
        var fileUpload = new FileUpload
        {
            MemberId = 1,
            UploadType = Enums.FileUploadType.Certificate,
            FileName = "cert.pdf",
            FilePath = "uploads/members/1/certificate/cert.pdf",
            SizeBytes = 2048
        };
        await _context.FileUploads.AddAsync(fileUpload);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByIdAsync(fileUpload.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(fileUpload.Id);
        result.FileName.Should().Be("cert.pdf");
    }

    [Test]
    public async Task GetByIdAsync_WithInvalidId_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task GetByMemberAsync_ShouldReturnAllFilesForMember()
    {
        // Arrange
        var memberId = 1;
        var file1 = new FileUpload
        {
            MemberId = memberId,
            UploadType = Enums.FileUploadType.Photo,
            FileName = "photo.jpg",
            FilePath = "uploads/members/1/photo/photo.jpg",
            SizeBytes = 1024
        };
        var file2 = new FileUpload
        {
            MemberId = memberId,
            UploadType = Enums.FileUploadType.Certificate,
            FileName = "cert.pdf",
            FilePath = "uploads/members/1/certificate/cert.pdf",
            SizeBytes = 2048
        };
        var file3 = new FileUpload
        {
            MemberId = 2, // Different member
            UploadType = Enums.FileUploadType.Photo,
            FileName = "other.jpg",
            FilePath = "uploads/members/2/photo/other.jpg",
            SizeBytes = 512
        };

        await _context.FileUploads.AddRangeAsync(file1, file2, file3);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByMemberAsync(memberId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(f => f.FileName == "photo.jpg");
        result.Should().Contain(f => f.FileName == "cert.pdf");
        result.Should().NotContain(f => f.FileName == "other.jpg");
    }

    [Test]
    public async Task GetByMemberAsync_WithNoFiles_ShouldReturnEmptyList()
    {
        // Act
        var result = await _repository.GetByMemberAsync(999);

        // Assert
        result.Should().BeEmpty();
    }

    [Test]
    public async Task UpdateAsync_ShouldUpdateFileUpload()
    {
        // Arrange
        var fileUpload = new FileUpload
        {
            MemberId = 1,
            UploadType = Enums.FileUploadType.Photo,
            FileName = "old.jpg",
            FilePath = "uploads/members/1/photo/old.jpg",
            SizeBytes = 1024
        };
        await _context.FileUploads.AddAsync(fileUpload);
        await _context.SaveChangesAsync();

        // Act
        fileUpload.FileName = "new.jpg";
        fileUpload.FilePath = "uploads/members/1/photo/new.jpg";
        fileUpload.SizeBytes = 2048;
        await _repository.UpdateAsync(fileUpload);

        // Assert
        var result = await _context.FileUploads.FindAsync(fileUpload.Id);
        result.Should().NotBeNull();
        result!.FileName.Should().Be("new.jpg");
        result.FilePath.Should().Be("uploads/members/1/photo/new.jpg");
        result.SizeBytes.Should().Be(2048);
    }

    [Test]
    public async Task DeleteAsync_ShouldRemoveFileUpload()
    {
        // Arrange
        var fileUpload = new FileUpload
        {
            MemberId = 1,
            UploadType = Enums.FileUploadType.PaymentProof,
            FileName = "payment.jpg",
            FilePath = "uploads/members/1/paymentproof/payment.jpg",
            SizeBytes = 1024
        };
        await _context.FileUploads.AddAsync(fileUpload);
        await _context.SaveChangesAsync();
        var id = fileUpload.Id;

        // Act
        await _repository.DeleteAsync(fileUpload);

        // Assert
        var result = await _context.FileUploads.FindAsync(id);
        result.Should().BeNull();
    }

    [Test]
    public async Task AddAsync_WithMultipleFiles_ShouldAddAll()
    {
        // Arrange
        var file1 = new FileUpload
        {
            MemberId = 1,
            UploadType = Enums.FileUploadType.Photo,
            FileName = "photo.jpg",
            FilePath = "uploads/members/1/photo/photo.jpg",
            SizeBytes = 1024
        };
        var file2 = new FileUpload
        {
            MemberId = 1,
            UploadType = Enums.FileUploadType.Certificate,
            FileName = "cert.pdf",
            FilePath = "uploads/members/1/certificate/cert.pdf",
            SizeBytes = 2048
        };

        // Act
        await _repository.AddAsync(file1);
        await _repository.AddAsync(file2);

        // Assert
        var allFiles = await _context.FileUploads.ToListAsync();
        allFiles.Should().HaveCount(2);
    }
}

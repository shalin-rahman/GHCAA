using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static GHCAA.Domain.Enums;

namespace GHCAA.Tests.Services;

[TestFixture]
public class EventServiceTests
{
    private ApplicationDbContext _context = null!;
    private Microsoft.Data.Sqlite.SqliteConnection _connection = null!;
    private EventService _service = null!;
    private Mock<ICommunicationService> _communicationMock = null!;
    private Mock<IFileStorageService> _fileStorageMock = null!;

    [SetUp]
    public void Setup()
    {
        _connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();

        _communicationMock = new Mock<ICommunicationService>();
        _fileStorageMock = new Mock<IFileStorageService>();
        _service = new EventService(_context, _communicationMock.Object, _fileStorageMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        _connection.Close();
    }

    [Test]
    public async Task CreateEventAsync_ShouldAddEvent()
    {
        // Arrange
        var dto = new CreateEventDto
        {
            Title = "Test Event",
            Description = "Test Description",
            Date = DateTime.UtcNow.AddDays(10),
            Location = "Test Location",
            RegistrationFee = 100,
            IsActive = true,
            RegistrationDeadline = DateTime.UtcNow.AddDays(5),
            AdminNote = "Staff only"
        };

        // Act
        var result = await _service.CreateEventAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Test Event");
        result.AdminNote.Should().Be("Staff only");
        result.RegistrationDeadline.Should().NotBeNull();
        _context.AlumniEvents.Count().Should().Be(1);
    }

    [Test]
    public async Task RegisterForEventAsync_ShouldCreateRegistration()
    {
        // Arrange
        var ev = new AlumniEvent { Title = "Event 1", Description = "D", Date = DateTime.UtcNow, Location = "L" };
        _context.AlumniEvents.Add(ev);
        await _context.SaveChangesAsync();

        var dto = new RegisterForEventDto
        {
            EventId = ev.Id,
            PaymentReference = "REF123"
        };

        // Act
        var result = await _service.RegisterForEventAsync(dto, 1, null);

        // Assert
        result.Should().NotBeNull();
        result.EventId.Should().Be(ev.Id);
        result.PaymentReference.Should().Be("REF123");
        result.Status.Should().Be(EventRegistrationStatus.Pending);
    }

    [Test]
    public async Task ApproveRegistrationAsync_ShouldUpdateStatusAndSendEmail()
    {
        // Arrange
        var member = new Member 
        { 
            FullName = "Test Member", Email = "test@example.com", 
            NID = "1", FatherName = "F", MotherName = "M", MobileNo = "01", PresentAddress = "A", PermanentAddress = "A", 
            EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0", SubjectGroup = "S", ProfessionalSector = "P", Designation = "D" 
        };
        var ev = new AlumniEvent { Title = "Grand Reunion", Description = "D", Date = DateTime.UtcNow, Location = "Campus" };
        
        _context.Members.Add(member);
        _context.AlumniEvents.Add(ev);
        await _context.SaveChangesAsync();
        
        var reg = new EventRegistration
        {
            EventId = ev.Id,
            MemberId = member.Id,
            PaymentReference = "REF999",
            Status = EventRegistrationStatus.Pending
        };
        _context.EventRegistrations.Add(reg);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.ApproveRegistrationAsync(reg.Id, 99, true);

        // Assert
        result.Should().BeTrue();
        var updated = await _context.EventRegistrations.FindAsync(reg.Id);
        updated!.Status.Should().Be(EventRegistrationStatus.Approved);
        updated.ApprovedByAdminId.Should().Be(99);

        _communicationMock.Verify(c => c.SendIndividualEmailAsync(
            member.Id, 
            "EVENT_REGISTRATION_CONFIRMATION", 
            It.IsAny<Dictionary<string, string>>(), 
            It.IsAny<CancellationToken>()), 
            Times.Once);
    }
    [Test]
    public async Task ApproveRegistrationAsync_ShouldReturnFalse_WhenRegistrationNotFound()
    {
        // Act
        var result = await _service.ApproveRegistrationAsync(999, 1, true);

        // Assert
        result.Should().BeFalse();
    }

    [Test]
    public async Task RegisterForEventAsync_ShouldIncludeReceiptPath_WhenFileProvided()
    {
        // Arrange
        var ev = new AlumniEvent { Title = "E", Description = "D", Date = DateTime.UtcNow, Location = "L" };
        _context.AlumniEvents.Add(ev);
        await _context.SaveChangesAsync();

        var receiptDto = new UploadedFileDto { FileName = "r.jpg", Content = new MemoryStream() };
        _fileStorageMock.Setup(f => f.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<FileUploadType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("/uploads/r.jpg");

        var dto = new RegisterForEventDto { EventId = ev.Id, PaymentReference = "P" };

        // Act
        var result = await _service.RegisterForEventAsync(dto, 1, receiptDto);

        // Assert
        result.ReceiptPath.Should().Be("/uploads/r.jpg");
        _fileStorageMock.Verify(f => f.SaveFileAsync(It.IsAny<Stream>(), "r.jpg", 1, FileUploadType.PaymentProof, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task RegisterForEventAsync_ShouldThrowException_WhenDeadlinePassed()
    {
        // Arrange
        var ev = new AlumniEvent 
        { 
            Title = "Past Event", Description = "D", Date = DateTime.UtcNow, 
            Location = "L", RegistrationDeadline = DateTime.UtcNow.AddHours(-1) 
        };
        _context.AlumniEvents.Add(ev);
        await _context.SaveChangesAsync();

        var dto = new RegisterForEventDto { EventId = ev.Id, PaymentReference = "P" };

        // Act
        Func<Task> act = async () => await _service.RegisterForEventAsync(dto, 1, null);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Registration is closed*");
    }

    [Test]
    public async Task UpdateEventAsync_ShouldUpdateAllFields()
    {
        // Arrange
        var ev = new AlumniEvent { Title = "Old Title", Description = "D", Date = DateTime.UtcNow, Location = "L" };
        _context.AlumniEvents.Add(ev);
        await _context.SaveChangesAsync();

        var dto = new UpdateEventDto
        {
            Id = ev.Id,
            Title = "New Title",
            Description = "New D",
            Date = DateTime.UtcNow.AddDays(1),
            Location = "New Loc",
            AdminNote = "Important Update"
        };

        // Act
        var result = await _service.UpdateEventAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("New Title");
        result.AdminNote.Should().Be("Important Update");
    }
}

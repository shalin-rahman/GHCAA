using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Moq;
using static GHCAA.Domain.Enums;

namespace GHCAA.Tests.Services;

[TestFixture]
public class EventServiceTests : TestBase
{
    private EventService _service = null!;
    private Mock<ICommunicationService> _communicationMock = null!;
    private Mock<IFileStorageService> _fileStorageMock = null!;

    [SetUp]
    public async Task Setup()
    {
        _communicationMock = new Mock<ICommunicationService>();
        _fileStorageMock = new Mock<IFileStorageService>();
        _service = new EventService(_context, _communicationMock.Object, _fileStorageMock.Object);

        // Clear seed data so count assertions are deterministic
        _context.AlumniEvents.RemoveRange(_context.AlumniEvents);
        await _context.SaveChangesAsync();
    }

    [Test]
    public async Task CreateEventAsync_ShouldAddEvent()
    {
        var dto = new CreateEventDto { Title = "Test Event", Description = "Test Description", Date = DateTime.UtcNow.AddDays(10), Location = "Test Location", RegistrationFee = 100, IsActive = true, RegistrationDeadline = DateTime.UtcNow.AddDays(5), AdminNote = "Staff only" };
        var result = await _service.CreateEventAsync(dto);

        result.Should().NotBeNull();
        result.Title.Should().Be("Test Event");
        result.AdminNote.Should().Be("Staff only");
        _context.AlumniEvents.Count().Should().Be(1);
    }

    [Test]
    public async Task RegisterForEventAsync_ShouldCreateRegistration()
    {
        var member = new Member { FullName = "EVT", Email = "e@t.com", NID = "12", FatherName = "F", MotherName = "M", MobileNo = "12", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0", HighestCertificate="HSC", HighestCertificateGroup="S", HighestCertificateSubject="None", GHCLastCertificate="HSC", GHCLastCertificateGroup="S", GHCLastCertificateSubject="None", ProfessionalSector="P", Designation="D" };
        var ev = new AlumniEvent { Title = "Event 1", Description = "D", Date = DateTime.UtcNow, Location = "L" };
        _context.Members.Add(member);
        _context.AlumniEvents.Add(ev);
        await _context.SaveChangesAsync();

        var result = await _service.RegisterForEventAsync(new RegisterForEventDto { EventId = ev.Id, PaymentReference = "REF123" }, member.Id, null);

        result.Should().NotBeNull();
        result.EventId.Should().Be(ev.Id);
        result.PaymentReference.Should().Be("REF123");
        result.Status.Should().Be(EventRegistrationStatus.Pending);
    }

    [Test]
    public async Task ApproveRegistrationAsync_ShouldUpdateStatusAndSendEmail()
    {
        var member = new Member { FullName = "Test Member", Email = "evtest@example.com", NID = "EVT1", FatherName = "F", MotherName = "M", MobileNo = "EVT1", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0", HighestCertificate="HSC", HighestCertificateGroup="S", HighestCertificateSubject="None", GHCLastCertificate="HSC", GHCLastCertificateGroup="S", GHCLastCertificateSubject="None", ProfessionalSector = "P", Designation = "D" };
        var ev = new AlumniEvent { Title = "Grand Reunion", Description = "D", Date = DateTime.UtcNow, Location = "Campus" };
        _context.Members.Add(member);
        _context.AlumniEvents.Add(ev);
        await _context.SaveChangesAsync();

        var reg = new EventRegistration { EventId = ev.Id, MemberId = member.Id, PaymentReference = "EVTREG999", Status = EventRegistrationStatus.Pending };
        _context.EventRegistrations.Add(reg);
        await _context.SaveChangesAsync();

        var result = await _service.ApproveRegistrationAsync(reg.Id, 99, true);

        result.Should().BeTrue();
        var updated = await _context.EventRegistrations.FindAsync(reg.Id);
        updated!.Status.Should().Be(EventRegistrationStatus.Approved);
        _communicationMock.Verify(c => c.SendIndividualEmailAsync(member.Id, "EVENT_REGISTRATION_CONFIRMATION", It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ApproveRegistrationAsync_ShouldReturnFalse_WhenRegistrationNotFound()
    {
        var result = await _service.ApproveRegistrationAsync(999, 1, true);
        result.Should().BeFalse();
    }

    [Test]
    public async Task RegisterForEventAsync_ShouldIncludeReceiptPath_WhenFileProvided()
    {
        var member = new Member { FullName = "EVT2", Email = "e2@t.com", NID = "123", FatherName = "F", MotherName = "M", MobileNo = "123", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0", HighestCertificate="HSC", HighestCertificateGroup="S", HighestCertificateSubject="None", GHCLastCertificate="HSC", GHCLastCertificateGroup="S", GHCLastCertificateSubject="None", ProfessionalSector="P", Designation="D" };
        var ev = new AlumniEvent { Title = "E", Description = "D", Date = DateTime.UtcNow, Location = "L" };
        _context.Members.Add(member);
        _context.AlumniEvents.Add(ev);
        await _context.SaveChangesAsync();

        var receiptDto = new UploadedFileDto { FileName = "r.jpg", Content = new MemoryStream() };
        _fileStorageMock.Setup(f => f.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<FileUploadType>(), It.IsAny<CancellationToken>())).ReturnsAsync("/uploads/r.jpg");

        var result = await _service.RegisterForEventAsync(new RegisterForEventDto { EventId = ev.Id, PaymentReference = "P" }, member.Id, receiptDto);

        result.ReceiptPath.Should().Be("/uploads/r.jpg");
    }

    [Test]
    public async Task RegisterForEventAsync_ShouldThrowException_WhenDeadlinePassed()
    {
        var ev = new AlumniEvent { Title = "Past Event", Description = "D", Date = DateTime.UtcNow, Location = "L", RegistrationDeadline = DateTime.UtcNow.AddHours(-1) };
        _context.AlumniEvents.Add(ev);
        await _context.SaveChangesAsync();

        Func<Task> act = async () => await _service.RegisterForEventAsync(new RegisterForEventDto { EventId = ev.Id, PaymentReference = "P" }, 1, null);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Registration is closed*");
    }

    [Test]
    public async Task UpdateEventAsync_ShouldUpdateAllFields()
    {
        var ev = new AlumniEvent { Title = "Old Title", Description = "D", Date = DateTime.UtcNow, Location = "L" };
        _context.AlumniEvents.Add(ev);
        await _context.SaveChangesAsync();

        var result = await _service.UpdateEventAsync(new UpdateEventDto { Id = ev.Id, Title = "New Title", Description = "New D", Date = DateTime.UtcNow.AddDays(1), Location = "New Loc", AdminNote = "Important Update" });

        result.Should().NotBeNull();
        result!.Title.Should().Be("New Title");
        result.AdminNote.Should().Be("Important Update");
    }

    [Test]
    public async Task RegisterForEventAsync_NonMember_ShouldFail_WhenEventDoesNotAllow()
    {
        var ev = new AlumniEvent { Title = "Member Only", Description = "D", Date = DateTime.UtcNow, Location = "L", AllowNonMembers = false };
        _context.AlumniEvents.Add(ev);
        await _context.SaveChangesAsync();

        var dto = new RegisterForEventDto { EventId = ev.Id, PaymentReference = "P", IsNonMember = true, GuestName = "Guest" };
        Func<Task> act = async () => await _service.RegisterForEventAsync(dto, null, null);
        
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("This event is for members only.");
    }

    [Test]
    public async Task RegisterForEventAsync_NonMember_ShouldSucceed_WhenEventAllows()
    {
        var ev = new AlumniEvent { Title = "Open Event", Description = "D", Date = DateTime.UtcNow, Location = "L", AllowNonMembers = true };
        _context.AlumniEvents.Add(ev);
        await _context.SaveChangesAsync();

        var dto = new RegisterForEventDto { EventId = ev.Id, PaymentReference = "P", IsNonMember = true, GuestName = "Guest", GuestEmail = "guest@ex.com" };
        var result = await _service.RegisterForEventAsync(dto, null, null);

        result.Should().NotBeNull();
        result.IsNonMember.Should().BeTrue();
        result.GuestName.Should().Be("Guest");
        result.MemberId.Should().BeNull();
    }

    [Test]
    public async Task UpdateEventLogoAsync_ShouldUpdateImageUrl()
    {
        var ev = new AlumniEvent { Title = "Event", Description = "D", Date = DateTime.UtcNow, Location = "L" };
        _context.AlumniEvents.Add(ev);
        await _context.SaveChangesAsync();

        var logoDto = new UploadedFileDto { FileName = "logo.png", Content = new MemoryStream() };
        _fileStorageMock.Setup(f => f.SaveFileAsync(It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<FileUploadType>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("/uploads/logo.png");

        var result = await _service.UpdateEventLogoAsync(ev.Id, logoDto);

        result.Should().NotBeNullOrEmpty();
        var updated = await _context.AlumniEvents.FindAsync(ev.Id);
        updated!.ImageUrl.Should().Be("/uploads/logo.png");
    }
}

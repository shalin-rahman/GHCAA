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
    private Mock<IGamificationService> _gamificationMock = null!;
    private Mock<INotificationService> _notificationMock = null!;

    [SetUp]
    public async Task Setup()
    {
        _communicationMock = new Mock<ICommunicationService>();
        _fileStorageMock = new Mock<IFileStorageService>();
        _gamificationMock = new Mock<IGamificationService>();
        _notificationMock = new Mock<INotificationService>();
        _service = new EventService(_context, _communicationMock.Object, _fileStorageMock.Object, _gamificationMock.Object, _notificationMock.Object);

        // Clear seed data so count assertions are deterministic
        _context.AlumniEvents.RemoveRange(_context.AlumniEvents);
        await _context.SaveChangesAsync();
    }

    [Test]
    public async Task CreateEventAsync_ShouldAddEvent()
    {
        var dto = new CreateEventDto { Title = "Test Event", Description = "Test Description", Location = "Dhaka City Park", StartDate = DateTime.UtcNow.AddDays(30), EndDate = DateTime.UtcNow.AddDays(31), RegistrationFee = 100, IsActive = true, RegistrationEndDate = DateTime.UtcNow.AddDays(5), AdminNote = "Staff only" };
        var result = await _service.CreateEventAsync(dto);

        result.Should().NotBeNull();
        result.Title.Should().Be("Test Event");
        result.AdminNote.Should().Be("Staff only");
        _context.AlumniEvents.Count().Should().Be(1);
    }

    [Test]
    public async Task RegisterForEventAsync_ShouldCreateRegistration()
    {
var member = new Member { FullName = "EVT", Email = "e@t.com", NID = "12", MobileNo = "12", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0" };
        var ev = new AlumniEvent { Title = "Event 1", Description = "D", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddHours(2), Location = "L" };
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
var member = new Member { FullName = "Test Member", Email = "evtest@example.com", NID = "EVT1", MobileNo = "EVT1", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0" };
        var ev = new AlumniEvent { Title = "Grand Reunion", Description = "D", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddHours(2), Location = "Campus" };
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
        _communicationMock.Verify(c => c.SendIndividualEmailAsync(member.Id, "EVENT_PARTICIPATION_APPROVED", It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()), Times.Once);
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
var member = new Member { FullName = "EVT2", Email = "e2@t.com", NID = "123", MobileNo = "123", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0" };
        var ev = new AlumniEvent { Title = "E", Description = "D", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddHours(2), Location = "L" };
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
        var ev = new AlumniEvent { Title = "Past Event", Description = "D", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddHours(2), Location = "L", RegistrationEndDate = DateTime.UtcNow.AddHours(-1) };
        _context.AlumniEvents.Add(ev);
        await _context.SaveChangesAsync();

        Func<Task> act = async () => await _service.RegisterForEventAsync(new RegisterForEventDto { EventId = ev.Id, PaymentReference = "P" }, 1, null);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Registration for this event is closed.");
    }

    [Test]
    public async Task UpdateEventAsync_ShouldUpdateAllFields()
    {
        var ev = new AlumniEvent { Title = "Old Title", Description = "D", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddHours(2), Location = "L" };
        _context.AlumniEvents.Add(ev);
        await _context.SaveChangesAsync();

        var result = await _service.UpdateEventAsync(new UpdateEventDto { Id = ev.Id, Title = "New Title", Description = "New D", StartDate = DateTime.UtcNow.AddDays(1), EndDate = DateTime.UtcNow.AddDays(2), Location = "New Loc", AdminNote = "Important Update" });

        result.Should().NotBeNull();
        result!.Title.Should().Be("New Title");
        result.AdminNote.Should().Be("Important Update");
    }

    [Test]
    public async Task RegisterForEventAsync_NonMember_ShouldFail_WhenEventDoesNotAllow()
    {
        var ev = new AlumniEvent { Title = "Member Only", Description = "D", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddHours(2), Location = "L", AllowNonMembers = false };
        _context.AlumniEvents.Add(ev);
        await _context.SaveChangesAsync();

        var dto = new RegisterForEventDto { EventId = ev.Id, PaymentReference = "P", IsNonMember = true, GuestName = "Guest" };
        Func<Task> act = async () => await _service.RegisterForEventAsync(dto, null, null);
        
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("This event is for members only.");
    }

    [Test]
    public async Task RegisterForEventAsync_NonMember_ShouldSucceed_WhenEventAllows()
    {
        var ev = new AlumniEvent { Title = "Open Event", Description = "D", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddHours(2), Location = "L", AllowNonMembers = true };
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
        var ev = new AlumniEvent { Title = "Event", Description = "D", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddHours(2), Location = "L" };
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

    [Test]
    public async Task GetAllRegistrationsForAdminAsync_ShouldIncludeReceiptPath()
    {
        var ev = new AlumniEvent { Title = "Event 1", Description = "D", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddHours(2), Location = "L" };
        _context.AlumniEvents.Add(ev);
        await _context.SaveChangesAsync();

        var reg = new EventRegistration 
        { 
            EventId = ev.Id, 
            PaymentReference = "P1", 
            ReceiptPath = "/uploads/receipt.pdf",
            Status = EventRegistrationStatus.Pending 
        };
        _context.EventRegistrations.Add(reg);
        await _context.SaveChangesAsync();

        var result = await _service.GetAllRegistrationsForAdminAsync(1, 10);
        
        // Dynamic dynamic check or reflection if result is anonymous
        var items = (System.Collections.IEnumerable)result.GetType().GetProperty("Items")!.GetValue(result, null)!;
        var firstItem = items.Cast<object>().First();
        var path = firstItem.GetType().GetProperty("ReceiptPath")!.GetValue(firstItem, null);
        
        path.Should().Be("/uploads/receipt.pdf");
    }

    [Test]
    public async Task DeleteEventAsync_ShouldRemoveEventIfNoRegistrations()
    {
        var ev = new AlumniEvent { Title = "Empty Event", Description = "D", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddHours(1), Location = "L" };
        _context.AlumniEvents.Add(ev);
        await _context.SaveChangesAsync();

        var result = await _service.DeleteEventAsync(ev.Id);
        result.Should().BeTrue();
        _context.AlumniEvents.Any(e => e.Id == ev.Id).Should().BeFalse();
    }

    [Test]
    public async Task CheckInParticipantAsync_ShouldSetCheckInTime()
    {
        var member = new Member { FullName = "EVT3", Email = "e3@t.com", NID = "1234", MobileNo = "1234", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0" };
        var ev = new AlumniEvent { Title = "E", Description = "D", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddHours(2), Location = "L" };
        _context.Members.Add(member);
        _context.AlumniEvents.Add(ev);
        await _context.SaveChangesAsync();

        var reg = new EventRegistration { EventId = ev.Id, MemberId = member.Id, Status = EventRegistrationStatus.Approved, TicketCode = "TC-1" };
        _context.EventRegistrations.Add(reg);
        await _context.SaveChangesAsync();

        var result = await _service.CheckInParticipantAsync(reg.Id);
        result.Should().BeTrue();
        var updated = await _context.EventRegistrations.FindAsync(reg.Id);
        updated!.CheckedInAt.Should().NotBeNull();
    }

    [Test]
    public async Task AddEventExpenseAsync_ShouldCreateExpense()
    {
        var ev = new AlumniEvent { Title = "Exp Event", Description = "D", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddHours(1), Location = "L" };
        _context.AlumniEvents.Add(ev);
        await _context.SaveChangesAsync();

        var dto = new AddEventExpenseDto { EventId = ev.Id, Category = "Catering", Amount = 500, Note = "Lunch" };
        var result = await _service.AddEventExpenseAsync(dto);

        result.Should().NotBeNull();
        result.Amount.Should().Be(500);
        _context.EventExpenses.Any(ex => ex.Id == result.Id).Should().BeTrue();
    }
}


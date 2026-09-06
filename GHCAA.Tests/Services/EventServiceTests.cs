using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.Extensions.Logging;
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
    private Mock<ILogger<EventService>> _loggerMock = null!;

    [SetUp]
    public async Task Setup()
    {
        _communicationMock = new Mock<ICommunicationService>();
        _fileStorageMock = new Mock<IFileStorageService>();
        _gamificationMock = new Mock<IGamificationService>();
        _notificationMock = new Mock<INotificationService>();
        _loggerMock = new Mock<ILogger<EventService>>();
        _service = new EventService(_context, _communicationMock.Object, _fileStorageMock.Object, _gamificationMock.Object, _notificationMock.Object, _loggerMock.Object);

        // Clear seed data so count assertions are deterministic
        _context.AlumniEvents.RemoveRange(_context.AlumniEvents);
        await _context.SaveChangesAsync();
    }

    [Category("FR-13")]
    [Test]
    public async Task CreateEventAsync_ShouldAddEvent()
    {
        var startDate = DateTime.UtcNow.AddDays(30);
        var endDate = DateTime.UtcNow.AddDays(31);
        var regStartDate = DateTime.UtcNow.AddDays(1);
        var regEndDate = DateTime.UtcNow.AddDays(5);
        var dto = new CreateEventDto
        {
            Title = "Test Event",
            Description = "Test Description",
            Location = "Dhaka City Park",
            StartDate = startDate,
            EndDate = endDate,
            RegistrationFee = 100,
            RequiresPayment = true,
            IsActive = true,
            ImageUrl = "/uploads/events/test.jpg",
            RegistrationStartDate = regStartDate,
            RegistrationEndDate = regEndDate,
            AllowNonMembers = true,
            AdminNote = "Staff only",
            ParticipantLimit = 50,
            HasWaitlist = true,
            RequiresRegistration = true
        };
        var result = await _service.CreateEventAsync(dto);

        result.Should().NotBeNull();
        result.Title.Should().Be("Test Event");
        result.Description.Should().Be("Test Description");
        result.StartDate.Should().Be(DateTime.SpecifyKind(startDate, DateTimeKind.Utc));
        result.EndDate.Should().Be(DateTime.SpecifyKind(endDate, DateTimeKind.Utc));
        result.Location.Should().Be("Dhaka City Park");
        result.RegistrationFee.Should().Be(100);
        result.RequiresPayment.Should().BeTrue();
        result.IsActive.Should().BeTrue();
        result.ImageUrl.Should().Be("/uploads/events/test.jpg");
        result.RegistrationStartDate.Should().Be(DateTime.SpecifyKind(regStartDate, DateTimeKind.Utc));
        result.RegistrationEndDate.Should().Be(DateTime.SpecifyKind(regEndDate, DateTimeKind.Utc));
        result.AllowNonMembers.Should().BeTrue();
        result.AdminNote.Should().Be("Staff only");
        result.ParticipantLimit.Should().Be(50);
        result.HasWaitlist.Should().BeTrue();
        result.RequiresRegistration.Should().BeTrue();
        _context.AlumniEvents.Count().Should().Be(1);
    }

    [Test]
    public async Task GetAllEventsForAdminAsync_ShouldIncludeInactiveEvents()
    {
        var active = new AlumniEvent { Title = "Active Event", Description = "D", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddHours(2), Location = "L", IsActive = true };
        var inactive = new AlumniEvent { Title = "Unpublished Event", Description = "D", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddHours(2), Location = "L", IsActive = false };
        _context.AlumniEvents.AddRange(active, inactive);
        await _context.SaveChangesAsync();

        var result = await _service.GetAllEventsForAdminAsync();

        result.Should().HaveCount(2);
        result.Select(e => e.Title).Should().Contain("Unpublished Event");
    }

    [Test]
    public async Task GetActiveEventsAsync_ShouldExcludeInactiveEvents()
    {
        var active = new AlumniEvent { Title = "Active Event", Description = "D", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddHours(2), Location = "L", IsActive = true };
        var inactive = new AlumniEvent { Title = "Unpublished Event", Description = "D", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddHours(2), Location = "L", IsActive = false };
        _context.AlumniEvents.AddRange(active, inactive);
        await _context.SaveChangesAsync();

        var result = await _service.GetActiveEventsAsync();

        result.Should().ContainSingle();
        result.Single().Title.Should().Be("Active Event");
    }

    [Category("FR-13")]
    [Test]
    public async Task UpdateEventAsync_ShouldAllowReactivatingAnUnpublishedEvent()
    {
        var ev = new AlumniEvent { Title = "Unpublished Event", Description = "D", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddHours(2), Location = "L", IsActive = false };
        _context.AlumniEvents.Add(ev);
        await _context.SaveChangesAsync();

        var dto = new UpdateEventDto
        {
            Id = ev.Id,
            Title = ev.Title,
            Description = ev.Description,
            StartDate = ev.StartDate,
            EndDate = ev.EndDate,
            Location = ev.Location,
            IsActive = true
        };

        var result = await _service.UpdateEventAsync(dto);

        result.Should().NotBeNull();
        result!.IsActive.Should().BeTrue();
    }

    [Category("FR-14")]
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

    [Category("FR-14")]
    [Test]
    public async Task RegisterForEventAsync_ShouldThrowException_WhenDeadlinePassed()
    {
        var ev = new AlumniEvent { Title = "Past Event", Description = "D", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddHours(2), Location = "L", RegistrationEndDate = DateTime.UtcNow.AddHours(-1) };
        _context.AlumniEvents.Add(ev);
        await _context.SaveChangesAsync();

        Func<Task> act = async () => await _service.RegisterForEventAsync(new RegisterForEventDto { EventId = ev.Id, PaymentReference = "P" }, 1, null);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Registration for this event is closed.");
    }

    [Category("FR-13")]
    [Test]
    public async Task UpdateEventAsync_ShouldUpdateAllFields()
    {
        var ev = new AlumniEvent { Title = "Old Title", Description = "D", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddHours(2), Location = "L", RegistrationFee = 0, RequiresPayment = false, ImageUrl = "/old.jpg", AllowNonMembers = false, ParticipantLimit = 5, HasWaitlist = false, RequiresRegistration = false };
        _context.AlumniEvents.Add(ev);
        await _context.SaveChangesAsync();

        var newStartDate = DateTime.UtcNow.AddDays(1);
        var newEndDate = DateTime.UtcNow.AddDays(2);
        var newRegStartDate = DateTime.UtcNow.AddHours(1);
        var newRegEndDate = DateTime.UtcNow.AddHours(12);
        var result = await _service.UpdateEventAsync(new UpdateEventDto
        {
            Id = ev.Id,
            Title = "New Title",
            Description = "New D",
            StartDate = newStartDate,
            EndDate = newEndDate,
            Location = "New Loc",
            RegistrationFee = 250,
            RequiresPayment = true,
            IsActive = true,
            ImageUrl = "/new.jpg",
            RegistrationStartDate = newRegStartDate,
            RegistrationEndDate = newRegEndDate,
            AllowNonMembers = true,
            AdminNote = "Important Update",
            ParticipantLimit = 20,
            HasWaitlist = true,
            RequiresRegistration = true
        });

        result.Should().NotBeNull();
        result!.Title.Should().Be("New Title");
        result.Description.Should().Be("New D");
        result.StartDate.Should().Be(DateTime.SpecifyKind(newStartDate, DateTimeKind.Utc));
        result.EndDate.Should().Be(DateTime.SpecifyKind(newEndDate, DateTimeKind.Utc));
        result.Location.Should().Be("New Loc");
        result.RegistrationFee.Should().Be(250);
        result.RequiresPayment.Should().BeTrue();
        result.IsActive.Should().BeTrue();
        result.ImageUrl.Should().Be("/new.jpg");
        result.RegistrationStartDate.Should().Be(DateTime.SpecifyKind(newRegStartDate, DateTimeKind.Utc));
        result.RegistrationEndDate.Should().Be(DateTime.SpecifyKind(newRegEndDate, DateTimeKind.Utc));
        result.AllowNonMembers.Should().BeTrue();
        result.AdminNote.Should().Be("Important Update");
        result.ParticipantLimit.Should().Be(20);
        result.HasWaitlist.Should().BeTrue();
        result.RequiresRegistration.Should().BeTrue();
    }

    [Category("FR-14")]
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

    [Category("FR-14")]
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

    [Category("FR-13")]
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

    [Category("FR-16")]
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

    [Category("FR-13")]
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

    [Category("FR-15")]
    [Test]
    public async Task RegisterForEventAsync_ShouldWaitlist_WhenCapacityExceeded()
    {
        // Arrange
        var ev = new AlumniEvent
        {
            Title = "Full Event",
            Description = "D",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            Location = "L",
            ParticipantLimit = 1,
            HasWaitlist = true,
            IsActive = true
        };
        _context.AlumniEvents.Add(ev);
        await _context.SaveChangesAsync();

        // Add one approved registration to fill capacity
        var reg1 = new EventRegistration
        {
            EventId = ev.Id,
            Status = EventRegistrationStatus.Approved,
            PaymentReference = "P1",
            TicketCode = "T1",
            RegisteredAt = DateTime.UtcNow
        };
        _context.EventRegistrations.Add(reg1);
        await _context.SaveChangesAsync();

        // Act
        var member = new Member { FullName = "Waitlister", Email = "w@t.com", NID = "W1", MobileNo = "W1", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0" };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        var result = await _service.RegisterForEventAsync(new RegisterForEventDto { EventId = ev.Id, PaymentReference = "P2" }, member.Id, null);

        // Assert
        result.Status.Should().Be(EventRegistrationStatus.Waitlisted);
    }

    [Category("FR-15")]
    [Test]
    public async Task RegisterForEventAsync_ShouldThrow_WhenCapacityFull_AndNoWaitlist()
    {
        // 29A.4: with a participant limit but no waitlist, registrations past the cap must be
        // rejected — previously the cap was skipped entirely, allowing unlimited registrations.
        var ev = new AlumniEvent
        {
            Title = "Capped Event",
            Description = "D",
            StartDate = DateTime.UtcNow.AddDays(1),
            EndDate = DateTime.UtcNow.AddDays(2),
            Location = "L",
            ParticipantLimit = 1,
            HasWaitlist = false,
            IsActive = true
        };
        _context.AlumniEvents.Add(ev);
        await _context.SaveChangesAsync();

        // A single Pending registration already occupies the one available slot.
        _context.EventRegistrations.Add(new EventRegistration
        {
            EventId = ev.Id,
            Status = EventRegistrationStatus.Pending,
            PaymentReference = "P1",
            TicketCode = "C1",
            RegisteredAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        var member = new Member { FullName = "Latecomer", Email = "l@t.com", NID = "L1", MobileNo = "L1", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0" };
        _context.Members.Add(member);
        await _context.SaveChangesAsync();

        Func<Task> act = async () => await _service.RegisterForEventAsync(
            new RegisterForEventDto { EventId = ev.Id, PaymentReference = "P2" }, member.Id, null);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*participant limit*");
    }

    [Test]
    public async Task RegisterForEventAsync_ShouldNotThrow_AndShouldLogWarning_WhenParticipationEmailFails()
    {
        // fc06894 hardened this path: a broken mail send (bad SMTP config, template error) used to
        // be silently swallowed. Registration must still succeed, but the failure now goes through
        // ILogger<EventService> instead of vanishing entirely.
        var member = new Member { FullName = "EVT4", Email = "e4@t.com", NID = "9999", MobileNo = "9999", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0" };
        var ev = new AlumniEvent { Title = "Mail Fail Event", Description = "D", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddHours(2), Location = "L" };
        _context.Members.Add(member);
        _context.AlumniEvents.Add(ev);
        await _context.SaveChangesAsync();

        _communicationMock
            .Setup(c => c.SendIndividualEmailAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("SMTP down"));

        Func<Task> act = async () => await _service.RegisterForEventAsync(
            new RegisterForEventDto { EventId = ev.Id, PaymentReference = "P" }, member.Id, null);

        await act.Should().NotThrowAsync();

        _context.EventRegistrations.Any(r => r.EventId == ev.Id && r.MemberId == member.Id).Should().BeTrue();

        _loggerMock.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => true),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Category("FR-16")]
    [Test]
    public async Task CheckInParticipantAsync_ShouldFail_WhenNotApproved()
    {
        // 29A.5: a Pending (or Rejected/Waitlisted) registration must not be able to check in
        // or earn attendance points.
        var member = new Member { FullName = "Pend", Email = "p@t.com", NID = "P1", MobileNo = "P1", FatherName = "F", MotherName = "M", PresentAddress = "A", PermanentAddress = "A", EmergencyContactName = "E", EmergencyContactRelation = "R", EmergencyContactPhone = "0" };
        var ev = new AlumniEvent { Title = "E", Description = "D", StartDate = DateTime.UtcNow, EndDate = DateTime.UtcNow.AddHours(2), Location = "L" };
        _context.Members.Add(member);
        _context.AlumniEvents.Add(ev);
        await _context.SaveChangesAsync();

        var reg = new EventRegistration { EventId = ev.Id, MemberId = member.Id, Status = EventRegistrationStatus.Pending, TicketCode = "TC-P" };
        _context.EventRegistrations.Add(reg);
        await _context.SaveChangesAsync();

        var result = await _service.CheckInParticipantAsync(reg.Id);

        result.Should().BeFalse();
        var updated = await _context.EventRegistrations.FindAsync(reg.Id);
        updated!.IsCheckedIn.Should().BeFalse();
        updated.CheckedInAt.Should().BeNull();
    }

    // 80.13: extracted from GatewaysController's payment-initiation/callback paths so it
    // doesn't touch ApplicationDbContext directly.
    [Test]
    public async Task GetRegistrationByPaymentReferenceAsync_ReturnsMatch_WithEventIncluded()
    {
        var ev = new AlumniEvent { Title = "E", Description = "D", Location = "L" };
        _context.AlumniEvents.Add(ev);
        await _context.SaveChangesAsync();

        var reg = new EventRegistration { EventId = ev.Id, PaymentReference = "EVT-REG-XYZ", Status = EventRegistrationStatus.Pending };
        _context.EventRegistrations.Add(reg);
        await _context.SaveChangesAsync();

        var result = await _service.GetRegistrationByPaymentReferenceAsync("EVT-REG-XYZ");

        result.Should().NotBeNull();
        result!.Event.Should().NotBeNull();
        result.Event!.Title.Should().Be("E");
    }

    [Test]
    public async Task GetRegistrationByPaymentReferenceAsync_ReturnsNull_WhenNoMatch()
    {
        var result = await _service.GetRegistrationByPaymentReferenceAsync("NO-SUCH-REF");

        result.Should().BeNull();
    }

    [Test]
    public async Task AutoApproveRegistrationAfterPaymentAsync_SetsApprovedFieldsWithoutNotification()
    {
        var ev = new AlumniEvent { Title = "E2", Description = "D", Location = "L" };
        _context.AlumniEvents.Add(ev);
        await _context.SaveChangesAsync();

        var reg = new EventRegistration { EventId = ev.Id, PaymentReference = "EVT-REG-AUTO", Status = EventRegistrationStatus.Pending };
        _context.EventRegistrations.Add(reg);
        await _context.SaveChangesAsync();

        await _service.AutoApproveRegistrationAfterPaymentAsync(reg.Id, adminId: 1);

        var updated = await _context.EventRegistrations.FindAsync(reg.Id);
        updated!.Status.Should().Be(EventRegistrationStatus.Approved);
        updated.ApprovedByAdminId.Should().Be(1);
        updated.ApprovedAt.Should().NotBeNull();

        // Deliberately narrower than ApproveRegistrationAsync: this is a payment confirmation,
        // not an admin review, so it must not fire the participation-approved notification.
        _notificationMock.Verify(x => x.CreateNotificationAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<NotificationType>(), It.IsAny<string>(), It.IsAny<System.Threading.CancellationToken>()),
            Times.Never);
    }
}


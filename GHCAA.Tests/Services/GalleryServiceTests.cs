using System.Threading.Tasks;
using FluentAssertions;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Services;

// Coverage for the Work Package 40 member-album approval workflow (previously zero test coverage
// despite the migration/bootstrap issues that shipped alongside it — see Work Package 41 postmortems).
[TestFixture]
public class GalleryServiceTests : TestBase
{
    private Mock<IAdminNotificationService> _adminNotification = null!;
    private Mock<INotificationService> _notification = null!;
    private GalleryService _service = null!;

    [SetUp]
    public void Setup()
    {
        _adminNotification = new Mock<IAdminNotificationService>();
        _notification = new Mock<INotificationService>();
        _service = new GalleryService(_context, _adminNotification.Object, _notification.Object);
    }

    private async Task<int> CreateMemberIdAsync(string name = "Album Owner")
    {
        var unique = Guid.NewGuid().ToString("N");
        var member = await CreateAndSaveTestMemberAsync(name, $"{unique}@example.com", $"017{unique[..8]}", unique[..10]);
        return member.Id;
    }

    // ---- member album creation -------------------------------------------------

    [Category("FR-53")]
        [Test]
    public async Task CreateMemberAlbumAsync_CreatesPendingInactiveAlbum_AndNotifiesAdmins()
    {
        var memberId = await CreateMemberIdAsync();

        var album = await _service.CreateMemberAlbumAsync(memberId, "Reunion 2026", "Photos from the reunion");

        album.Status.Should().Be(Enums.SubmissionStatus.Pending);
        album.IsActive.Should().BeFalse("a new member album must wait for admin approval before going public");
        album.OwnerMemberId.Should().Be(memberId);

        _adminNotification.Verify(x => x.NotifyPendingApprovalAsync(
            "Album", "Reunion 2026", It.IsAny<string>(), $"/admin/gallery/{album.Id}", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Category("FR-53")]
        [Test]
    public async Task AddMemberPhotoToAlbumAsync_CreatesPendingPhoto_AndNotifiesAdmins()
    {
        var memberId = await CreateMemberIdAsync();
        var album = await _service.CreateMemberAlbumAsync(memberId, "Reunion 2026", null);

        var photo = await _service.AddMemberPhotoToAlbumAsync(memberId, album.Id, "/uploads/reunion1.jpg", "Group photo");

        photo.Should().NotBeNull();
        photo!.Status.Should().Be(Enums.SubmissionStatus.Pending);
        photo.UploadedByMemberId.Should().Be(memberId);
        _adminNotification.Verify(x => x.NotifyPendingApprovalAsync(
            "Photo", "Reunion 2026", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Category("FR-53")]
        [Test]
    public async Task AddMemberPhotoToAlbumAsync_ReturnsNull_WhenAlbumDoesNotExist()
    {
        var memberId = await CreateMemberIdAsync();

        var photo = await _service.AddMemberPhotoToAlbumAsync(memberId, galleryId: 999_999, "/uploads/x.jpg", null);

        photo.Should().BeNull();
    }

    [Category("FR-53")]
        [Test]
    public async Task GetMemberAlbumsAsync_ReturnsOnlyThatMembersAlbums()
    {
        var ownerA = await CreateMemberIdAsync("Owner A");
        var ownerB = await CreateMemberIdAsync("Owner B");
        await _service.CreateMemberAlbumAsync(ownerA, "A's Album", null);
        await _service.CreateMemberAlbumAsync(ownerB, "B's Album", null);

        var albums = await _service.GetMemberAlbumsAsync(ownerA);

        albums.Should().ContainSingle().Which.Title.Should().Be("A's Album");
    }

    // ---- public/approved visibility filtering ------------------------------------

    [Test]
    public async Task GetAllGalleriesAsync_OnlyActive_ExcludesUnapprovedGalleries()
    {
        var memberId = await CreateMemberIdAsync();
        await _service.CreateMemberAlbumAsync(memberId, "Still Pending", null); // Pending, IsActive=false
        var approved = await _service.CreateEventGalleryAsync(new EventGallery
        {
            Title = "Approved Gallery",
            EventDate = DateTime.UtcNow,
            Status = Enums.SubmissionStatus.Approved,
            IsActive = true
        });

        var visible = await _service.GetAllGalleriesAsync(onlyActive: true);

        visible.Should().ContainSingle(g => g.Id == approved.Id);
        visible.Should().NotContain(g => g.Title == "Still Pending");
    }

    [Test]
    public async Task GetAllGalleriesAsync_OnlyActive_FiltersOutUnapprovedPhotos_WithinAnApprovedGallery()
    {
        var gallery = await _service.CreateEventGalleryAsync(new EventGallery
        {
            Title = "Mixed Gallery",
            EventDate = DateTime.UtcNow,
            Status = Enums.SubmissionStatus.Approved,
            IsActive = true
        });
        _context.EventPhotos.Add(new EventPhoto { EventGalleryId = gallery.Id, PhotoPath = "/a.jpg", Status = Enums.SubmissionStatus.Approved });
        _context.EventPhotos.Add(new EventPhoto { EventGalleryId = gallery.Id, PhotoPath = "/b.jpg", Status = Enums.SubmissionStatus.Pending });
        await _context.SaveChangesAsync();

        var visible = await _service.GetAllGalleriesAsync(onlyActive: true);

        var loaded = visible.Single(g => g.Id == gallery.Id);
        loaded.Photos.Should().ContainSingle().Which.PhotoPath.Should().Be("/a.jpg");
    }

    [Test]
    public async Task GetGalleryByIdAsync_FiltersPendingPhotos_WhenGalleryIsApproved()
    {
        var gallery = await _service.CreateEventGalleryAsync(new EventGallery
        {
            Title = "Detail View",
            EventDate = DateTime.UtcNow,
            Status = Enums.SubmissionStatus.Approved,
            IsActive = true
        });
        _context.EventPhotos.Add(new EventPhoto { EventGalleryId = gallery.Id, PhotoPath = "/approved.jpg", Status = Enums.SubmissionStatus.Approved });
        _context.EventPhotos.Add(new EventPhoto { EventGalleryId = gallery.Id, PhotoPath = "/pending.jpg", Status = Enums.SubmissionStatus.Pending });
        await _context.SaveChangesAsync();

        var loaded = await _service.GetGalleryByIdAsync(gallery.Id);

        loaded!.Photos.Should().ContainSingle().Which.PhotoPath.Should().Be("/approved.jpg");
    }

    // ---- admin approval queues ----------------------------------------------------

    [Test]
    public async Task GetPendingGalleryApprovalsAsync_ReturnsOnlyPendingGalleries()
    {
        var memberId = await CreateMemberIdAsync();
        var pending = await _service.CreateMemberAlbumAsync(memberId, "Pending Album", null);
        await _service.CreateEventGalleryAsync(new EventGallery { Title = "Already Approved", EventDate = DateTime.UtcNow, Status = Enums.SubmissionStatus.Approved, IsActive = true });

        var queue = await _service.GetPendingGalleryApprovalsAsync();

        queue.Should().ContainSingle(g => g.Id == pending.Id);
    }

    [Test]
    public async Task GetPendingPhotoApprovalsAsync_ReturnsOnlyPendingPhotos()
    {
        var memberId = await CreateMemberIdAsync();
        var album = await _service.CreateMemberAlbumAsync(memberId, "Album", null);
        await _service.AddMemberPhotoToAlbumAsync(memberId, album.Id, "/pending.jpg", null);
        _context.EventPhotos.Add(new EventPhoto { EventGalleryId = album.Id, PhotoPath = "/approved.jpg", Status = Enums.SubmissionStatus.Approved });
        await _context.SaveChangesAsync();

        var queue = await _service.GetPendingPhotoApprovalsAsync();

        queue.Should().ContainSingle(p => p.PhotoPath == "/pending.jpg");
    }

    // ---- approve / reject transitions ----------------------------------------------

    [Category("FR-53")]
        [Test]
    public async Task ApproveGalleryAsync_ActivatesGallery_ClearsRejectionReason_AndNotifiesOwner()
    {
        var memberId = await CreateMemberIdAsync();
        var album = await _service.CreateMemberAlbumAsync(memberId, "Album To Approve", null);
        album.RejectionReason = "previously rejected"; // simulate a prior reject-then-resubmit
        await _context.SaveChangesAsync();

        var result = await _service.ApproveGalleryAsync(album.Id);

        result.Should().BeTrue();
        var updated = await _context.EventGalleries.FindAsync(album.Id);
        updated!.Status.Should().Be(Enums.SubmissionStatus.Approved);
        updated.IsActive.Should().BeTrue();
        updated.RejectionReason.Should().BeNull();
        _notification.Verify(x => x.CreateNotificationAsync(
            memberId, "Album Approved", It.IsAny<string>(), Enums.NotificationType.GeneralSystem, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Test]
    public async Task ApproveGalleryAsync_ReturnsFalse_WhenGalleryDoesNotExist()
    {
        (await _service.ApproveGalleryAsync(999_999)).Should().BeFalse();
    }

    [Category("FR-53")]
        [Test]
    public async Task RejectGalleryAsync_DeactivatesGallery_AndRecordsReason()
    {
        var memberId = await CreateMemberIdAsync();
        var album = await _service.CreateMemberAlbumAsync(memberId, "Album To Reject", null);

        var result = await _service.RejectGalleryAsync(album.Id, "Inappropriate content");

        result.Should().BeTrue();
        var updated = await _context.EventGalleries.FindAsync(album.Id);
        updated!.Status.Should().Be(Enums.SubmissionStatus.Rejected);
        updated.IsActive.Should().BeFalse();
        updated.RejectionReason.Should().Be("Inappropriate content");
        _notification.Verify(x => x.CreateNotificationAsync(
            memberId, "Album Rejected", It.Is<string>(m => m.Contains("Inappropriate content")), Enums.NotificationType.GeneralSystem, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Category("FR-53")]
        [Test]
    public async Task ApprovePhotoAsync_ApprovesPhoto_AndNotifiesUploader()
    {
        var memberId = await CreateMemberIdAsync();
        var album = await _service.CreateMemberAlbumAsync(memberId, "Album", null);
        var photo = await _service.AddMemberPhotoToAlbumAsync(memberId, album.Id, "/p.jpg", null);

        var result = await _service.ApprovePhotoAsync(photo!.Id);

        result.Should().BeTrue();
        (await _context.EventPhotos.FindAsync(photo.Id))!.Status.Should().Be(Enums.SubmissionStatus.Approved);
        _notification.Verify(x => x.CreateNotificationAsync(
            memberId, "Photo Approved", It.IsAny<string>(), Enums.NotificationType.GeneralSystem, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Category("FR-53")]
        [Test]
    public async Task RejectPhotoAsync_RejectsPhoto_AndRecordsReason()
    {
        var memberId = await CreateMemberIdAsync();
        var album = await _service.CreateMemberAlbumAsync(memberId, "Album", null);
        var photo = await _service.AddMemberPhotoToAlbumAsync(memberId, album.Id, "/p.jpg", null);

        var result = await _service.RejectPhotoAsync(photo!.Id, "Blurry photo");

        result.Should().BeTrue();
        var updated = await _context.EventPhotos.FindAsync(photo.Id);
        updated!.Status.Should().Be(Enums.SubmissionStatus.Rejected);
        updated.RejectionReason.Should().Be("Blurry photo");
    }

    [Test]
    public async Task ApproveGalleryAsync_DoesNotNotify_WhenGalleryHasNoOwner()
    {
        // Admin-created galleries (not member albums) have no OwnerMemberId.
        var gallery = await _service.CreateEventGalleryAsync(new EventGallery
        {
            Title = "Admin Gallery",
            EventDate = DateTime.UtcNow,
            Status = Enums.SubmissionStatus.Pending,
            IsActive = false
        });

        await _service.ApproveGalleryAsync(gallery.Id);

        _notification.Verify(x => x.CreateNotificationAsync(
            It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Enums.NotificationType>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // ---- update --------------------------------------------------------------------

    [Category("FR-17")]
        [Test]
    public async Task UpdateEventGalleryAsync_ShouldUpdateAllFields_AndNormalizeEventDateToUtc()
    {
        var gallery = await _service.CreateEventGalleryAsync(new EventGallery
        {
            Title = "Old Title",
            Description = "Old Description",
            EventDate = DateTime.UtcNow,
            Status = Enums.SubmissionStatus.Pending,
            IsActive = false
        });
        DetachAll();

        var unspecifiedKindDate = DateTime.SpecifyKind(new DateTime(2026, 12, 25), DateTimeKind.Unspecified);
        var update = new EventGallery
        {
            Id = gallery.Id,
            Title = "New Title",
            Description = "New Description",
            EventDate = unspecifiedKindDate,
            Status = Enums.SubmissionStatus.Approved,
            IsActive = true
        };

        var result = await _service.UpdateEventGalleryAsync(update);

        result.EventDate.Kind.Should().Be(DateTimeKind.Utc);

        var saved = await _context.EventGalleries.FindAsync(gallery.Id);
        saved!.Title.Should().Be("New Title");
        saved.Description.Should().Be("New Description");
        saved.EventDate.Should().Be(DateTime.SpecifyKind(unspecifiedKindDate, DateTimeKind.Utc));
        saved.Status.Should().Be(Enums.SubmissionStatus.Approved);
        saved.IsActive.Should().BeTrue();
    }

    // ---- CRUD ------------------------------------------------------------------------

    [Test]
    public async Task DeleteGalleryAsync_RemovesGallery_AndReturnsFalse_WhenAlreadyGone()
    {
        var gallery = await _service.CreateEventGalleryAsync(new EventGallery { Title = "To Delete", EventDate = DateTime.UtcNow, IsActive = true });

        (await _service.DeleteGalleryAsync(gallery.Id)).Should().BeTrue();
        (await _context.EventGalleries.FindAsync(gallery.Id)).Should().BeNull();
        (await _service.DeleteGalleryAsync(gallery.Id)).Should().BeFalse();
    }

    [Category("FR-17")]
        [Test]
    public async Task AddPhotosToGalleryAsync_AddsAllPaths_AndReturnsFalse_ForUnknownGallery()
    {
        var gallery = await _service.CreateEventGalleryAsync(new EventGallery { Title = "Photo Batch", EventDate = DateTime.UtcNow, IsActive = true });

        var added = await _service.AddPhotosToGalleryAsync(gallery.Id, new[] { "/a.jpg", "/b.jpg" });

        added.Should().BeTrue();
        (await _context.EventPhotos.CountAsync(p => p.EventGalleryId == gallery.Id)).Should().Be(2);
        (await _service.AddPhotosToGalleryAsync(999_999, new[] { "/x.jpg" })).Should().BeFalse();
    }

    [Test]
    public async Task RemovePhotoAsync_RemovesPhoto_AndReturnsFalse_WhenNotFound()
    {
        var gallery = await _service.CreateEventGalleryAsync(new EventGallery { Title = "Gallery", EventDate = DateTime.UtcNow, IsActive = true });
        await _service.AddPhotosToGalleryAsync(gallery.Id, new[] { "/a.jpg" });
        var photo = await _context.EventPhotos.FirstAsync(p => p.EventGalleryId == gallery.Id);

        (await _service.RemovePhotoAsync(photo.Id)).Should().BeTrue();
        (await _service.RemovePhotoAsync(photo.Id)).Should().BeFalse();
    }
}

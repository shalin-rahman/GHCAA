using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Tests.Services;

public class ArchiveServiceTests
{
    private static (ApplicationDbContext Context, ArchiveService Service) Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var context = new ApplicationDbContext(options);
        return (context, new ArchiveService(context));
    }

    [Test]
    public async Task PublicItem_hides_unapproved_items()
    {
        var (db, service) = Create();
        var collection = new ArchiveCollection { Title = "Stories", PublicationState = Enums.ArchivePublicationState.Published, ModerationState = Enums.ArchiveModerationState.Approved };
        db.ArchiveCollections.Add(collection);
        db.ArchiveItems.AddRange(
            new ArchiveItem { ArchiveCollectionId = 1, Collection = collection, Narrator = "Approved", Transcript = "visible", PublicationState = Enums.ArchivePublicationState.Published, ModerationState = Enums.ArchiveModerationState.Approved },
            new ArchiveItem { ArchiveCollectionId = 1, Collection = collection, Narrator = "Pending", Transcript = "hidden", PublicationState = Enums.ArchivePublicationState.Draft, ModerationState = Enums.ArchiveModerationState.Pending });
        await db.SaveChangesAsync();

        var result = await service.GetPublicItemAsync(1);
        Assert.That(result?.Narrator, Is.EqualTo("Approved"));
        Assert.That(await service.GetPublicItemAsync(2), Is.Null);
    }

    [Test]
    public async Task Admin_items_include_incomplete_records()
    {
        var (db, service) = Create();
        db.ArchiveItems.Add(new ArchiveItem { ArchiveCollectionId = 1, Narrator = "Incomplete", Transcript = null, PublicationState = Enums.ArchivePublicationState.Draft, ModerationState = Enums.ArchiveModerationState.Pending });
        await db.SaveChangesAsync();

        var result = await service.GetAdminItemsAsync(null);
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Transcript, Is.Null);
    }

    [Test]
    public async Task Admin_search_matches_transcript()
    {
        var (db, service) = Create();
        db.ArchiveItems.AddRange(
            new ArchiveItem { ArchiveCollectionId = 1, Narrator = "A", Transcript = "flood of 1971" },
            new ArchiveItem { ArchiveCollectionId = 1, Narrator = "B", Transcript = "unrelated account" });
        await db.SaveChangesAsync();

        var result = await service.GetAdminItemsAsync("1971");
        Assert.That(result, Has.Count.EqualTo(1));
        Assert.That(result[0].Narrator, Is.EqualTo("A"));
    }

    [Test]
    public void CreateItem_rejects_records_without_media_or_transcript()
    {
        var (_, service) = Create();
        var dto = new GHCAA.Application.DTOs.ArchiveItemDto
        {
            ArchiveCollectionId = 1,
            Narrator = "Incomplete"
        };

        var exception = Assert.ThrowsAsync<InvalidOperationException>(
            async () => await service.CreateItemAsync(dto, 1));
        Assert.That(exception!.Message, Is.EqualTo(
            "An archive item must include a file upload, external media URL, or transcript."));
    }
}

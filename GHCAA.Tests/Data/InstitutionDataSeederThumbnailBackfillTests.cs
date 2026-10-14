using FluentAssertions;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace GHCAA.Tests.Data
{
    // The thumbnail column came after the photos were seeded. The backfill reads the paths from the
    // active profile's photos.json, so the migration holds no profile's file names.
    [TestFixture]
    [Category("NFR-P4")]
    public class InstitutionDataSeederThumbnailBackfillTests : TestBase
    {
        private int _galleryId;

        [SetUp]
        public async Task SetUp()
        {
            var gallery = new EventGallery { Title = "Backfill", EventDate = DateTime.UtcNow, Status = Enums.SubmissionStatus.Approved, IsActive = true };
            _context.EventGalleries.Add(gallery);
            await _context.SaveChangesAsync();
            _galleryId = gallery.Id;
        }

        private async Task<int> AddPhotoAsync(string path, string? thumb = null)
        {
            var photo = new EventPhoto { EventGalleryId = _galleryId, PhotoPath = path, ThumbnailPath = thumb, Status = Enums.SubmissionStatus.Approved };
            _context.EventPhotos.Add(photo);
            await _context.SaveChangesAsync();
            return photo.Id;
        }

        private static EventPhoto Seed(string path, string? thumb) => new() { PhotoPath = path, ThumbnailPath = thumb };

        private Task<string?> ThumbOfAsync(int id) =>
            _context.EventPhotos.IgnoreQueryFilters().Where(p => p.Id == id).Select(p => p.ThumbnailPath).SingleAsync();

        private Task BackfillAsync(params EventPhoto[] seed) =>
            InstitutionDataSeeder.BackfillPhotoThumbnailsAsync(_context, seed, NullLogger.Instance);

        [Test]
        public async Task FillsAMissingThumbnail_FromTheSeedRowWithTheSamePath()
        {
            var id = await AddPhotoAsync("/assets/gallery/a/01.jpg");

            await BackfillAsync(Seed("/assets/gallery/a/01.jpg", "/assets/gallery/a/thumbs/01.jpg"));

            (await ThumbOfAsync(id)).Should().Be("/assets/gallery/a/thumbs/01.jpg");
        }

        [Test]
        public async Task FillsAnEmptyThumbnail()
        {
            var id = await AddPhotoAsync("/assets/gallery/a/01.jpg", "");

            await BackfillAsync(Seed("/assets/gallery/a/01.jpg", "/assets/gallery/a/thumbs/01.jpg"));

            (await ThumbOfAsync(id)).Should().Be("/assets/gallery/a/thumbs/01.jpg");
        }

        [Test]
        public async Task LeavesAThumbnailThatIsAlreadySet()
        {
            var id = await AddPhotoAsync("/assets/gallery/a/01.jpg", "/custom/thumb.jpg");

            await BackfillAsync(Seed("/assets/gallery/a/01.jpg", "/assets/gallery/a/thumbs/01.jpg"));

            (await ThumbOfAsync(id)).Should().Be("/custom/thumb.jpg");
        }

        [Test]
        public async Task LeavesPhotosTheProfileDoesNotList()
        {
            var uploaded = await AddPhotoAsync("/uploads/members/1/galleryphoto/x.jpg");

            await BackfillAsync(Seed("/assets/gallery/a/01.jpg", "/assets/gallery/a/thumbs/01.jpg"));

            (await ThumbOfAsync(uploaded)).Should().BeNull();
        }

        [Test]
        public async Task SeedRowWithoutAThumbnail_ChangesNothing()
        {
            var id = await AddPhotoAsync("/assets/gallery/a/01.jpg");

            await BackfillAsync(Seed("/assets/gallery/a/01.jpg", null));

            (await ThumbOfAsync(id)).Should().BeNull();
        }

        [Test]
        public async Task NoSeedPhotos_IsANoOp()
        {
            await AddPhotoAsync("/assets/gallery/a/01.jpg");

            var act = () => BackfillAsync();

            await act.Should().NotThrowAsync();
        }
    }
}

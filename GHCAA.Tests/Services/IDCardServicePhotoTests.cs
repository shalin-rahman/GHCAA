using System.Text;
using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Infrastructure.Services;
using Microsoft.AspNetCore.Hosting;
using Moq;

namespace GHCAA.Tests.Services;

// Where the ID card finds the member's photo: the upload store first, then the web root for
// seeded assets/... photos that never go into the bucket.
[TestFixture]
public class IDCardServicePhotoTests : TestBase
{
    private string _webRoot = null!;
    private Mock<IFileStorageService> _storage = null!;
    private IDCardService _service = null!;

    [SetUp]
    public void PhotoSetup()
    {
        _webRoot = Path.Combine(Path.GetTempPath(), "idcard-webroot-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(_webRoot, "assets", "images"));

        var environment = new Mock<IWebHostEnvironment>();
        environment.Setup(x => x.WebRootPath).Returns(_webRoot);
        var orgConfig = new Mock<IOrgConfigService>();
        orgConfig.Setup(x => x.GetConfigAsync())
            .ReturnsAsync(new OrgConfigDto { Contact = new ContactDto { PortalBaseUrl = "https://portal.test" } });
        _storage = new Mock<IFileStorageService>();
        _service = new IDCardService(_context, orgConfig.Object, _storage.Object, environment.Object);
    }

    [TearDown]
    public void PhotoTearDown()
    {
        if (Directory.Exists(_webRoot)) Directory.Delete(_webRoot, true);
    }

    private async Task<string> CardFor(string photoPath)
    {
        var member = await CreateAndSaveTestMemberAsync();
        member.PhotoPath = photoPath;
        await _context.SaveChangesAsync();
        return await _service.GenerateIDCardDataUriAsync(member.Id);
    }

    private static string Base64(string text) => Convert.ToBase64String(Encoding.UTF8.GetBytes(text));

    [Test]
    public async Task StoredPhoto_IsReadFromTheStore()
    {
        _storage.Setup(x => x.OpenReadAsync("uploads/members/1/photo/a.jpg", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new StoredFile(new MemoryStream(Encoding.UTF8.GetBytes("stored-photo")), null, null));

        var card = await CardFor("/uploads/members/1/photo/a.jpg");

        DecodedSvg(card).Should().Contain(Base64("stored-photo"));
    }

    [Category("NFR-R4")]
    [Test]
    public async Task SeededAssetPhoto_IsReadFromTheWebRoot()
    {
        await File.WriteAllTextAsync(Path.Combine(_webRoot, "assets", "images", "seed.jpg"), "seed-photo");

        var card = await CardFor("/assets/images/seed.jpg");

        DecodedSvg(card).Should().Contain(Base64("seed-photo"));
    }

    [Category("NFR-S7")]
    [Test]
    public async Task PathThatClimbsOutOfTheWebRoot_IsNotRead()
    {
        var outside = Path.Combine(Path.GetDirectoryName(_webRoot)!, "idcard-outside-" + Guid.NewGuid().ToString("N") + ".txt");
        await File.WriteAllTextAsync(outside, "secret");
        try
        {
            var card = await CardFor("assets/../../" + Path.GetFileName(outside));

            DecodedSvg(card).Should().NotContain(Base64("secret"));
        }
        finally
        {
            File.Delete(outside);
        }
    }

    // The card comes back as an SVG data URI. The photo sits inside it as base64.
    private static string DecodedSvg(string dataUri)
    {
        var comma = dataUri.IndexOf(',');
        var payload = dataUri[(comma + 1)..];
        return dataUri[..comma].EndsWith(";base64")
            ? Encoding.UTF8.GetString(Convert.FromBase64String(payload))
            : Uri.UnescapeDataString(payload);
    }
}

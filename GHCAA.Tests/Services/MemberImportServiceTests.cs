using System.Text.Json;
using ClosedXML.Excel;
using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Services;

// Coverage for the bulk member import — the largest previously-untested unit in the backend.
// Exercises the real ClosedXML path with in-memory workbooks rather than mocking the reader,
// so the header/mapping/validation/de-duplication logic is genuinely executed.
[TestFixture]
public class MemberImportServiceTests : TestBase
{
    private Mock<IFileStorageService> _storage = null!;
    private Mock<IUserService> _userService = null!;
    private Mock<IOrgConfigService> _orgConfig = null!;
    private MemberImportService _service = null!;

    // Matches the hardcoded GHCAA defaults BuildGhcaaDefaults() serves today, so import behavior
    // under test stays the same as before ImportEmailBase/MembershipNumberPrefix moved to org-config.
    private static readonly OrgConfigDto TestOrgConfig = new()
    {
        Branding = new BrandingDto { MembershipNumberPrefix = "GHC-" },
        Contact = new ContactDto { ImportEmailBase = "haragangian" }
    };

    [SetUp]
    public void Setup()
    {
        _storage = new Mock<IFileStorageService>();
        _userService = new Mock<IUserService>();
        _orgConfig = new Mock<IOrgConfigService>();
        _orgConfig.Setup(x => x.GetConfigAsync()).ReturnsAsync(TestOrgConfig);
        _service = new MemberImportService(
            _context, _storage.Object, _userService.Object, new Mock<ILogger<MemberImportService>>().Object, _orgConfig.Object);
    }

    // ---- helpers -------------------------------------------------------------------

    private static IFormFile BuildWorkbook(string[] headers, params string[][] rows)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Sheet1");

        for (var c = 0; c < headers.Length; c++)
            ws.Cell(1, c + 1).Value = headers[c];

        for (var r = 0; r < rows.Length; r++)
            for (var c = 0; c < rows[r].Length; c++)
                ws.Cell(r + 2, c + 1).Value = rows[r][c];

        var ms = new MemoryStream();
        wb.SaveAs(ms);
        ms.Position = 0;
        return new FormFile(ms, 0, ms.Length, "ExcelFile", "members.xlsx");
    }

    private static MemberImportRequestDto Request(
        IFormFile file,
        Dictionary<string, string> mapping,
        Dictionary<string, string>? defaults = null) => new()
        {
            ExcelFile = file,
            ColumnMappingJson = JsonSerializer.Serialize(mapping),
            DefaultValuesJson = JsonSerializer.Serialize(defaults ?? new Dictionary<string, string>())
        };

    private static Dictionary<string, string> StandardMapping() => new()
    {
        ["Name"] = nameof(Member.FullName),
        ["NationalId"] = nameof(Member.NID),
        ["Mail"] = nameof(Member.Email),
        ["Mobile"] = nameof(Member.MobileNo)
    };

    // ---- guard clauses -------------------------------------------------------------

    [Test]
    public async Task ImportMembersAsync_ReturnsError_WhenMappingJsonIsInvalid()
    {
        var request = new MemberImportRequestDto
        {
            ExcelFile = BuildWorkbook(["Name"], ["Someone"]),
            ColumnMappingJson = "{ not valid json",
            DefaultValuesJson = "{}"
        };

        var result = await _service.ImportMembersAsync(request);

        result.SuccessCount.Should().Be(0);
        result.Errors.Should().ContainSingle().Which.Should().Contain("Invalid mapping configuration");
    }

    [Test]
    public async Task ImportMembersAsync_ReturnsError_WhenFileHasOnlyHeaders()
    {
        var request = Request(BuildWorkbook(["Name", "NationalId"]), StandardMapping());

        var result = await _service.ImportMembersAsync(request);

        result.SuccessCount.Should().Be(0);
        result.Errors.Should().ContainSingle().Which.Should().Contain("empty or only contains headers");
    }

    // ---- happy path ----------------------------------------------------------------

    [Test]
    public async Task ImportMembersAsync_InsertsNewMember_FromMappedColumns()
    {
        var request = Request(
            BuildWorkbook(["Name", "NationalId", "Mail", "Mobile"],
                ["Ayesha Rahman", "1990111222", "ayesha@example.com", "01711000111"]),
            StandardMapping());

        var result = await _service.ImportMembersAsync(request);

        result.SuccessCount.Should().Be(1);

        var member = await _context.Members.FirstOrDefaultAsync(m => m.NID == "1990111222");
        member.Should().NotBeNull();
        member!.FullName.Should().Be("Ayesha Rahman");
        member.Email.Should().Be("ayesha@example.com");
        member.MobileNo.Should().Be("01711000111");
        // Imported members are treated as already-vetted, not pending applicants.
        member.Status.Should().Be(Enums.MembershipStatus.Active);
        member.EmailVerified.Should().BeTrue();
    }

    [Test]
    public async Task ImportMembersAsync_CreatesUserAccount_ForEachImportedMember()
    {
        var request = Request(
            BuildWorkbook(["Name", "NationalId", "Mail", "Mobile"],
                ["Ayesha Rahman", "1990111222", "ayesha@example.com", "01711000111"]),
            StandardMapping());

        await _service.ImportMembersAsync(request);

        // Username and initial password are both the NID.
        _userService.Verify(x => x.CreateUserAccountAsync(
            It.IsAny<int>(), "1990111222", "1990111222", It.IsAny<CancellationToken>()), Times.Once);
    }

    // ---- validation & auto-fill ----------------------------------------------------

    [Test]
    public async Task ImportMembersAsync_SkipsRow_WhenFullNameMissing()
    {
        var before = await _context.Members.CountAsync();
        var request = Request(
            BuildWorkbook(["Name", "NationalId", "Mail", "Mobile"],
                ["", "1990111222", "ayesha@example.com", "01711000111"]),
            StandardMapping());

        var result = await _service.ImportMembersAsync(request);

        result.SuccessCount.Should().Be(0);
        result.FailureCount.Should().Be(1);
        result.Errors.Should().Contain(e => e.Contains("Missing Full Name"));
        (await _context.Members.CountAsync()).Should().Be(before, "the invalid row must not be persisted");
    }

    [Test]
    public async Task ImportMembersAsync_AutoFillsRequiredFields_AndReportsEachSubstitution()
    {
        var request = Request(
            BuildWorkbook(["Name", "NationalId", "Mail", "Mobile"],
                ["Ayesha Rahman", "1990111222", "ayesha@example.com", "01711000111"]),
            StandardMapping());

        var result = await _service.ImportMembersAsync(request);

        var member = await _context.Members.FirstAsync(m => m.NID == "1990111222");
        member.FatherName.Should().StartWith(Constants.Defaults.ImportPrefix);
        member.MotherName.Should().StartWith(Constants.Defaults.ImportPrefix);
        member.PresentAddress.Should().Contain(Constants.Defaults.UnknownValue);
        // PermanentAddress is copied from PresentAddress rather than given its own placeholder.
        member.PermanentAddress.Should().Be(member.PresentAddress);
        member.EmergencyContactRelation.Should().Be(Constants.Defaults.UnknownValue);

        // Substitutions are surfaced as warnings even though the row succeeded.
        result.SuccessCount.Should().Be(1);
        result.Errors.Should().Contain(e => e.Contains("FatherName missing"));
        result.Errors.Should().Contain(e => e.Contains("PermanentAddress missing"));
    }

    [Test]
    public async Task ImportMembersAsync_GeneratesTraceableEmail_WhenMissing()
    {
        var request = Request(
            BuildWorkbook(["Name", "NationalId", "Mobile"],
                ["Ayesha Rahman", "1990111222", "01711000111"]),
            new Dictionary<string, string>
            {
                ["Name"] = nameof(Member.FullName),
                ["NationalId"] = nameof(Member.NID),
                ["Mobile"] = nameof(Member.MobileNo)
            });

        var result = await _service.ImportMembersAsync(request);

        var member = await _context.Members.FirstAsync(m => m.NID == "1990111222");
        member.Email.Should().Be($"{TestOrgConfig.Contact.ImportEmailBase}+1990111222@gmail.com");
        result.Errors.Should().Contain(e => e.Contains("Email missing"));
    }

    [Test]
    public async Task ImportMembersAsync_GeneratesPlaceholderNid_WhenMissing()
    {
        var request = Request(
            BuildWorkbook(["Name", "Mail", "Mobile"],
                ["Ayesha Rahman", "ayesha@example.com", "01711000111"]),
            new Dictionary<string, string>
            {
                ["Name"] = nameof(Member.FullName),
                ["Mail"] = nameof(Member.Email),
                ["Mobile"] = nameof(Member.MobileNo)
            });

        var result = await _service.ImportMembersAsync(request);

        result.SuccessCount.Should().Be(1);
        var member = await _context.Members.FirstAsync(m => m.Email == "ayesha@example.com");
        member.NID.Should().StartWith(Constants.Defaults.ImportPrefix);
        result.Errors.Should().Contain(e => e.Contains("NID missing"));
    }

    [Test]
    public async Task ImportMembersAsync_AppliesDefaultValues_OnlyToEmptyFields()
    {
        var request = Request(
            BuildWorkbook(["Name", "NationalId", "Mail", "Mobile", "Size"],
                ["Ayesha Rahman", "1990111222", "ayesha@example.com", "01711000111", ""]),
            new Dictionary<string, string>
            {
                ["Name"] = nameof(Member.FullName),
                ["NationalId"] = nameof(Member.NID),
                ["Mail"] = nameof(Member.Email),
                ["Mobile"] = nameof(Member.MobileNo),
                ["Size"] = nameof(Member.TShirtSize)
            },
            defaults: new Dictionary<string, string>
            {
                [nameof(Member.TShirtSize)] = "L",
                // FullName is already populated from the sheet, so the default must not win.
                [nameof(Member.FullName)] = "SHOULD NOT OVERWRITE"
            });

        await _service.ImportMembersAsync(request);

        var member = await _context.Members.FirstAsync(m => m.NID == "1990111222");
        member.TShirtSize.Should().Be("L");
        member.FullName.Should().Be("Ayesha Rahman");
    }

    // ---- duplicate handling --------------------------------------------------------

    [Test]
    public async Task ImportMembersAsync_SkipsDuplicateNid_WithinTheSameFile()
    {
        var request = Request(
            BuildWorkbook(["Name", "NationalId", "Mail", "Mobile"],
                ["Ayesha Rahman", "1990111222", "ayesha@example.com", "01711000111"],
                ["Ayesha Duplicate", "1990111222", "other@example.com", "01711000222"]),
            StandardMapping());

        var result = await _service.ImportMembersAsync(request);

        result.SuccessCount.Should().Be(1);
        result.FailureCount.Should().Be(1);
        result.Errors.Should().Contain(e => e.Contains("Duplicate NID"));
        (await _context.Members.CountAsync(m => m.NID == "1990111222")).Should().Be(1);
    }

    [Test]
    public async Task ImportMembersAsync_UpdatesExistingMember_WhenNidAlreadyInDatabase()
    {
        var existing = await CreateAndSaveTestMemberAsync("Old Name", "old@example.com", "01700000000", "1990111222");
        DetachAll();

        var request = Request(
            BuildWorkbook(["Name", "NationalId", "Mail", "Mobile"],
                ["Updated Name", "1990111222", "updated@example.com", "01711000111"]),
            StandardMapping());

        var result = await _service.ImportMembersAsync(request);

        result.SuccessCount.Should().Be(1);
        (await _context.Members.CountAsync(m => m.NID == "1990111222"))
            .Should().Be(1, "an existing NID must update in place, not insert a second row");

        var member = await _context.Members.AsNoTracking().FirstAsync(m => m.Id == existing.Id);
        member.FullName.Should().Be("Updated Name");
        member.Email.Should().Be("updated@example.com");
    }

    [Test]
    public async Task ImportMembersAsync_DeDuplicatesEmail_WhenItCollidesWithAnotherRow()
    {
        var request = Request(
            BuildWorkbook(["Name", "NationalId", "Mail", "Mobile"],
                ["First Person", "1990111222", "shared@example.com", "01711000111"],
                ["Second Person", "1990333444", "shared@example.com", "01711000222"]),
            StandardMapping());

        var result = await _service.ImportMembersAsync(request);

        result.SuccessCount.Should().Be(2);
        var emails = await _context.Members.Select(m => m.Email).ToListAsync();
        emails.Should().OnlyHaveUniqueItems("the unique index on Email must not be violated by an import");
    }

    // ---- export --------------------------------------------------------------------

    [Test]
    public async Task ExportMembersToExcelAsync_ProducesAReadableWorkbook()
    {
        await CreateAndSaveTestMemberAsync("Exported Member", "export@example.com", "01712345000", "5550001111");

        var bytes = await _service.ExportMembersToExcelAsync();

        bytes.Should().NotBeNullOrEmpty();
        using var wb = new XLWorkbook(new MemoryStream(bytes));
        var ws = wb.Worksheets.First();
        ws.RangeUsed()!.RowsUsed().Count().Should().BeGreaterThan(1, "a header row plus at least one member");
    }
}

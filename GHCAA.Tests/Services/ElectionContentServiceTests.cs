using System.ComponentModel.DataAnnotations;
using System.Text.Json.Nodes;
using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using static GHCAA.Domain.Enums;

namespace GHCAA.Tests.Services;

// 37.14s. Election pages and form layouts kept in one table, with tokens filled on read.
[TestFixture]
[Category("FR-25")]
public sealed class ElectionContentServiceTests : TestBase
{
    private const string SimpleLayout = """{"sections":[{"name":"Details","kind":"fields","items":["Name"]}]}""";

    private ElectionContentService _service = null!;

    [SetUp]
    public async Task SetUpService()
    {
        var config = new Mock<IOrgConfigService>();
        config.Setup(x => x.GetConfigAsync()).ReturnsAsync(new OrgConfigDto
        {
            Branding = new() { FullName = "Example Alumni Association", ShortName = "EAA" }
        });
        _service = new ElectionContentService(_context, config.Object);
        await ElectionContentSeeder.SyncAsync(_context, NullLogger.Instance);
    }

    private static ElectionContentTokens Tokens() =>
        new("Example Alumni Association", "EAA", "President", "General Secretary",
            ["President", "Member"], ["President", "Member 1", "Member 2"]);

    [Test]
    public void ResolveText_FillsScalarsAndRepeatsPositionLines()
    {
        var text = Tokens().ResolveText("# {{OrgShortName}}\n- {{Positions}}\nSigned: {{Head}} and {{Secretary}}");

        text.Should().Be("# EAA\n- President\n- Member\nSigned: President and General Secretary");
    }

    [Test]
    public void ResolveLayout_RepeatsArrayItemsAndFillsNestedStrings()
    {
        var json = """{"header":["{{OrgFullName}} member no."],"sections":[{"kind":"table","items":["Seat"],"rows":["{{PositionSeats}}"]}],"signatures":["{{Head}}"]}""";

        var resolved = System.Text.Json.JsonSerializer.Deserialize<ElectionFormLayout>(
            Tokens().ResolveLayout(json), new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;

        resolved.Header.Should().Equal("Example Alumni Association member no.");
        resolved.Sections[0].Rows.Should().Equal("President", "Member 1", "Member 2");
        resolved.Signatures.Should().Equal("President");
    }

    [Test]
    public void ResolveText_NoActivePositions_DropsTheRepeatedLine()
    {
        var tokens = Tokens() with { Positions = [] };

        tokens.ResolveText("before\n- {{Positions}}\nafter").Should().Be("before\nafter");
    }

    [Test]
    public void Validate_PageNeedsFormat_FormMustNotHaveOne()
    {
        ElectionContentService.Validate(new UpsertElectionContentDto { Kind = ElectionContentKind.Page, Key = "p", Title = "P", Body = "x" })
            .Should().Be("A page needs a format.");
        ElectionContentService.Validate(new UpsertElectionContentDto { Kind = ElectionContentKind.Form, Format = ElectionPageFormat.Prose, Key = "ER-90", Title = "F", Body = SimpleLayout })
            .Should().Be("A form has no page format.");
    }

    [TestCase("not json", "not valid JSON")]
    [TestCase("""{"sections":[]}""", "at least one section")]
    [TestCase("""{"sections":[{"name":"A","kind":"chart","items":["x"]}]}""", "Section kind must be one of")]
    [TestCase("""{"sections":[{"name":"A","kind":"text","items":[]}]}""", "has no items")]
    [TestCase("""{"sections":[{"name":"A","kind":"table","items":["x"],"blank":51}]}""", "use 0 to 50")]
    public void ValidateLayout_RejectsBadLayouts(string body, string message)
    {
        ElectionContentService.ValidateLayout(body).Should().Contain(message);
    }

    [Test]
    public void ValidateLayout_EverySeededFormIsValid()
    {
        foreach (var form in _context.ElectionContents.Where(x => x.Kind == ElectionContentKind.Form).ToList())
            ElectionContentService.ValidateLayout(form.Body).Should().BeNull(form.Key);
    }

    [Test]
    public async Task Create_StoresAnUpperCaseFormCode_AndRejectsADuplicate()
    {
        var dto = new UpsertElectionContentDto { Kind = ElectionContentKind.Form, Key = " er-90 ", Title = " Extra Form ", Body = SimpleLayout };

        var (created, error) = await _service.CreateAsync(dto, adminId: 7);
        error.Should().BeNull();
        created!.Key.Should().Be("ER-90");
        created.Title.Should().Be("Extra Form");

        var (again, duplicate) = await _service.CreateAsync(new UpsertElectionContentDto
        {
            Kind = ElectionContentKind.Form,
            Key = "Er-90",
            Title = "Other",
            Body = SimpleLayout
        }, adminId: 7);
        again.Should().BeNull();
        duplicate.Should().Be("A form with key ER-90 already exists.");
    }

    // Two admins create the same key at once. Both pass the lookup and the second save hits the unique index.
    [Test]
    public async Task Create_SameKeyAtOnce_ReturnsAlreadyExists()
    {
        var firstSave = new SaveFirst(async () =>
        {
            _context.ElectionContents.Add(new ElectionContent { Kind = ElectionContentKind.Form, Key = "ER-92", Title = "First", Body = SimpleLayout });
            await _context.SaveChangesAsync();
        });
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_context.Database.GetDbConnection())
            .AddInterceptors(firstSave)
            .Options;
        await using var racing = new ApplicationDbContext(options);
        var service = new ElectionContentService(racing, Mock.Of<IOrgConfigService>());

        var (created, error) = await service.CreateAsync(new UpsertElectionContentDto
        {
            Kind = ElectionContentKind.Form,
            Key = "er-92",
            Title = "Second",
            Body = SimpleLayout
        }, adminId: 7);

        created.Should().BeNull();
        error.Should().Be("A form with key ER-92 already exists.");
        racing.ChangeTracker.Entries<ElectionContent>().Should().BeEmpty();
        _context.ElectionContents.Count(x => x.Key == "ER-92").Should().Be(1);
    }

    // A save error that is not a unique clash, such as a dropped connection, must not read as "already exists".
    [Test]
    public async Task Create_OtherSaveError_IsRethrown()
    {
        var failing = new SaveFirst(() => throw new DbUpdateException("connection lost"));
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_context.Database.GetDbConnection())
            .AddInterceptors(failing)
            .Options;
        await using var context = new ApplicationDbContext(options);
        var service = new ElectionContentService(context, Mock.Of<IOrgConfigService>());

        var act = () => service.CreateAsync(new UpsertElectionContentDto
        {
            Kind = ElectionContentKind.Form,
            Key = "ER-93",
            Title = "Lost",
            Body = SimpleLayout
        }, adminId: 7);

        await act.Should().ThrowAsync<DbUpdateException>().WithMessage("connection lost");
    }

    // A page and a form may share a key such as er-25; the kind tells them apart.
    [Test]
    public async Task Create_PageWithAFormsKey_IsAllowed()
    {
        var (page, error) = await _service.CreateAsync(new UpsertElectionContentDto
        {
            Kind = ElectionContentKind.Page,
            Format = ElectionPageFormat.Prose,
            Key = "ER-01",
            Title = "Notes",
            Body = "text"
        }, adminId: 7);

        error.Should().BeNull();
        page!.Kind.Should().Be(ElectionContentKind.Page);
    }

    [Test]
    public async Task Update_KeepsKindAndKey()
    {
        var form = _context.ElectionContents.Single(x => x.Kind == ElectionContentKind.Form && x.Key == "ER-01");

        var (_, renamed) = await _service.UpdateAsync(form.Id, new UpsertElectionContentDto
        {
            Kind = ElectionContentKind.Form,
            Key = "ER-91",
            Title = "x",
            Body = SimpleLayout
        }, adminId: 7);
        var (_, rekinded) = await _service.UpdateAsync(form.Id, new UpsertElectionContentDto
        {
            Kind = ElectionContentKind.Page,
            Format = ElectionPageFormat.Prose,
            Key = "ER-01",
            Title = "x",
            Body = "x"
        }, adminId: 7);

        renamed.Should().Be("Kind and key cannot change.");
        rekinded.Should().Be("Kind and key cannot change.");
    }

    [Test]
    public async Task Update_SavesTheEditAndWhoMadeIt()
    {
        var form = _context.ElectionContents.Single(x => x.Kind == ElectionContentKind.Form && x.Key == "ER-01");

        var (updated, error) = await _service.UpdateAsync(form.Id, new UpsertElectionContentDto
        {
            Kind = ElectionContentKind.Form,
            Key = "er-01",
            Title = "Renamed",
            Body = SimpleLayout,
            IsPublished = false
        }, adminId: 9);

        error.Should().BeNull();
        updated!.Title.Should().Be("Renamed");
        updated.IsPublished.Should().BeFalse();
        _context.ChangeTracker.Clear();
        _context.ElectionContents.Single(x => x.Id == form.Id).UpdatedByAdminId.Should().Be(9);
    }

    [Test]
    public async Task Update_UnknownId_Throws()
    {
        var act = () => _service.UpdateAsync(999_999, new UpsertElectionContentDto { Kind = ElectionContentKind.Form, Key = "ER-01", Title = "x", Body = SimpleLayout }, 1);

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Test]
    public async Task GetPublished_ListLeavesOutBodiesAndUnpublishedRows()
    {
        var form = _context.ElectionContents.Single(x => x.Kind == ElectionContentKind.Form && x.Key == "ER-02");
        form.IsPublished = false;
        await _context.SaveChangesAsync();

        var forms = await _service.GetPublishedAsync(ElectionContentKind.Form);

        forms.Should().HaveCount(49).And.OnlyContain(x => x.Body == null && x.Kind == ElectionContentKind.Form);
        forms.Should().NotContain(x => x.Key == "ER-02");
        (await _service.GetPublishedAsync(ElectionContentKind.Form, "ER-02")).Should().BeNull();
    }

    [Test]
    public async Task GetPublished_OneForm_FillsTokens_AdminReadKeepsThem()
    {
        var published = await _service.GetPublishedAsync(ElectionContentKind.Form, "er-30");
        var raw = await _service.GetByIdAsync(published!.Id);

        published.Body.Should().NotContain("{{");
        raw!.Body.Should().Contain("{{PositionSeats}}");
    }

    [Test]
    public async Task GetAll_IncludesUnpublishedRowsWithoutBodies()
    {
        var form = _context.ElectionContents.Single(x => x.Kind == ElectionContentKind.Form && x.Key == "ER-03");
        form.IsPublished = false;
        await _context.SaveChangesAsync();

        var all = await _service.GetAllAsync();

        all.Should().Contain(x => x.Key == "ER-03" && !x.IsPublished);
        all.Should().OnlyContain(x => x.Body == null);
    }

    [Test]
    public void ResolveLayout_BodyThatIsNotJson_ComesBackUnchanged()
    {
        Tokens().ResolveLayout("{not json").Should().Be("{not json");
    }

    // The JSON enum converter accepts numbers, so DataAnnotations must refuse an unnamed one.
    [TestCase(99, null)]
    [TestCase(0, 99)]
    public void UpsertDto_UnnamedKindOrFormat_FailsValidation(int kind, int? format)
    {
        var dto = new UpsertElectionContentDto
        {
            Key = "rules",
            Kind = (ElectionContentKind)kind,
            Format = (ElectionPageFormat?)format,
            Title = "Rules",
            Body = "text",
        };

        var valid = Validator.TryValidateObject(dto, new ValidationContext(dto), [], validateAllProperties: true);

        valid.Should().BeFalse();
    }

    // One bad entry in forms.json is skipped; the rest still seed, in file order.
    [Test]
    public void ParseForms_SkipsBadEntries_KeepsTheRest()
    {
        var forms = JsonNode.Parse($$"""
            [
              {"code":"ER-01","title":"First","sections":[{"name":"A","kind":"fields","items":["Name"]}]},
              {"title":"No code","sections":[{"name":"A","kind":"fields","items":["Name"]}]},
              {"code":"ER-03","title":"Bad kind","sections":[{"name":"A","kind":"poem","items":["Name"]}]},
              {"code":"ER-04","title":"Fourth","sections":[{"name":"A","kind":"text","items":["Line"]}]}
            ]
            """)!.AsArray();

        var rows = ElectionContentSeeder.ParseForms(forms, "forms.json");

        rows.Select(x => (x.Key, x.DisplayOrder)).Should().Equal(("ER-01", 1), ("ER-04", 4));
        rows[0].Body.Should().NotContain("\"code\"").And.NotContain("\"title\"");
    }

    // Runs the other admin's save once, just before this context saves.
    private sealed class SaveFirst(Func<Task> otherSave) : SaveChangesInterceptor
    {
        private bool _done;

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            if (!_done)
            {
                _done = true;
                await otherSave();
            }
            return result;
        }
    }
}

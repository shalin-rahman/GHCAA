using System.Reflection;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Infrastructure.Services;

namespace GHCAA.Tests.OrgConfig;

/// <summary>
/// Work Package 62.3/62.4. `profiles/ghc/org-config.json` is the regression baseline for the
/// white-label work: 62.6 replaces OrgConfigService.BuildGhcaaDefaults() with a read of this pack,
/// and the swap is only safe if the pack reproduces the hardcoded defaults exactly.
///
/// The pack is generated from BuildGhcaaDefaults() rather than typed by hand, because "verbatim"
/// across ~190 lines of nested records is not something hand-transcription can promise. Regenerate
/// deliberately with the [Explicit] test below; the ordinary test asserts the file on disk still
/// matches the code, and is what fails if either side drifts before 62.6 lands.
/// </summary>
[TestFixture]
public class GhcProfilePackTests
{
    // Matches profiles/default/org-config.json's existing style: PascalCase, indented.
    // InstitutionProfileProvider reads with PropertyNameCaseInsensitive, so casing is
    // presentation only — chosen to match the pack already in the repository.
    private static readonly JsonSerializerOptions PackFormat = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        // The GHC pack carries "৳" and Bengali locale strings. Without this the default
        // encoder escapes them to \uXXXX, which round-trips correctly but makes the pack
        // unreadable to the person who has to edit it for a second institution.
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static OrgConfigDto BuildGhcaaDefaults()
    {
        // Private static on OrgConfigService. Reflected rather than made internal, so this
        // test cannot be the reason production visibility widens.
        var method = typeof(OrgConfigService).GetMethod(
            "BuildGhcaaDefaults", BindingFlags.NonPublic | BindingFlags.Static);
        method.Should().NotBeNull(
            "OrgConfigService.BuildGhcaaDefaults() is the source of the GHC pack; if it was "
            + "renamed or removed, this test and Work Package 62.6 both need revisiting");
        return (OrgConfigDto)method!.Invoke(null, null)!;
    }

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "profiles")))
            dir = dir.Parent;
        dir.Should().NotBeNull("the repository root holding profiles/ should be above the test binary");
        return dir!.FullName;
    }

    private static string PackPath() => Path.Combine(RepoRoot(), "profiles", "ghc", "org-config.json");

    [Test]
    public void GhcPack_ReproducesTheHardcodedDefaults_Exactly()
    {
        File.Exists(PackPath()).Should().BeTrue(
            $"profiles/ghc/org-config.json is Work Package 62.3's deliverable; expected it at {PackPath()}");

        var fromPack = JsonSerializer.Deserialize<OrgConfigDto>(
            File.ReadAllText(PackPath()),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        // Compared through the serializer rather than by reference so nested records, lists and
        // dictionaries are all covered without asserting field by field — the point is that no
        // value differs anywhere, not that a chosen subset matches.
        JsonSerializer.Serialize(fromPack, PackFormat)
            .Should().Be(JsonSerializer.Serialize(BuildGhcaaDefaults(), PackFormat),
                "the GHC pack is the regression baseline for 62.6's swap; if this fails, either "
                + "BuildGhcaaDefaults() changed without the pack being regenerated, or the pack "
                + "was hand-edited. Regenerate with the Explicit test in this fixture.");
    }

    [Test]
    public void GhcPack_DeclaresTheGhcOrgId()
    {
        // Cheap guard against the pack being overwritten by a regeneration pointed at the wrong
        // profile — the failure mode that would make 62.6 silently ship the neutral sample values.
        var fromPack = JsonSerializer.Deserialize<OrgConfigDto>(
            File.ReadAllText(PackPath()),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        fromPack!.OrgId.Should().Be("ghcaa");
    }

    [Test, Explicit("Writes profiles/ghc/org-config.json. Run deliberately, not as part of the suite.")]
    public void Regenerate_GhcPack_FromCode()
    {
        var path = PackPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(BuildGhcaaDefaults(), PackFormat) + "\n");
        TestContext.Out.WriteLine($"wrote {path}");
    }
}

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;

namespace GHCAA.Tests.OrgConfig;

/// <summary>
/// Work Package 62.6 — the profile pack driving live configuration, and the guard that stops it
/// doing so before anyone has said which institution this deployment serves.
///
/// The plan's exit criterion for Phase A is that ORG_PROFILE=ghc produces a byte-identical
/// GET /api/config response to the hardcoded defaults. That is asserted here against the same
/// frozen golden file OrgConfigGoldenSnapshotTests uses, so both routes are measured against one
/// fixed artefact rather than against each other.
///
/// The second case is the one that matters operationally: ORG_PROFILE unset must NOT serve
/// profiles/default/ (the neutral "Sample Alumni Association" pack). Nothing sets ORG_PROFILE in
/// this repository — not the Dockerfile, not a workflow, not appsettings — so an unguarded swap
/// would have rebranded a live association with real members and started issuing MEM- numbers.
/// </summary>
[TestFixture]
public class ProfileDrivenConfigTests : TestBase
{
    private static readonly JsonSerializerOptions GoldenFormat = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private sealed class StubProfile(string name, bool explicitlySelected, OrgConfigDto defaults)
        : IInstitutionProfileProvider
    {
        public string ProfileName { get; } = name;
        public bool ProfileExplicitlySelected { get; } = explicitlySelected;
        public OrgConfigDto OrgConfigDefaults { get; } = defaults;
    }

    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "profiles")))
            dir = dir.Parent;
        dir.Should().NotBeNull();
        return dir!.FullName;
    }

    private static OrgConfigDto PackFromDisk(string profileName)
    {
        var path = Path.Combine(RepoRoot(), "profiles", profileName, "org-config.json");
        File.Exists(path).Should().BeTrue($"expected a profile pack at {path}");
        return JsonSerializer.Deserialize<OrgConfigDto>(
            File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    private static string Golden() =>
        File.ReadAllText(Path.Combine(RepoRoot(), "GHCAA.Tests", "OrgConfig", "golden",
            "org-config-ghc.golden.json")).Replace("\r\n", "\n").TrimEnd('\n');

    private async Task<string> ConfigJsonAsync(IInstitutionProfileProvider? profile)
    {
        var service = new OrgConfigService(_context, new MemoryCache(new MemoryCacheOptions()), profile);
        return JsonSerializer.Serialize(await service.GetConfigAsync(), GoldenFormat).Replace("\r\n", "\n");
    }

    [Test]
    public async Task OrgProfileGhc_ReproducesTheGoldenSnapshot_ByteIdentically()
    {
        // Phase A's stated exit criterion, from docs/WHITE_LABEL_PLAN.md.
        var ghc = new StubProfile("ghc", explicitlySelected: true, PackFromDisk("ghc"));

        (await ConfigJsonAsync(ghc)).Should().Be(Golden(),
            "reading profiles/ghc/org-config.json must produce exactly what the hardcoded defaults "
            + "produced, or 62.6's swap is not safe to complete");
    }

    [Test]
    public async Task OrgProfileUnset_KeepsHardcodedDefaults_AndDoesNotServeTheSamplePack()
    {
        // The provider still resolves a pack when ORG_PROFILE is unset — it just resolves the
        // neutral one. Handing that to the service and still getting GHC back is the guard working.
        var unset = new StubProfile("default", explicitlySelected: false, PackFromDisk("default"));

        (await ConfigJsonAsync(unset)).Should().Be(Golden(),
            "an unset ORG_PROFILE must not rebrand a live deployment to the sample pack");
    }

    [Test]
    public async Task NoProfileProviderAtAll_KeepsHardcodedDefaults()
    {
        // The 8 pre-existing OrgConfigService tests construct it without a provider; that path has
        // to keep working and keep meaning "use the code defaults".
        (await ConfigJsonAsync(null)).Should().Be(Golden());
    }

    [Test]
    public async Task ExplicitlySelectedProfile_ActuallyDrivesTheOutput()
    {
        // Proves the selected branch is live rather than the guard swallowing everything — without
        // this, the two tests above would pass even if the pack were never read at all.
        var sample = new StubProfile("default", explicitlySelected: true, PackFromDisk("default"));

        var json = await ConfigJsonAsync(sample);

        json.Should().NotBe(Golden());
        json.Should().Contain("Sample Alumni Association");
        json.Should().Contain("MEM-");
    }
}

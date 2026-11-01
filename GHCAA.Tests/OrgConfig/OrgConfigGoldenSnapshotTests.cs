using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GHCAA.Tests.OrgConfig;

/// <summary>
/// Work Package 62.4 — the golden config snapshot, and the safety net the rest of Work Package 62
/// depends on.
///
/// The snapshot in golden/org-config-ghc.golden.json was captured from the real OrgConfigService
/// against an empty database on 2026-09-04, while the GHC values were still hardcoded. 62.6b
/// deleted that code, so the service now reads profiles/ghc/org-config.json, and this test checks
/// the result against the frozen file. A pack that is subtly wrong (a dropped locale, a renamed
/// key, a reordered list) fails here.
///
/// Regenerating the golden defeats the point. If this test fails, the question is what changed in
/// the config, not how to make the file match. Regenerate only when a config change is intended,
/// reviewed, and recorded against a tracker item.
/// </summary>
[TestFixture]
public class OrgConfigGoldenSnapshotTests : TestBase
{
    private static readonly JsonSerializerOptions GoldenFormat = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static string GoldenPath()
    {
        var dir = new DirectoryInfo(TestContext.CurrentContext.TestDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "profiles")))
            dir = dir.Parent;
        dir.Should().NotBeNull("expected the repository root, which holds profiles/, above the test binary");
        return Path.Combine(dir!.FullName, "GHCAA.Tests", "OrgConfig", "golden", "org-config-ghc.golden.json");
    }

    // With no OrganizationConfig row, GetConfigAsync serves the pack values unchanged.
    private async Task<OrgConfigDto> ConfigFromRealServiceAsync()
    {
        var service = new OrgConfigService(_context, new MemoryCache(new MemoryCacheOptions()), TestProfiles.Ghc);
        return await service.GetConfigAsync();
    }

    [Test]
    public async Task GetConfigAsync_OnEmptyDatabase_MatchesTheFrozenGoldenSnapshot()
    {
        File.Exists(GoldenPath()).Should().BeTrue($"the 62.4 golden snapshot should exist at {GoldenPath()}");

        var actual = JsonSerializer.Serialize(await ConfigFromRealServiceAsync(), GoldenFormat);

        actual.Replace("\r\n", "\n").Should().Be(
            File.ReadAllText(GoldenPath()).Replace("\r\n", "\n").TrimEnd('\n'),
            "this snapshot was frozen while the GHC defaults were still hardcoded. A difference "
            + "means the running configuration changed. Find out what changed rather than "
            + "regenerating the golden to match.");
    }

    [Test, Explicit("Overwrites the frozen 62.4 golden snapshot. Only for an intended, reviewed config change.")]
    public async Task Regenerate_GoldenSnapshot()
    {
        var path = GoldenPath();
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, JsonSerializer.Serialize(await ConfigFromRealServiceAsync(), GoldenFormat));
        TestContext.Out.WriteLine($"wrote {path}");
    }
}

using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;

namespace GHCAA.Tests.OrgConfig;

/// <summary>
/// Work Package 62.4 — the golden config snapshot, and the safety net the rest of Work Package 62
/// depends on.
///
/// The snapshot in golden/org-config-ghc.golden.json was captured from the real OrgConfigService
/// against an empty database on 2026-09-04, BEFORE 62.6 replaces BuildGhcaaDefaults() with a read
/// of profiles/ghc/org-config.json. Its whole purpose is to be frozen: after that swap, this test
/// compares the running service against what the service produced while the values were still
/// hardcoded. A pack that is subtly wrong — a dropped locale, a renamed key, a list that reordered —
/// fails here.
///
/// That is a different guarantee from GhcProfilePackTests, which compares the pack against
/// BuildGhcaaDefaults(). Those two move together once 62.6 lands and would agree with each other
/// even if both were wrong. This file does not move.
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

    // The empty database is the case that matters: with no OrganizationConfig row, GetConfigAsync
    // falls through to the code defaults, which is exactly the path 62.6 rewrites.
    private async Task<OrgConfigDto> ConfigFromRealServiceAsync()
    {
        var service = new OrgConfigService(_context, new MemoryCache(new MemoryCacheOptions()));
        return await service.GetConfigAsync();
    }

    [Test]
    public async Task GetConfigAsync_OnEmptyDatabase_MatchesTheFrozenGoldenSnapshot()
    {
        File.Exists(GoldenPath()).Should().BeTrue($"the 62.4 golden snapshot should exist at {GoldenPath()}");

        var actual = JsonSerializer.Serialize(await ConfigFromRealServiceAsync(), GoldenFormat);

        actual.Replace("\r\n", "\n").Should().Be(
            File.ReadAllText(GoldenPath()).Replace("\r\n", "\n").TrimEnd('\n'),
            "this snapshot was frozen before Work Package 62.6 swapped the hardcoded defaults for a "
            + "profile-pack read. A difference means the running configuration changed — investigate "
            + "what changed rather than regenerating the golden to match.");
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

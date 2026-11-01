using FluentAssertions;
using GHCAA.Application.Interfaces;
using GHCAA.Infrastructure.Services;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GHCAA.Tests.OrgConfig;

/// <summary>
/// Work Package 62.6 and 62.6b. The profile pack is the only source of the live defaults.
///
/// OrgConfigGoldenSnapshotTests checks that the GHC pack gives the frozen golden output. This
/// checks the other direction: a different pack gives different output. An unset ORG_PROFILE
/// never reaches this service, because InstitutionProfileProvider throws at boot (see
/// InstitutionProfileProviderTests).
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

    private static string Golden() =>
        File.ReadAllText(Path.Combine(TestProfiles.RepoRoot(), "GHCAA.Tests", "OrgConfig", "golden",
            "org-config-ghc.golden.json")).Replace("\r\n", "\n").TrimEnd('\n');

    private async Task<string> ConfigJsonAsync(IInstitutionProfileProvider profile)
    {
        var service = new OrgConfigService(_context, new MemoryCache(new MemoryCacheOptions()), profile);
        return JsonSerializer.Serialize(await service.GetConfigAsync(), GoldenFormat).Replace("\r\n", "\n");
    }

    [Test]
    public async Task DefaultProfile_DrivesTheOutput()
    {
        // Without this the golden test would also pass if the service ignored the pack it was given.
        var json = await ConfigJsonAsync(TestProfiles.FromPack("default"));

        json.Should().NotBe(Golden());
        json.Should().Contain("Sample Alumni Association");
        json.Should().Contain("MEM-");
    }
}

using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using System.Text.Json;

namespace GHCAA.Tests.OrgConfig;

/// <summary>
/// OrgConfigService has no code defaults since 62.6b, so every test that builds one needs a
/// profile. These read the real packs under profiles/ instead of copying them into fixtures.
/// </summary>
public static class TestProfiles
{
    private sealed class PackProfile(string name, OrgConfigDto defaults) : IInstitutionProfileProvider
    {
        public string ProfileName { get; } = name;
        public OrgConfigDto OrgConfigDefaults { get; } = defaults;
    }

    // Read fresh each time. The DTO holds mutable lists, and a shared copy would let one test's
    // edit leak into the next.
    public static IInstitutionProfileProvider Ghc => FromPack("ghc");

    public static IInstitutionProfileProvider FromPack(string profileName) =>
        new PackProfile(profileName, PackFromDisk(profileName));

    public static OrgConfigDto PackFromDisk(string profileName)
    {
        var path = Path.Combine(RepoRoot(), "profiles", profileName, "org-config.json");
        if (!File.Exists(path))
            throw new FileNotFoundException($"Expected a profile pack at {path}.", path);
        return JsonSerializer.Deserialize<OrgConfigDto>(
            File.ReadAllText(path),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
    }

    public static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "profiles")))
            dir = dir.Parent;
        return dir?.FullName
            ?? throw new DirectoryNotFoundException("No profiles/ folder above the test binary.");
    }
}

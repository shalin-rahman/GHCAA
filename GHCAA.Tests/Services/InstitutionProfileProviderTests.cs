using System;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;
using GHCAA.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Services;

// Coverage for docs/TODO.md 62.1: IInstitutionProfileProvider is resolved eagerly at boot
// (Program.cs), so a broken profile pack must fail loudly rather than surface lazily.
[TestFixture]
public class InstitutionProfileProviderTests
{
    private string _root = null!;

    [SetUp]
    public void Setup()
    {
        _root = Path.Combine(Path.GetTempPath(), "ghcaa-profile-test-" + Guid.NewGuid());
        Directory.CreateDirectory(_root);
    }

    [TearDown]
    public void TearDown()
    {
        try { Directory.Delete(_root, recursive: true); } catch { /* best-effort cleanup */ }
    }

    private static IConfiguration ConfigWithProfile(string? orgProfile) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(orgProfile == null
                ? new Dictionary<string, string?>()
                : new Dictionary<string, string?> { ["ORG_PROFILE"] = orgProfile })
            .Build();

    private IHostEnvironment EnvironmentAt(string contentRoot)
    {
        var env = new Mock<IHostEnvironment>();
        env.Setup(e => e.ContentRootPath).Returns(contentRoot);
        return env.Object;
    }

    [Test]
    public void MissingOrgProfile_DefaultsToDefaultProfile()
    {
        Directory.CreateDirectory(Path.Combine(_root, "profiles", "default"));
        File.WriteAllText(Path.Combine(_root, "profiles", "default", "org-config.json"), "{}");

        var provider = new InstitutionProfileProvider(ConfigWithProfile(null), EnvironmentAt(_root));

        provider.ProfileName.Should().Be("default");
    }

    [Test]
    public void NamedProfile_FoundDirectly_IsUsedWithoutFallingBackToDefault()
    {
        Directory.CreateDirectory(Path.Combine(_root, "profiles", "ghc"));
        File.WriteAllText(Path.Combine(_root, "profiles", "ghc", "org-config.json"),
            """{ "Branding": { "ShortName": "GHC Alumni" } }""");

        var provider = new InstitutionProfileProvider(ConfigWithProfile("ghc"), EnvironmentAt(_root));

        provider.ProfileName.Should().Be("ghc");
        provider.OrgConfigDefaults.Branding.ShortName.Should().Be("GHC Alumni");
    }

    [Test]
    public void NamedProfileMissingFile_FallsBackToDefaultProfilesFile()
    {
        Directory.CreateDirectory(Path.Combine(_root, "profiles", "acme"));
        // acme has no org-config.json of its own
        Directory.CreateDirectory(Path.Combine(_root, "profiles", "default"));
        File.WriteAllText(Path.Combine(_root, "profiles", "default", "org-config.json"),
            """{ "Branding": { "ShortName": "Fallback Name" } }""");

        var provider = new InstitutionProfileProvider(ConfigWithProfile("acme"), EnvironmentAt(_root));

        provider.ProfileName.Should().Be("acme"); // the requested name, even though the file came from default
        provider.OrgConfigDefaults.Branding.ShortName.Should().Be("Fallback Name");
    }

    [Test]
    public void NeitherNamedNorDefaultProfileHasTheFile_ThrowsAReadableError()
    {
        var act = () => new InstitutionProfileProvider(ConfigWithProfile("nonexistent"), EnvironmentAt(_root));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*nonexistent*")
            .Which.Message.Should().Contain("default");
    }

    [Test]
    public void ProfilesFolder_OneLevelAboveContentRoot_IsAlsoFound()
    {
        // Mirrors a local `dotnet run` from GHCAA.API/, whose content root sits one directory
        // below the repo root that actually holds profiles/.
        var apiLikeContentRoot = Path.Combine(_root, "GHCAA.API");
        Directory.CreateDirectory(apiLikeContentRoot);
        Directory.CreateDirectory(Path.Combine(_root, "profiles", "default"));
        File.WriteAllText(Path.Combine(_root, "profiles", "default", "org-config.json"), "{}");

        var provider = new InstitutionProfileProvider(ConfigWithProfile(null), EnvironmentAt(apiLikeContentRoot));

        provider.ProfileName.Should().Be("default");
    }
}

using FluentAssertions;

namespace GHCAA.Tests.OrgConfig;

/// <summary>
/// Work Package 62.3/62.4. profiles/ghc/org-config.json was generated from the old hardcoded
/// defaults, which 62.6b deleted. The pack is now the source, and the golden file in
/// OrgConfigGoldenSnapshotTests is what catches an accidental edit to it.
/// </summary>
[TestFixture]
public class GhcProfilePackTests
{
    [Test]
    public void GhcPack_DeclaresTheGhcOrgId()
    {
        // Guards against the GHC pack being overwritten with another profile's values.
        TestProfiles.PackFromDisk("ghc").OrgId.Should().Be("ghcaa");
    }
}

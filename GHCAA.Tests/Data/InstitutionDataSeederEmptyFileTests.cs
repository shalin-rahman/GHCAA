using FluentAssertions;
using GHCAA.Infrastructure.Data;
using NUnit.Framework;

namespace GHCAA.Tests.Data
{
    // The unregistered-file warning skips files holding only "[]". Anything else, including a file
    // that fails to parse, still has to warn so a real data file is never dropped quietly.
    [TestFixture]
    public class InstitutionDataSeederEmptyFileTests
    {
        private string _path = null!;

        [SetUp]
        public void SetUp() => _path = Path.Combine(Path.GetTempPath(), $"seed-{Guid.NewGuid():N}.json");

        [TearDown]
        public void TearDown()
        {
            if (File.Exists(_path)) File.Delete(_path);
        }

        [TestCase("[]")]
        [TestCase("  [ ]\n")]
        public void IsEmptyJsonArray_EmptyArray_ReturnsTrue(string content)
        {
            File.WriteAllText(_path, content);
            InstitutionDataSeeder.IsEmptyJsonArray(_path).Should().BeTrue();
        }

        [TestCase("[{\"Id\":1}]")]
        [TestCase("{}")]
        [TestCase("not json")]
        [TestCase("")]
        public void IsEmptyJsonArray_AnythingElse_ReturnsFalse(string content)
        {
            File.WriteAllText(_path, content);
            InstitutionDataSeeder.IsEmptyJsonArray(_path).Should().BeFalse();
        }
    }
}

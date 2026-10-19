using Amazon.S3;
using FluentAssertions;
using GHCAA.Application.Interfaces;
using GHCAA.Infrastructure;
using GHCAA.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GHCAA.Tests.Services;

// FileStorage:Provider decides where uploads live. Render wipes the container disk on redeploy,
// so a bad setting must stop startup rather than quietly fall back to the disk.
[TestFixture]
public class FileStorageRegistrationTests
{
    private static IServiceCollection Register(Dictionary<string, string?> settings)
    {
        settings["DatabaseProvider"] = "Sqlite";
        var config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();
        return new ServiceCollection().AddInfrastructure(config);
    }

    private static Type? StorageImplementation(IServiceCollection services) =>
        services.Last(d => d.ServiceType == typeof(IFileStorageService)).ImplementationType;

    [Test]
    public void NoProvider_UsesTheLocalDisk()
    {
        var services = Register(new());

        StorageImplementation(services).Should().Be(typeof(LocalFileStorageService));
        services.Should().NotContain(d => d.ServiceType == typeof(IAmazonS3));
    }

    [TestCase("S3")]
    [TestCase("s3")]
    public void S3Provider_UsesTheBucket(string provider)
    {
        var services = Register(new()
        {
            ["FileStorage:Provider"] = provider,
            ["FileStorage:S3:BucketName"] = "test-bucket"
        });

        StorageImplementation(services).Should().Be(typeof(S3FileStorageService));
        services.Should().ContainSingle(d => d.ServiceType == typeof(IAmazonS3));
    }

    [Test]
    public void S3Provider_WithoutBucket_StopsStartup()
    {
        var act = () => Register(new() { ["FileStorage:Provider"] = "S3" });

        act.Should().Throw<InvalidOperationException>().WithMessage("*BucketName*");
    }

    [Test]
    public void UnknownProvider_StopsStartup()
    {
        var act = () => Register(new() { ["FileStorage:Provider"] = "S4" });

        act.Should().Throw<InvalidOperationException>().WithMessage("*'S4' is not supported*");
    }
}

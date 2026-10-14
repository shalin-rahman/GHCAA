using FluentAssertions;
using GHCAA.Application.Interfaces;
using GHCAA.Infrastructure.Services;

namespace GHCAA.Tests.Services
{
    // PagingExtensions is internal to Infrastructure, so Map is reached by reflection.
    [TestFixture]
    [Category("NFR-P1")]
    public class PagingExtensionsTests
    {
        [Test]
        public void Map_CarriesEveryPagingField()
        {
            var source = new PagedResult<int> { Items = [1, 2], TotalItems = 5, TotalPages = 3, Page = 2, PageSize = 2, NextCursor = "abc" };
            var map = typeof(JobHubService).Assembly
                .GetType("GHCAA.Infrastructure.Services.PagingExtensions", throwOnError: true)!
                .GetMethod("Map")!
                .MakeGenericMethod(typeof(int), typeof(string));

            var mapped = (PagedResult<string>)map.Invoke(null, [source, (Func<int, string>)(i => $"#{i}")])!;

            mapped.Items.Should().Equal("#1", "#2");
            mapped.Should().BeEquivalentTo(source, o => o.Excluding(r => r.Items));
        }
    }
}

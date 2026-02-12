using FluentAssertions;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GHCAA.Tests.Services;

[TestFixture]
public class NewsServiceTests
{
    private ApplicationDbContext _context = null!;
    private Microsoft.Data.Sqlite.SqliteConnection _connection = null!;
    private NewsService _service = null!;

    [SetUp]
    public void Setup()
    {
        _connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();

        _service = new NewsService(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
        _connection.Close();
    }

    [Test]
    public async Task GetActiveNewsAsync_ShouldOnlyReturnActivePosts()
    {
        // Arrange
        _context.NewsPosts.AddRange(new List<NewsPost>
        {
            new NewsPost { Title = "Active 1", Content = "C1", IsActive = true, AuthorId = 1, Category = Enums.NewsCategory.News },
            new NewsPost { Title = "Active 2", Content = "C2", IsActive = true, AuthorId = 1, Category = Enums.NewsCategory.OrganisationalUpdate },
            new NewsPost { Title = "Inactive", Content = "C3", IsActive = false, AuthorId = 1, Category = Enums.NewsCategory.BusinessInformation }
        });
        await _context.SaveChangesAsync();

        // Act
        var results = await _service.GetActiveNewsAsync();

        // Assert
        results.Should().HaveCount(2);
        results.All(n => n.IsActive).Should().BeTrue();
    }
}

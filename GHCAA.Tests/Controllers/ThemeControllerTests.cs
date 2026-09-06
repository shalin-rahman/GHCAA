using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GHCAA.API.Controllers;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Controllers
{
    [TestFixture]
    public class ThemeControllerTests
    {
        private Mock<IThemeService> _themeServiceMock;
        private ThemeController _controller;

        private SpecialDayTheme MakeTheme(int id = 1) => new SpecialDayTheme
        {
            Id = id,
            Title = "Eid Theme",
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(1),
            BackgroundColor = "#1a2a3a",
            TextColor = "#fdf51c",
            AnnouncementText = "ঈদ মোবারক!",
            AnimationStyle = "Fade",
            ImageUrl = "",
            SidebarColor = "#2f3e46",
            EnableGradientFading = true,
            IsEnabled = true
        };

        [SetUp]
        public void Setup()
        {
            _themeServiceMock = new Mock<IThemeService>();
            _controller = new ThemeController(_themeServiceMock.Object);
        }

        [Category("FR-47")]
        [Test]
        public async Task GetActiveTheme_WhenThemeExists_ReturnsOk()
        {
            _themeServiceMock.Setup(s => s.GetActiveThemeAsync()).ReturnsAsync(MakeTheme());

            var result = await _controller.GetActiveTheme();

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var ok = result.Result as OkObjectResult;
            var theme = ok!.Value as SpecialDayTheme;
            Assert.That(theme, Is.Not.Null);
            Assert.That(theme!.AnimationStyle, Is.EqualTo("Fade"));
            Assert.That(theme.AnnouncementText, Is.EqualTo("ঈদ মোবারক!"));
        }

        [Test]
        public async Task GetActiveTheme_WhenNoActive_ReturnsNoContent()
        {
            _themeServiceMock.Setup(s => s.GetActiveThemeAsync()).ReturnsAsync((SpecialDayTheme?)null);

            var result = await _controller.GetActiveTheme();

            Assert.That(result.Result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task GetAllThemes_ReturnsOkWithList()
        {
            _themeServiceMock.Setup(s => s.GetAllThemesAsync())
                             .ReturnsAsync(new List<SpecialDayTheme> { MakeTheme(1), MakeTheme(2) });

            var result = await _controller.GetAllThemes();

            Assert.That(result.Result, Is.InstanceOf<OkObjectResult>());
            var ok = result.Result as OkObjectResult;
            var list = ok!.Value as List<SpecialDayTheme>;
            Assert.That(list, Has.Count.EqualTo(2));
        }

        [Category("FR-47")]
        [Test]
        public async Task CreateTheme_ReturnsCreatedWithNewTheme()
        {
            var input = MakeTheme(0);
            var saved = MakeTheme(5);
            _themeServiceMock.Setup(s => s.CreateThemeAsync(input)).ReturnsAsync(saved);

            var result = await _controller.CreateTheme(input);

            Assert.That(result.Result, Is.InstanceOf<CreatedAtActionResult>());
            var created = result.Result as CreatedAtActionResult;
            var theme = created!.Value as SpecialDayTheme;
            Assert.That(theme!.Id, Is.EqualTo(5));
        }

        [Test]
        public async Task UpdateTheme_WithMatchingId_ReturnsNoContent()
        {
            var theme = MakeTheme(3);
            _themeServiceMock.Setup(s => s.UpdateThemeAsync(theme)).Returns(Task.CompletedTask);

            var result = await _controller.UpdateTheme(3, theme);

            Assert.That(result, Is.InstanceOf<NoContentResult>());
        }

        [Test]
        public async Task UpdateTheme_WithMismatchedId_ReturnsBadRequest()
        {
            var theme = MakeTheme(3);

            var result = await _controller.UpdateTheme(99, theme);

            // 82.4: a mismatched id now returns Problem()'s ObjectResult, not a bare BadRequestResult.
            Assert.That(result, Is.InstanceOf<ObjectResult>());
            Assert.That(((ObjectResult)result!).StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task DeleteTheme_CallsServiceAndReturnsNoContent()
        {
            _themeServiceMock.Setup(s => s.DeleteThemeAsync(1)).Returns(Task.CompletedTask);

            var result = await _controller.DeleteTheme(1);

            Assert.That(result, Is.InstanceOf<NoContentResult>());
            _themeServiceMock.Verify(s => s.DeleteThemeAsync(1), Times.Once);
        }

        [Test]
        public void Theme_WithGradientAndSidebarColor_HasExpectedProps()
        {
            var theme = MakeTheme();
            Assert.That(theme.EnableGradientFading, Is.True);
            Assert.That(theme.SidebarColor, Is.EqualTo("#2f3e46"));
        }
    }
}

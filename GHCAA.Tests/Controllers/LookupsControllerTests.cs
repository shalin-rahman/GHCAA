using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.API.Controllers;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;

namespace GHCAA.Tests.Controllers
{
    // LookupsController controls dropdown/lookup master data with no prior coverage (47.13.2).
    // One success + one failure test per action, covering the full controller (not just CRUD).
    [TestFixture]
    public class LookupsControllerTests
    {
        private Mock<ILookupService> _lookupServiceMock;
        private Mock<IMemberService> _memberServiceMock;
        private LookupsController _controller;

        [SetUp]
        public void Setup()
        {
            _lookupServiceMock = new Mock<ILookupService>();
            _memberServiceMock = new Mock<IMemberService>();
            _controller = new LookupsController(_lookupServiceMock.Object, _memberServiceMock.Object);
        }

        [Test]
        public async Task GetPublicStats_ReturnsOk_WithStats()
        {
            object stats = new { TotalMembers = 100 };
            _memberServiceMock.Setup(s => s.GetPublicStatsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(stats);

            var result = await _controller.GetPublicStats(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            Assert.That(((OkObjectResult)result).Value, Is.EqualTo(stats));
        }

        [Test]
        public async Task GetPublicStats_ReturnsOk_WhenServiceReturnsNull()
        {
            _memberServiceMock.Setup(s => s.GetPublicStatsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(default(object));

            var result = await _controller.GetPublicStats(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            Assert.That(((OkObjectResult)result).Value, Is.Null);
        }

        [Test]
        public async Task GetAllLookups_ReturnsOk_WithGroupedLookups()
        {
            var lookups = new Dictionary<string, IEnumerable<LookupDto>>
            {
                { "Degree", new List<LookupDto>() }
            };
            _lookupServiceMock.Setup(s => s.GetAllLookupsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(lookups);

            var result = await _controller.GetAllLookups(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            Assert.That(((OkObjectResult)result).Value, Is.EqualTo(lookups));
        }

        [Test]
        public async Task GetAllLookups_ReturnsOk_WhenNoGroupsExist()
        {
            var empty = new Dictionary<string, IEnumerable<LookupDto>>();
            _lookupServiceMock.Setup(s => s.GetAllLookupsAsync(It.IsAny<CancellationToken>())).ReturnsAsync(empty);

            var result = await _controller.GetAllLookups(CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            Assert.That(((OkObjectResult)result).Value, Is.EqualTo(empty));
        }

        [Test]
        public async Task GetByGroup_ReturnsOk_WithItems()
        {
            var items = new List<LookupDto> { new LookupDto() };
            _lookupServiceMock.Setup(s => s.GetByGroupAsync("Degree", It.IsAny<CancellationToken>())).ReturnsAsync(items);

            var result = await _controller.GetByGroup("Degree", CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            Assert.That(((OkObjectResult)result).Value, Is.EqualTo(items));
        }

        [Test]
        public async Task GetByGroup_ReturnsOk_WithEmptyList_WhenGroupUnknown()
        {
            var empty = new List<LookupDto>();
            _lookupServiceMock.Setup(s => s.GetByGroupAsync("NoSuchGroup", It.IsAny<CancellationToken>())).ReturnsAsync(empty);

            var result = await _controller.GetByGroup("NoSuchGroup", CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
            Assert.That(((OkObjectResult)result).Value, Is.EqualTo(empty));
        }

        [Test]
        public async Task CreateLookup_ReturnsCreatedAtAction_OnSuccess()
        {
            var item = new LookupItem { LookupGroup = "Degree", Value = "BSc" };
            var created = new LookupItem { Id = 1, LookupGroup = "Degree", Value = "BSc" };
            _lookupServiceMock.Setup(s => s.AddLookupItemAsync(item, It.IsAny<CancellationToken>())).ReturnsAsync(created);

            var result = await _controller.CreateLookup(item, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
            var createdResult = (CreatedAtActionResult)result;
            Assert.That(createdResult.ActionName, Is.EqualTo(nameof(LookupsController.GetByGroup)));
            Assert.That(createdResult.Value, Is.EqualTo(created));
        }

        [Test]
        public void CreateLookup_PropagatesException_WhenServiceThrowsOnDuplicate()
        {
            var item = new LookupItem { LookupGroup = "Degree", Value = "BSc" };
            _lookupServiceMock.Setup(s => s.AddLookupItemAsync(item, It.IsAny<CancellationToken>()))
                               .ThrowsAsync(new System.InvalidOperationException("Duplicate lookup value"));

            Assert.ThrowsAsync<System.InvalidOperationException>(async () =>
                await _controller.CreateLookup(item, CancellationToken.None));
        }

        [Test]
        public async Task UpdateLookup_ReturnsOk_WhenServiceSucceeds()
        {
            var item = new LookupItem { LookupGroup = "Degree", Value = "MSc" };
            _lookupServiceMock.Setup(s => s.UpdateLookupItemAsync(1, item, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await _controller.UpdateLookup(1, item, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task UpdateLookup_ReturnsNotFound_WhenIdMissing()
        {
            var item = new LookupItem { LookupGroup = "Degree", Value = "MSc" };
            _lookupServiceMock.Setup(s => s.UpdateLookupItemAsync(99, item, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = await _controller.UpdateLookup(99, item, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }

        [Test]
        public async Task DeleteLookup_ReturnsOk_WhenServiceSucceeds()
        {
            _lookupServiceMock.Setup(s => s.DeleteLookupItemAsync(1, It.IsAny<CancellationToken>())).ReturnsAsync(true);

            var result = await _controller.DeleteLookup(1, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<OkObjectResult>());
        }

        [Test]
        public async Task DeleteLookup_ReturnsNotFound_WhenIdMissing()
        {
            _lookupServiceMock.Setup(s => s.DeleteLookupItemAsync(99, It.IsAny<CancellationToken>())).ReturnsAsync(false);

            var result = await _controller.DeleteLookup(99, CancellationToken.None);

            Assert.That(result, Is.InstanceOf<NotFoundResult>());
        }
    }
}

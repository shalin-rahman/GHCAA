using FluentAssertions;
using GHCAA.Application.DTOs;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;
using static GHCAA.Domain.Enums;

namespace GHCAA.Tests.Services;

[TestFixture]
public sealed class ElectionPersonaServiceTests : TestBase
{
    private static SaveElectionPersonaDto MakeDto(string name = "Test Persona") => new()
    {
        Name = name,
        GroupName = "Officials",
        Description = "A test persona.",
        Permissions = ElectionPermission.ViewDashboard,
        MinCount = 0,
        MaxCount = null,
        ShowOnPublicBoard = true,
        TakesOverFromAdmin = false,
        DeclarationText = "I declare.",
        SortOrder = 1
    };

    [Test]
    [Category("FR-39")]
    public async Task CreateAsync_AddsPersona()
    {
        var result = (await new ElectionPersonaService(_context, NewFreeze()).CreateAsync(MakeDto(), CancellationToken.None)).Persona!;

        result.Id.Should().BeGreaterThan(0);
        _context.ElectionPersonas.Single().Name.Should().Be("Test Persona");
    }

    [Test]
    [Category("FR-39")]
    public async Task CreateAsync_RefusesDuplicateName()
    {
        var service = new ElectionPersonaService(_context, NewFreeze());
        await service.CreateAsync(MakeDto(), CancellationToken.None);

        var (success, error, _) = await service.CreateAsync(MakeDto(), CancellationToken.None);

        success.Should().BeFalse();
        error.Should().Be("duplicate-name");
        _context.ElectionPersonas.Count().Should().Be(1);
    }

    [Test]
    [Category("FR-39")]
    public void SaveDto_MaxCountBelowMinCount_FailsValidation()
    {
        var dto = MakeDto();
        dto.MinCount = 3;
        dto.MaxCount = 1;

        var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        System.ComponentModel.DataAnnotations.Validator.TryValidateObject(dto, new(dto), results, true).Should().BeFalse();
        results.Should().ContainSingle(r => r.MemberNames.Contains(nameof(SaveElectionPersonaDto.MaxCount)));
    }

    [Test]
    [Category("FR-39")]
    public void PickPrimaryRoleNameForClient_MemberAndOfficial_GivesMember()
    {
        var roles = new List<Role>
        {
            new() { Name = GHCAA.Domain.Constants.Roles.ElectionOfficial },
            new() { Name = GHCAA.Domain.Constants.Roles.Member }
        };

        AuthService.PickPrimaryRoleNameForClient(roles).Should().Be(GHCAA.Domain.Constants.Roles.Member);
    }

    [Test]
    [Category("FR-39")]
    public async Task UpdateAsync_ChangesFields()
    {
        var service = new ElectionPersonaService(_context, NewFreeze());
        var created = (await service.CreateAsync(MakeDto(), CancellationToken.None)).Persona!;

        var dto = MakeDto("Renamed Persona");
        var (success, error, updated) = await service.UpdateAsync(created.Id, dto, CancellationToken.None);

        success.Should().BeTrue();
        error.Should().BeNull();
        updated!.Name.Should().Be("Renamed Persona");
    }

    [Test]
    [Category("FR-39")]
    public async Task UpdateAsync_ReturnsNotFoundForMissingId()
    {
        var (success, error, updated) = await new ElectionPersonaService(_context, NewFreeze()).UpdateAsync(9999, MakeDto(), CancellationToken.None);

        success.Should().BeFalse();
        error.Should().Be("not-found");
        updated.Should().BeNull();
    }

    [Test]
    [Category("FR-39")]
    public async Task DeleteAsync_RemovesUnusedPersona()
    {
        var service = new ElectionPersonaService(_context, NewFreeze());
        var created = (await service.CreateAsync(MakeDto(), CancellationToken.None)).Persona!;

        var (success, error) = await service.DeleteAsync(created.Id, CancellationToken.None);

        success.Should().BeTrue();
        error.Should().BeNull();
        _context.ElectionPersonas.Should().BeEmpty();
    }

    [Test]
    [Category("FR-39")]
    public async Task SetActiveAsync_FalseHidesFromActiveList()
    {
        var service = new ElectionPersonaService(_context, NewFreeze());
        var created = (await service.CreateAsync(MakeDto(), CancellationToken.None)).Persona!;

        var ok = await service.SetActiveAsync(created.Id, false, CancellationToken.None);
        var activeOnly = await service.ListAsync(includeInactive: false, CancellationToken.None);
        var withInactive = await service.ListAsync(includeInactive: true, CancellationToken.None);

        ok.Should().BeTrue();
        activeOnly.Should().BeEmpty();
        withInactive.Should().ContainSingle(p => p.Id == created.Id && !p.IsActive);
    }

    [Test]
    [Category("FR-39")]
    public async Task Seeder_FillsEmptyTable()
    {
        await ElectionPersonaSeeder.EnsureAsync(_context, NullLogger.Instance);

        _context.ElectionPersonas.Should().HaveCount(14);
    }

    [Test]
    [Category("FR-39")]
    public async Task Seeder_AddsMissingDefaultsAndKeepsExistingRows()
    {
        var service = new ElectionPersonaService(_context, NewFreeze());
        await service.CreateAsync(MakeDto("Returning Officer"), CancellationToken.None);
        await service.CreateAsync(MakeDto(), CancellationToken.None);

        await ElectionPersonaSeeder.EnsureAsync(_context, NullLogger.Instance);

        _context.ElectionPersonas.Should().HaveCount(15);
        _context.ElectionPersonas.Single(p => p.Name == "Returning Officer").Description.Should().Be("A test persona.");
    }

    [Test]
    [Category("FR-39")]
    public async Task CreateAsync_RefusesGroupOutsideTheList()
    {
        var dto = MakeDto();
        dto.GroupName = "Oficials";

        var (success, error, _) = await new ElectionPersonaService(_context, NewFreeze()).CreateAsync(dto, CancellationToken.None);

        success.Should().BeFalse();
        error.Should().Be("invalid-group");
        _context.ElectionPersonas.Should().BeEmpty();
    }

    [Test]
    [Category("FR-39")]
    public async Task UpdateAsync_KeepsAnOlderGroup_ButRefusesMovingToAnUnknownOne()
    {
        var now = DateTime.UtcNow;
        var legacy = new ElectionPersona
        {
            Name = "Old Persona", GroupName = "Volunteers", Description = "d", DeclarationText = "x",
            Permissions = ElectionPermission.ViewDashboard, IsActive = true, CreatedAt = now, UpdatedAt = now
        };
        _context.ElectionPersonas.Add(legacy);
        await _context.SaveChangesAsync();
        var service = new ElectionPersonaService(_context, NewFreeze());

        var keep = MakeDto("Old Persona");
        keep.GroupName = "Volunteers";
        (await service.UpdateAsync(legacy.Id, keep, CancellationToken.None)).Success.Should().BeTrue();

        var move = MakeDto("Old Persona");
        move.GroupName = "Helpers";
        var (success, error, _) = await service.UpdateAsync(legacy.Id, move, CancellationToken.None);
        success.Should().BeFalse();
        error.Should().Be("invalid-group");
    }

    [Test]
    [Category("FR-39")]
    public async Task Seeder_FillsADefaultPersonaThatHasNoPermissions()
    {
        var empty = MakeDto("Returning Officer");
        empty.Permissions = ElectionPermission.None;
        var edited = MakeDto("Scrutineer");
        edited.Permissions = ElectionPermission.ViewAudit;
        var custom = MakeDto("Custom Helper");
        custom.Permissions = ElectionPermission.None;
        var service = new ElectionPersonaService(_context, NewFreeze());
        await service.CreateAsync(empty, CancellationToken.None);
        await service.CreateAsync(edited, CancellationToken.None);
        await service.CreateAsync(custom, CancellationToken.None);

        await ElectionPersonaSeeder.EnsureAsync(_context, NullLogger.Instance);

        _context.ElectionPersonas.Single(p => p.Name == "Returning Officer").Permissions.Should().Be(
            ElectionPermission.ViewDashboard | ElectionPermission.ViewAudit | ElectionPermission.DecideNominations
            | ElectionPermission.SetBallotKey | ElectionPermission.Count | ElectionPermission.Approve);
        _context.ElectionPersonas.Single(p => p.Name == "Scrutineer").Permissions.Should().Be(ElectionPermission.ViewAudit);
        _context.ElectionPersonas.Single(p => p.Name == "Custom Helper").Permissions.Should().Be(ElectionPermission.None);
    }

    [Test]
    [Category("FR-39")]
    public async Task Seeder_EveryDefaultPersonaHasPermissionsAndAKnownGroup()
    {
        await ElectionPersonaSeeder.EnsureAsync(_context, NullLogger.Instance);

        _context.ElectionPersonas.Should().OnlyContain(p =>
            p.Permissions != ElectionPermission.None && Constants.Elections.PersonaGroups.All.Contains(p.GroupName));
    }
}

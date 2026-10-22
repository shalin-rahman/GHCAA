using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using static GHCAA.Domain.Enums;

namespace GHCAA.Infrastructure.Services;

// The values that fill {{...}} tokens in election pages and form layouts. Positions and
// PositionSeats repeat: the line or array item that holds one is copied once per value.
public sealed record ElectionContentTokens(
    string OrgFullName,
    string OrgShortName,
    string Head,
    string Secretary,
    IReadOnlyList<string> Positions,
    IReadOnlyList<string> PositionSeats)
{
    private const string PositionsToken = "{{Positions}}";
    private const string PositionSeatsToken = "{{PositionSeats}}";

    internal static readonly JsonSerializerOptions LayoutJson = new()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static async Task<ElectionContentTokens> LoadAsync(ApplicationDbContext db, OrgConfigDto org, CancellationToken ct = default)
    {
        var (head, secretary) = await IDCardService.GetSignatureCaptionsAsync(db, ct);
        var positions = await db.EcPositions.AsNoTracking().Where(p => p.IsActive)
            .OrderBy(p => p.DisplayOrder).ThenBy(p => p.Id)
            .Select(p => new { p.Name, p.SeatCount }).ToListAsync(ct);
        return new ElectionContentTokens(org.Branding.FullName, org.Branding.ShortName, head, secretary,
            positions.Select(p => p.Name).ToList(),
            positions.SelectMany(p => p.SeatCount > 1
                ? Enumerable.Range(1, p.SeatCount).Select(n => $"{p.Name} {n}")
                : [p.Name]).ToList());
    }

    // Markdown and other line-based text.
    public string ResolveText(string text)
    {
        var lines = text.Split('\n');
        return string.Join('\n', lines.SelectMany(Expand).Select(ReplaceScalars));
    }

    // A form layout. Strings are filled in place; an array item holding a repeater becomes several items.
    // A body that is not JSON comes back unchanged, so a corrupt row reads as itself and not as a 500.
    public string ResolveLayout(string json)
    {
        JsonNode? node;
        try
        {
            node = JsonNode.Parse(json);
        }
        catch (JsonException)
        {
            return json;
        }
        return node == null ? json : Resolve(node)!.ToJsonString(LayoutJson);
    }

    private JsonNode? Resolve(JsonNode? node)
    {
        switch (node)
        {
            case JsonObject obj:
                foreach (var key in obj.Select(p => p.Key).ToList())
                    obj[key] = Resolve(obj[key]?.DeepClone());
                return obj;
            case JsonArray array:
                var items = new JsonArray();
                foreach (var item in array)
                {
                    if (item is JsonValue value && value.TryGetValue<string>(out var text))
                        foreach (var line in Expand(text)) items.Add(ReplaceScalars(line));
                    else
                        items.Add(Resolve(item?.DeepClone()));
                }
                return items;
            case JsonValue value when value.TryGetValue<string>(out var text):
                return ReplaceScalars(text);
            default:
                return node;
        }
    }

    private IEnumerable<string> Expand(string line)
    {
        if (line.Contains(PositionSeatsToken)) return PositionSeats.Select(v => line.Replace(PositionSeatsToken, v));
        if (line.Contains(PositionsToken)) return Positions.Select(v => line.Replace(PositionsToken, v));
        return [line];
    }

    private string ReplaceScalars(string text) => text
        .Replace("{{OrgFullName}}", OrgFullName)
        .Replace("{{OrgShortName}}", OrgShortName)
        .Replace("{{Head}}", Head)
        .Replace("{{Secretary}}", Secretary);
}

// 37.14s. Election pages and form layouts. Public reads fill the tokens; admin reads and
// saves keep them. There is no delete: the seeder would add a profile key straight back, so
// an admin unpublishes a row instead.
public sealed class ElectionContentService(ApplicationDbContext db, IOrgConfigService orgConfigService) : IElectionContentService
{
    public async Task<IReadOnlyList<ElectionContentDto>> GetPublishedAsync(ElectionContentKind? kind, CancellationToken cancellationToken = default)
    {
        var rows = await db.ElectionContents.AsNoTracking()
            .Where(x => x.IsPublished && (kind == null || x.Kind == kind))
            .OrderBy(x => x.Kind).ThenBy(x => x.DisplayOrder).ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);
        return rows.Select(x => Map(x, body: null)).ToList();
    }

    public async Task<ElectionContentDto?> GetPublishedAsync(ElectionContentKind kind, string key, CancellationToken cancellationToken = default)
    {
        var row = await FindAsync(db, kind, key, publishedOnly: true, cancellationToken);
        if (row == null) return null;
        var tokens = await ElectionContentTokens.LoadAsync(db, await orgConfigService.GetConfigAsync(), cancellationToken);
        return Map(row, kind == ElectionContentKind.Form ? tokens.ResolveLayout(row.Body) : tokens.ResolveText(row.Body));
    }

    public async Task<IReadOnlyList<ElectionContentDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var rows = await db.ElectionContents.AsNoTracking()
            .OrderBy(x => x.Kind).ThenBy(x => x.DisplayOrder).ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);
        return rows.Select(x => Map(x, body: null)).ToList();
    }

    public async Task<ElectionContentDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var row = await db.ElectionContents.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        return row == null ? null : Map(row, row.Body);
    }

    public async Task<(ElectionContentDto? Content, string? Error)> CreateAsync(UpsertElectionContentDto dto, int adminId, CancellationToken cancellationToken = default)
    {
        var key = NormalizeKey(dto.Kind, dto.Key);
        if (Validate(dto) is { } error) return (null, error);
        if (await FindAsync(db, dto.Kind, key, publishedOnly: false, cancellationToken) != null)
            return (null, Duplicate(dto.Kind, key));

        var row = new ElectionContent { Key = key, Kind = dto.Kind };
        Apply(row, dto, adminId);
        db.ElectionContents.Add(row);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (DbErrors.IsUniqueViolation(ex))
        {
            // Two admins created the same key at once and the other save won. Detach the row so
            // a later save on this context does not try it again.
            db.Entry(row).State = EntityState.Detached;
            return (null, Duplicate(dto.Kind, key));
        }
        return (Map(row, row.Body), null);
    }

    public async Task<(ElectionContentDto? Content, string? Error)> UpdateAsync(int id, UpsertElectionContentDto dto, int adminId, CancellationToken cancellationToken = default)
    {
        var row = await db.ElectionContents.SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
            ?? throw new KeyNotFoundException($"Election content {id} not found.");
        // Printed QR codes and ?doc= links name the key, so it stays as it was.
        if (row.Kind != dto.Kind || !string.Equals(row.Key, NormalizeKey(dto.Kind, dto.Key), StringComparison.OrdinalIgnoreCase))
            return (null, "Kind and key cannot change.");
        if (Validate(dto) is { } error) return (null, error);

        Apply(row, dto, adminId);
        await db.SaveChangesAsync(cancellationToken);
        return (Map(row, row.Body), null);
    }

    // Keys are stored as given. Form codes are upper case so ER-27c and ER-27C are one form.
    internal static async Task<ElectionContent?> FindAsync(ApplicationDbContext db, ElectionContentKind kind, string key, bool publishedOnly, CancellationToken ct)
    {
        var normalized = NormalizeKey(kind, key);
        var lower = normalized.ToLowerInvariant();
        return await db.ElectionContents.AsNoTracking()
            .Where(x => x.Kind == kind && (x.Key == normalized || x.Key.ToLower() == lower) && (!publishedOnly || x.IsPublished))
            .FirstOrDefaultAsync(ct);
    }

    private static string Duplicate(ElectionContentKind kind, string key) =>
        $"A {kind.ToString().ToLowerInvariant()} with key {key} already exists.";

    private static string NormalizeKey(ElectionContentKind kind, string key) =>
        kind == ElectionContentKind.Form ? key.Trim().ToUpperInvariant() : key.Trim();

    // Public so a test can check the rules without a database.
    public static string? Validate(UpsertElectionContentDto dto)
    {
        if (dto.Kind == ElectionContentKind.Page)
            return dto.Format == null ? "A page needs a format." : null;
        if (dto.Format != null) return "A form has no page format.";
        return ValidateLayout(dto.Body);
    }

    public static string? ValidateLayout(string body)
    {
        ElectionFormLayout? layout;
        try
        {
            layout = JsonSerializer.Deserialize<ElectionFormLayout>(body, ElectionContentTokens.LayoutJson);
        }
        catch (JsonException ex)
        {
            return $"The form layout is not valid JSON: {ex.Message}";
        }
        if (layout == null || layout.Sections.Count == 0) return "The form layout needs at least one section.";
        foreach (var section in layout.Sections)
        {
            if (section.Kind == null || !ElectionFormLayout.SectionKinds.Contains(section.Kind))
                return $"Section kind must be one of: {string.Join(", ", ElectionFormLayout.SectionKinds)}.";
            if (section.Items.Count == 0) return $"Section \"{section.Name}\" has no items.";
            if (section.Blank is < 0 or > 50) return $"Section \"{section.Name}\" asks for {section.Blank} blank rows; use 0 to 50.";
        }
        return null;
    }

    private static void Apply(ElectionContent row, UpsertElectionContentDto dto, int adminId)
    {
        row.Format = dto.Format;
        row.Group = string.IsNullOrWhiteSpace(dto.Group) ? null : dto.Group.Trim();
        row.Title = dto.Title.Trim();
        row.Summary = string.IsNullOrWhiteSpace(dto.Summary) ? null : dto.Summary.Trim();
        row.Body = dto.Body;
        row.DisplayOrder = dto.DisplayOrder;
        row.IsPublished = dto.IsPublished;
        row.LastModified = DateTime.UtcNow;
        row.UpdatedByAdminId = adminId;
    }

    private static ElectionContentDto Map(ElectionContent x, string? body) => new()
    {
        Id = x.Id,
        Key = x.Key,
        Kind = x.Kind,
        Format = x.Format,
        Group = x.Group,
        Title = x.Title,
        Summary = x.Summary,
        Body = body,
        DisplayOrder = x.DisplayOrder,
        IsPublished = x.IsPublished,
        LastModified = x.LastModified,
    };
}

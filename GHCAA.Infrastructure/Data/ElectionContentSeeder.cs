using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using static GHCAA.Domain.Enums;

namespace GHCAA.Infrastructure.Data
{
    // 37.14s. Loads the election pages and form layouts from profiles/<org>/election-content
    // (ORG_PROFILE first, then default). It adds a key the table lacks, and refreshes a row no
    // admin has saved, so a wording change in the profile reaches a live database on the next
    // start. A row an admin has saved is never overwritten.
    public static class ElectionContentSeeder
    {
        internal const string Folder = "election-content";

        // Keeps quotes and non-ASCII readable when an admin opens the layout to edit it.
        private static readonly JsonSerializerOptions LayoutJson = new() { Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping };

        public static async Task SyncAsync(ApplicationDbContext context, ILogger logger, CancellationToken ct = default)
        {
            var seeded = Load(logger);
            if (seeded.Count == 0)
            {
                logger.LogWarning("Election content sync skipped: no profile files under {Folder}.", Folder);
                return;
            }

            var existing = (await context.ElectionContents.ToListAsync(ct))
                .GroupBy(x => (x.Kind, x.Key.ToUpperInvariant()))
                .ToDictionary(g => g.Key, g => g.First());
            int added = 0, refreshed = 0;
            foreach (var row in seeded)
            {
                var key = (row.Kind, row.Key.ToUpperInvariant());
                if (!existing.TryGetValue(key, out var current))
                {
                    context.ElectionContents.Add(row);
                    existing[key] = row;
                    added++;
                }
                // Every admin save sets UpdatedByAdminId, so a null one means the row is still as seeded.
                else if (current.UpdatedByAdminId == null && Refresh(current, row))
                {
                    refreshed++;
                }
            }

            if (added == 0 && refreshed == 0) return;
            await context.SaveChangesAsync(ct);
            logger.LogInformation("Election content sync added {Added} rows and refreshed {Refreshed}.", added, refreshed);
        }

        // Copies the profile text onto a row still as seeded. IsPublished stays, as only an admin changes it.
        private static bool Refresh(ElectionContent current, ElectionContent seeded)
        {
            if (current.Title == seeded.Title && current.Body == seeded.Body && current.Summary == seeded.Summary
                && current.Group == seeded.Group && current.Format == seeded.Format && current.DisplayOrder == seeded.DisplayOrder)
                return false;
            current.Title = seeded.Title;
            current.Body = seeded.Body;
            current.Summary = seeded.Summary;
            current.Group = seeded.Group;
            current.Format = seeded.Format;
            current.DisplayOrder = seeded.DisplayOrder;
            return true;
        }

        // The pages.json path for the active profile, or null. Public so a test can find the profile copies.
        public static string? PagesManifestPath() =>
            ApplicationDbContext.ResolveProfilePackPath(Path.Combine(Folder, "pages.json"));

        // Public for tests. Returns pages then forms, or an empty list when the profile has neither file.
        public static List<ElectionContent> Load(ILogger? logger = null)
        {
            var rows = new List<ElectionContent>();
            var pagesPath = PagesManifestPath();
            if (pagesPath != null)
            {
                var pagesDir = Path.Combine(Path.GetDirectoryName(pagesPath)!, "pages");
                foreach (var page in ReadArray(pagesPath, logger))
                {
                    try
                    {
                        var file = Path.Combine(pagesDir, Required(page!.AsObject(), "file"));
                        if (!File.Exists(file))
                        {
                            logger?.LogWarning("Election page {File} is listed in pages.json but missing.", file);
                            continue;
                        }
                        rows.Add(new ElectionContent
                        {
                            Key = Required(page.AsObject(), "key"),
                            Kind = ElectionContentKind.Page,
                            Format = Enum.Parse<ElectionPageFormat>(Required(page.AsObject(), "format")),
                            Group = (string?)page["group"],
                            Title = Required(page.AsObject(), "title"),
                            Summary = (string?)page["summary"],
                            Body = File.ReadAllText(file),
                            DisplayOrder = (int)page["displayOrder"]!,
                        });
                    }
                    catch (Exception ex)
                    {
                        // A missing field or bad value in one entry skips that entry only.
                        logger?.LogWarning(ex, "Election page entry {Entry} in {File} skipped.", page?.ToJsonString(), pagesPath);
                    }
                }
            }

            var formsPath = ApplicationDbContext.ResolveProfilePackPath(Path.Combine(Folder, "forms.json"));
            if (formsPath != null)
                rows.AddRange(ParseForms(ReadArray(formsPath, logger), formsPath, logger));

            return rows;
        }

        // Public for tests. A bad entry is logged and skipped; DisplayOrder keeps its place in the file.
        public static List<ElectionContent> ParseForms(JsonArray forms, string source, ILogger? logger = null)
        {
            var rows = new List<ElectionContent>();
            var order = 0;
            foreach (var form in forms)
            {
                order++;
                try
                {
                    var layout = form!.AsObject();
                    var code = Required(layout, "code");
                    var title = Required(layout, "title");
                    // Key and Title have their own columns, so the stored layout leaves them out.
                    layout.Remove("code");
                    layout.Remove("title");
                    var body = layout.ToJsonString(LayoutJson);
                    // The same check an admin edit gets, so a bad layout fails here and not at print time.
                    if (ElectionContentService.ValidateLayout(body) is { } error)
                    {
                        logger?.LogWarning("Election form {Code} in {File} skipped: {Error}", code, source, error);
                        continue;
                    }
                    rows.Add(new ElectionContent
                    {
                        Key = code,
                        Kind = ElectionContentKind.Form,
                        Title = title,
                        Body = body,
                        DisplayOrder = order,
                    });
                }
                catch (Exception ex)
                {
                    logger?.LogWarning(ex, "Election form entry {Order} in {File} skipped.", order, source);
                }
            }
            return rows;
        }

        // A cast of a missing field gives null rather than throwing, and a null key would fail the whole save.
        private static string Required(JsonObject entry, string field) =>
            (string?)entry[field] is { } value && !string.IsNullOrWhiteSpace(value)
                ? value
                : throw new InvalidOperationException($"Field '{field}' is missing or empty.");

        // A file that does not parse is skipped whole, so it cannot stop the other file from seeding.
        private static JsonArray ReadArray(string path, ILogger? logger)
        {
            try
            {
                return JsonNode.Parse(File.ReadAllText(path))?.AsArray() ?? new JsonArray();
            }
            catch (Exception ex) when (ex is JsonException or InvalidOperationException or IOException)
            {
                logger?.LogWarning(ex, "Election content file {File} could not be read and was skipped.", path);
                return new JsonArray();
            }
        }
    }
}

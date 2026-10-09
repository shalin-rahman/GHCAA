using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GHCAA.Infrastructure.Services
{
    // 82.115: parses docs/TODO.md into the rows the "Developer Options" admin screen shows,
    // so a SuperAdmin can see open work without opening the raw file.
    public class DevTrackerService : IDevTrackerService
    {
        private readonly IHostEnvironment _environment;
        private readonly ILogger<DevTrackerService> _logger;

        // Matches "# Work Package 83 — Registration transaction bug ..."
        private static readonly Regex WorkPackageHeaderPattern = new(
            @"^#\s*Work Package\s+(?<number>\d+)\s*[—-]\s*(?<title>.+?)\s*$",
            RegexOptions.Compiled);

        // Same item shapes as docs/book/build/tracker_page.py, so this page and the published
        // tracker count the same items:
        //   "82.115 [TODO] **Priority: P3 | Depends on: none.** text"
        //   "60.4 [TODO, tracked in 82.84] **Priority: P1.** text"  (note after the status word)
        //   "- **37.13m Title [TODO] Priority: P1 | Depends on: none.** text"  (WP37 bullet shape)
        // A title is only allowed after the bullet and bold, so a numbered prose line is not an item.
        private static readonly Regex ItemLinePattern = new(
            @"^(?<bold>\s*- \*\*)?(?<id>\d+\.\d+[a-z]?)(?(bold)(?: (?<title>[^\[\n]*?))?) *" +
            @"\[(?<status>TODO|IN PROGRESS|BLOCKED|ONHOLD|PARTIAL|DONE)[^\]]*\]\s*(?<rest>.*)$",
            RegexOptions.Compiled);

        private static readonly Regex MetaRunPattern = new(@"\*\*([^*]*(?:Priority|Depends on):[^*]*)\*\*", RegexOptions.Compiled);
        private static readonly Regex PriorityPattern = new(@"Priority:\s*(P\d)", RegexOptions.Compiled);
        private static readonly Regex DependsOnPattern = new(@"Depends on:?\s*([^|*]+)", RegexOptions.Compiled);

        private static readonly string[] OpenStatuses = { "TODO", "PARTIAL", "IN PROGRESS", "BLOCKED", "ONHOLD" };

        public DevTrackerService(IHostEnvironment environment, ILogger<DevTrackerService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<List<DevTrackerItemDto>> GetOpenItemsAsync(DevTrackerFilterDto filter, CancellationToken cancellationToken = default)
        {
            var path = ResolveTodoPath(_environment.ContentRootPath);
            if (path == null)
            {
                _logger.LogWarning("docs/TODO.md not found from content root {ContentRoot}; returning an empty tracker.", _environment.ContentRootPath);
                return new List<DevTrackerItemDto>();
            }

            var lines = await File.ReadAllLinesAsync(path, cancellationToken);

            var items = new List<DevTrackerItemDto>();
            var currentWpNumber = 0;
            var currentWpTitle = "Unassigned";

            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var headerMatch = WorkPackageHeaderPattern.Match(line);
                if (headerMatch.Success)
                {
                    currentWpNumber = int.Parse(headerMatch.Groups["number"].Value);
                    currentWpTitle = headerMatch.Groups["title"].Value;
                    continue;
                }

                var itemMatch = ItemLinePattern.Match(line);
                if (!itemMatch.Success) continue;

                var status = itemMatch.Groups["status"].Value;
                if (!OpenStatuses.Contains(status)) continue;

                // Most items wrap onto the following lines, and older ones put the Priority run
                // there too, so read up to the next blank line, item or heading.
                var body = new List<string> { (itemMatch.Groups["bold"].Success ? "**" : "") + itemMatch.Groups["rest"].Value };
                for (var j = i + 1; j < lines.Length; j++)
                {
                    var next = lines[j];
                    if (string.IsNullOrWhiteSpace(next) || next.StartsWith('#') || ItemLinePattern.IsMatch(next)) break;
                    body.Add(next.Trim());
                }
                var blob = string.Join(" ", body);

                var meta = MetaRunPattern.Match(blob);
                var metaText = meta.Success ? meta.Groups[1].Value : blob;
                var priority = PriorityPattern.Match(metaText);
                var dependsMatch = DependsOnPattern.Match(metaText);
                var dependsOn = dependsMatch.Success ? dependsMatch.Groups[1].Value.Trim().TrimEnd('.').Trim() : null;

                var summary = meta.Success ? blob.Remove(meta.Index, meta.Length) : blob;
                summary = summary.TrimStart(' ', '|', '*', '.').Trim();
                if (itemMatch.Groups["title"].Success && !string.IsNullOrWhiteSpace(itemMatch.Groups["title"].Value))
                {
                    summary = $"{itemMatch.Groups["title"].Value.Trim()}. {summary}";
                }

                items.Add(new DevTrackerItemDto
                {
                    Id = itemMatch.Groups["id"].Value,
                    Status = status,
                    Priority = priority.Success ? priority.Groups[1].Value : "none",
                    DependsOn = string.IsNullOrWhiteSpace(dependsOn) || dependsOn.Equals("none", StringComparison.OrdinalIgnoreCase) ? null : dependsOn,
                    Summary = summary,
                    WorkPackageNumber = currentWpNumber,
                    WorkPackageTitle = currentWpTitle
                });
            }

            if (!string.IsNullOrWhiteSpace(filter.Priority) && !filter.Priority.Equals("all", StringComparison.OrdinalIgnoreCase))
            {
                items = items.Where(i => i.Priority.Equals(filter.Priority, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return items
                .OrderBy(i => i.WorkPackageNumber)
                .ThenBy(i => i.Priority)
                .ToList();
        }

        // The Docker image's WORKDIR copies docs/TODO.md straight into the content root (see
        // Dockerfile), so the direct path covers production. A local `dotnet run` from
        // GHCAA.API/ has the content root one level below the repo root, where docs/ actually
        // lives — the parent fallback covers that, same as InstitutionProfileProvider does for
        // profiles/.
        private static string? ResolveTodoPath(string contentRootPath)
        {
            var direct = Path.Combine(contentRootPath, "docs", "TODO.md");
            if (File.Exists(direct)) return direct;

            var parent = Directory.GetParent(contentRootPath);
            if (parent != null)
            {
                var viaParent = Path.Combine(parent.FullName, "docs", "TODO.md");
                if (File.Exists(viaParent)) return viaParent;
            }

            return null;
        }
    }
}

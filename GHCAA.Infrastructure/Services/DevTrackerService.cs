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

        // Matches "82.115 [TODO] **Priority: P3 | Depends on: none.** description..." and the
        // older "6.2 [TODO] **Priority: P3.** description" form without a Depends-on clause.
        private static readonly Regex ItemLinePattern = new(
            @"^(?<id>\d+\.\d+[a-z]?)\s+\[(?<status>[A-Za-z-]+)(?:\s+\d{4}-\d{2}-\d{2})?\]\s+" +
            @"\*\*Priority:\s*(?<priority>P\d)\s*(?:\|\s*Depends on:\s*(?<dependsOn>[^*]+?))?\.\*\*\s*(?<summary>.*)$",
            RegexOptions.Compiled);

        private static readonly string[] OpenStatuses = { "TODO", "PARTIAL" };

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

            foreach (var line in lines)
            {
                var headerMatch = WorkPackageHeaderPattern.Match(line);
                if (headerMatch.Success)
                {
                    currentWpNumber = int.Parse(headerMatch.Groups["number"].Value);
                    currentWpTitle = headerMatch.Groups["title"].Value;
                    continue;
                }

                var itemMatch = ItemLinePattern.Match(line);
                if (!itemMatch.Success) continue;

                var status = itemMatch.Groups["status"].Value.ToUpperInvariant();
                if (!OpenStatuses.Contains(status)) continue;

                var dependsOn = itemMatch.Groups["dependsOn"].Success ? itemMatch.Groups["dependsOn"].Value.Trim() : null;

                items.Add(new DevTrackerItemDto
                {
                    Id = itemMatch.Groups["id"].Value,
                    Status = status,
                    Priority = itemMatch.Groups["priority"].Value,
                    DependsOn = string.IsNullOrWhiteSpace(dependsOn) || dependsOn.Equals("none", StringComparison.OrdinalIgnoreCase) ? null : dependsOn,
                    Summary = itemMatch.Groups["summary"].Value.Trim(),
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

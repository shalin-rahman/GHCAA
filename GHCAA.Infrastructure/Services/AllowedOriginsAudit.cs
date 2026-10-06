using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GHCAA.Infrastructure.Services;

// 7.18, DEPLOY-CORS-001 and 002. AllowedOrigins comes from an environment variable, so the app
// cannot see who changed it. Startup checks its shape and records each change it notices.
public static class AllowedOriginsAudit
{
    // What is wrong with each bad entry. Empty when every entry is a bare https origin.
    public static IReadOnlyList<string> Problems(IEnumerable<string> origins)
    {
        var problems = new List<string>();
        foreach (var origin in origins)
        {
            if (origin.Trim() == Constants.Deploy.WildcardOrigin)
                problems.Add($"\"{origin}\" allows every site; list each origin instead.");
            else if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps)
                problems.Add($"\"{origin}\" is not an https address.");
            // A bare origin is exactly its scheme, host and port. A path, query, fragment, user
            // info or trailing slash all make it longer, and CORS would never match it.
            else if (!string.Equals(uri.GetLeftPart(UriPartial.Authority), origin, StringComparison.OrdinalIgnoreCase)
                || uri.UserInfo.Length > 0)
                problems.Add($"\"{origin}\" must be scheme and host only, with no path, query or user info.");
        }
        return problems;
    }

    // 7.19. Emails and payment returns send people to ClientUrl, so it must be one of the
    // origins the API accepts. Null when it is.
    public static string? ClientUrlProblem(string? clientUrl, IEnumerable<string> origins)
    {
        if (string.IsNullOrWhiteSpace(clientUrl))
            return $"{Constants.ConfigKeys.ClientUrl} is not set.";
        return origins.Contains(clientUrl, StringComparer.OrdinalIgnoreCase)
            ? null
            : $"{Constants.ConfigKeys.ClientUrl} \"{clientUrl}\" is not in {Constants.ConfigKeys.AllowedOrigins}.";
    }

    // Writes an activity row when the list differs from the one last recorded, including the
    // first time. Returns true when it wrote one.
    public static async Task<bool> RecordIfChangedAsync(ApplicationDbContext db, IActivityService activity,
        IElectionFreezeService freeze, IReadOnlyList<string> origins, CancellationToken ct = default)
    {
        var lastMetadata = await db.ActivityLogs.AsNoTracking()
            .Where(a => a.ActivityType == Constants.Deploy.AllowedOriginsChangedAuditType)
            .OrderByDescending(a => a.Timestamp).ThenByDescending(a => a.Id)
            .Select(a => a.Metadata)
            .FirstOrDefaultAsync(ct);
        var previous = lastMetadata is null ? null : JsonSerializer.Deserialize<Recorded>(lastMetadata)?.Origins;
        if (previous is not null && previous.SequenceEqual(origins)) return false;

        var frozen = await freeze.FindFrozenAsync(ct: ct);
        var description = previous is null
            ? "Recorded the allowed origins for the first time."
            : "The allowed origins changed since the last start.";
        if (frozen is not null)
            description += $" \"{frozen.Title}\" is frozen for polling or counting.";

        await activity.LogActivityAsync(null, Constants.Deploy.AllowedOriginsChangedAuditType, description,
            source: Constants.ActivitySources.System,
            metadata: JsonSerializer.Serialize(new Recorded(origins.ToArray(), previous, frozen?.Id)),
            cancellationToken: ct);
        return true;
    }

    private sealed record Recorded(string[] Origins, string[]? Previous, int? FrozenElectionId);
}

using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using GHCAA.Infrastructure.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GHCAA.Infrastructure.Services
{
    // The defaults come from profiles/<ORG_PROFILE>/org-config.json (62.6b). There is no code copy
    // to fall back to: InstitutionProfileProvider stops the app at boot when ORG_PROFILE is unset.
    public class OrgConfigService(
        ApplicationDbContext db,
        IMemoryCache cache,
        IInstitutionProfileProvider profiles,
        Microsoft.Extensions.Configuration.IConfiguration? configuration = null,
        IElectionFreezeService? freeze = null) : IOrgConfigService
    {
        private const string CacheKey = "org_config_v1";
        private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

        // Camelcase matches ASP.NET Core's default wire format and keeps DB storage consistent
        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        public async Task<OrgConfigDto> GetConfigAsync()
        {
            // GetOrCreateAsync is atomic under concurrent load — no thundering-herd race
            return (await cache.GetOrCreateAsync(CacheKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = CacheTtl;
                var record = await db.OrganizationConfigs.AsNoTracking().FirstOrDefaultAsync();
                var dto = record is not null
                    ? ReadStored(record.ConfigJson) ?? BuildDefaults()
                    : BuildDefaults();

                // Keep profile-owned localization values current, while preserving the
                // administrator-selected date format stored in the organization row.
                var localizationDefaults = BuildDefaults().Localization;
                var storedDateFormat = dto.Localization?.DateFormat;
                var profileDateFormat = Constants.Localization.NormalizeDateFormat(
                    localizationDefaults.DateFormat,
                    Constants.Localization.DayMonthYearDateFormat);
                dto = dto with
                {
                    Localization = localizationDefaults with
                    {
                        DateFormat = Constants.Localization.NormalizeDateFormat(storedDateFormat, profileDateFormat)
                    }
                };

                // 62.11: appsettings is an override, not the source — only PortalBaseUrl has ever
                // needed a per-environment value distinct from the profile pack (preprod uses
                // https://preprod.haragangian.com/portal, the pack carries the production URL).
                // Absent config or an absent key leaves the profile/pack value untouched.
                var portalBaseUrlOverride = configuration?[Constants.ConfigKeys.PortalBaseUrlOverride];
                if (!string.IsNullOrWhiteSpace(portalBaseUrlOverride))
                    dto = dto with { Contact = dto.Contact with { PortalBaseUrl = portalBaseUrlOverride } };

                return dto;
            }))!;
        }

        public async Task UpdateConfigAsync(OrgConfigDto dto, string updatedByAdminId)
        {
            // 34.D20. MemberService gives new members General when it cannot parse this name, so a
            // typo would change every new member's type without anyone noticing. IsDefined stops
            // a number like "99", which TryParse accepts.
            var requestedType = dto.Workflow?.DefaultMembershipType;
            if (!Enum.TryParse<Enums.MembershipType>(requestedType, true, out var defaultType)
                || !Enum.IsDefined(defaultType))
                throw new ArgumentException($"Unknown default membership type \"{requestedType}\".");

            var profileDateFormat = Constants.Localization.NormalizeDateFormat(
                BuildDefaults().Localization.DateFormat,
                Constants.Localization.DayMonthYearDateFormat);
            dto = dto with
            {
                Localization = (dto.Localization ?? new LocalizationDto()) with
                {
                    DateFormat = Constants.Localization.NormalizeDateFormat(
                        dto.Localization?.DateFormat,
                        profileDateFormat)
                },
                // An unknown name saves as null, which falls back to the server setting rather
                // than to a sender that does not exist.
                Workflow = (dto.Workflow ?? new WorkflowDto()) with
                {
                    EmailProvider = EmailSettingsOptions.Normalize(dto.Workflow?.EmailProvider),
                    DefaultMembershipType = defaultType.ToString()
                }
            };

            var record = await db.OrganizationConfigs.FirstOrDefaultAsync(x => x.OrgId == dto.OrgId)
                         ?? new OrganizationConfig { OrgId = dto.OrgId };

            // Spec 023 FR-038. The row only keeps who saved last, so the old election values are
            // read before they are overwritten. A row saved before the Elections section existed
            // reads the DTO defaults, which are the values that were in force.
            var previousElections = record.Id == 0
                ? BuildDefaults().Elections
                : ReadStored(record.ConfigJson)?.Elections ?? new();
            var electionChanges = DiffElectionSettings(previousElections, dto.Elections ?? new());
            // FR-039 (37.13q). Only a save that touches the Elections section is refused.
            if (electionChanges.Count > 0 && freeze is not null)
                await freeze.EnsureNotFrozenAsync(Constants.Elections.FrozenRules.Settings, electionChanges);

            record.OrgId = dto.OrgId;
            record.SchemaVersion = dto.SchemaVersion;
            record.ConfigJson = JsonSerializer.Serialize(dto, JsonOpts);
            record.UpdatedAt = DateTime.UtcNow;
            record.UpdatedByAdminId = updatedByAdminId;
            // App-managed concurrency token: set a fresh value each write (Npgsql/SQLite do not
            // auto-generate it, and NOT NULL would otherwise be violated on insert).
            record.RowVersion = Guid.NewGuid().ToByteArray();

            if (record.Id == 0)
                db.OrganizationConfigs.Add(record);

            // Added before the one SaveChanges, so the config and its audit row commit together.
            if (electionChanges.Count > 0)
            {
                db.ActivityLogs.Add(new ActivityLog
                {
                    ActorId = int.TryParse(updatedByAdminId, out var actorId) ? actorId : null,
                    ActivityType = Constants.Elections.SettingsChangedAuditType,
                    Description = $"Election settings changed: {string.Join(", ", electionChanges.Keys)}.",
                    Timestamp = record.UpdatedAt,
                    Source = Constants.ActivitySources.System,
                    Metadata = JsonSerializer.Serialize(
                        new { updatedBy = updatedByAdminId, changes = electionChanges }, JsonOpts)
                });
            }

            await db.SaveChangesAsync();
            InvalidateCache();
        }

        private void InvalidateCache() => cache.Remove(CacheKey);

        // Compares the serialized form, so a field added to ElectionSettingsDto later is covered
        // without touching this method.
        private static SortedDictionary<string, object?> DiffElectionSettings(
            ElectionSettingsDto before, ElectionSettingsDto after)
        {
            var was = JsonSerializer.SerializeToElement(before, JsonOpts);
            var now = JsonSerializer.SerializeToElement(after, JsonOpts);
            var names = was.EnumerateObject().Select(p => p.Name)
                .Union(now.EnumerateObject().Select(p => p.Name));

            var changes = new SortedDictionary<string, object?>(StringComparer.Ordinal);
            foreach (var name in names)
            {
                var hadOld = was.TryGetProperty(name, out var oldValue);
                var hasNew = now.TryGetProperty(name, out var newValue);
                if (hadOld && hasNew && oldValue.GetRawText() == newValue.GetRawText())
                    continue;
                changes[name] = new
                {
                    from = hadOld ? oldValue.Clone() : (JsonElement?)null,
                    to = hasNew ? newValue.Clone() : (JsonElement?)null
                };
            }
            return changes;
        }

        // 37.13o. A row saved before TwoPersonActions existed has no list, and the DTO default
        // would turn on two-person counting that nobody chose. Such a row keeps the one-person
        // count it had before 37.13h. A saved list is used as it is.
        private static OrgConfigDto? ReadStored(string configJson)
        {
            var dto = JsonSerializer.Deserialize<OrgConfigDto>(configJson, JsonOpts);
            var node = System.Text.Json.Nodes.JsonNode.Parse(configJson, new System.Text.Json.Nodes.JsonNodeOptions { PropertyNameCaseInsensitive = true });
            if (dto is null || node?[nameof(OrgConfigDto.Elections)]?[nameof(ElectionSettingsDto.TwoPersonActions)] is not null)
                return dto;
            var count = nameof(Enums.ElectionApprovalAction.Count);
            return dto with { Elections = dto.Elections with { TwoPersonActions = dto.Elections.TwoPersonActions.Where(a => a != count).ToList() } };
        }

        private OrgConfigDto BuildDefaults() => profiles.OrgConfigDefaults;
    }
}

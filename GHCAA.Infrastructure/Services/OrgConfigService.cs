using System.Text.Json;
using System.Text.Json.Serialization;
using GHCAA.Application.DTOs;
using GHCAA.Application.Interfaces;
using GHCAA.Domain;
using GHCAA.Domain.Models;
using GHCAA.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace GHCAA.Infrastructure.Services
{
    // `profiles` is optional on purpose, and only until ORG_PROFILE is set on the deployments.
    // Work Package 62.6 replaces the hardcoded GHC defaults below with a read of
    // profiles/<ORG_PROFILE>/org-config.json. The pack and the hardcoded values are proven
    // byte-identical by OrgConfigGoldenSnapshotTests, so the swap itself is safe — what is not safe
    // is *selecting* the pack when nobody has said which one. An unset ORG_PROFILE resolves to the
    // neutral "default" sample pack, and serving "Sample Alumni Association" and MEM- numbers to a
    // live association with real members would be a far worse outcome than keeping 190 lines of C#
    // a little longer. So the pack drives configuration only once a profile is explicitly chosen,
    // and until then this behaves exactly as it did before. Once ORG_PROFILE=ghc is set on the
    // deployments, BuildGhcaaDefaults() is dead code and 62.6b deletes it along with this parameter.
    public class OrgConfigService(
        ApplicationDbContext db,
        IMemoryCache cache,
        IInstitutionProfileProvider? profiles = null) : IOrgConfigService
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
                    ? JsonSerializer.Deserialize<OrgConfigDto>(record.ConfigJson, JsonOpts) ?? BuildDefaults()
                    : BuildDefaults();

                // Localization copy has no admin UI to edit it deliberately (org-config admin form
                // only touches Branding/Workflow/Features), but a stored config row round-trips the
                // *entire* DTO on every admin save (UpdateConfigAsync serializes the whole object),
                // so any Localization text captured into a row before a later source-code copy fix
                // (e.g. a corrected Bengali tagline) stays permanently stale otherwise. Since this
                // section is code-owned, always serve the current source value rather than trusting
                // whatever happened to be persisted.
                // 62.6: heal from the active source — the profile pack when one is selected, the
                // hardcoded copy otherwise — rather than always from code.
                return dto with { Localization = BuildDefaults().Localization };
            }))!;
        }

        public async Task UpdateConfigAsync(OrgConfigDto dto, string updatedByAdminId)
        {
            var record = await db.OrganizationConfigs.FirstOrDefaultAsync(x => x.OrgId == dto.OrgId)
                         ?? new OrganizationConfig { OrgId = dto.OrgId };

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

            await db.SaveChangesAsync();
            InvalidateCache();
        }

        private void InvalidateCache() => cache.Remove(CacheKey);

        // The defaults actually in force. Reads the institution profile pack once ORG_PROFILE names
        // one; falls back to the hardcoded GHC copy while it is unset, for the reason given on the
        // class. Both sides are asserted byte-identical by OrgConfigGoldenSnapshotTests, so which
        // branch runs is invisible to callers today — that is what makes the eventual deletion safe.
        private OrgConfigDto BuildDefaults() =>
            profiles is { ProfileExplicitlySelected: true }
                ? profiles.OrgConfigDefaults
                : BuildGhcaaDefaults();

        // Single authoritative source of GHCAA defaults.
        // Used when DB table is empty (first boot before seed completes).
        // Mirrors Constants.cs — those values become the eventual delete target once all callers
        // are migrated to IOrgConfigService.
        private static OrgConfigDto BuildGhcaaDefaults() => new()
        {
            OrgId = "ghcaa",
            SchemaVersion = 1,
            Branding = new()
            {
                ShortName = "GHCAA",
                FullName = "Govt. Haraganga College Alumni Association",
                MemberNickname = "Haragangian",
                InstitutionName = "Govt. Haraganga College",
                InstitutionAcronym = "GHC",
                MembershipNumberPrefix = "GHC-",
                ApprovalSeal = "GHC APPROVED",
                EstablishedOn = "29 Nov 2025",
                LogoUrl = "/assets/logo.png",
                PrimaryColor = "#121212",
                AccentColor = "#c5a059"
            },
            Contact = new()
            {
                SupportEmail = "haragangian@gmail.com",
                ImportEmailBase = "haragangian",
                RegisteredOffice = "Govt. Haraganga College Campus, Munshiganj, Bangladesh.",
                CampusAddress = "Govt. Haraganga College, Munshiganj-1500, Bangladesh.",
                PhoneNumbers = new List<string> { "+880 1711-234567", "+880 1812-345678" },
                PortalBaseUrl = "https://haragangian.com/portal",
                SocialLinks = new() { Facebook = "#", Whatsapp = "#", Youtube = "#" }
            },
            Currency = new() { Code = "BDT", Symbol = "৳", Name = "Bangladeshi Taka" },
            Features = new(),
            Workflow = new()
            {
                MemberApprovalMode = "ManualReview",
                DefaultMembershipType = "General",
                // Derived from enum so adding/removing a type requires only one change in Enums.cs
                MembershipTypes = Enum.GetNames<Enums.MembershipType>().ToList()
            },
            Localization = new()
            {
                DefaultLocale = "en",
                SupportedLocales = ["en", "bn"],
                Locales = new Dictionary<string, LocalePackDto>
                {
                    ["en"] = new()
                    {
                        OrgName = "Govt. Haraganga College Alumni Association",
                        Tagline = "Sharing Heritage, Aligning Lives, Integrating Networks",
                        MemberLabel = "Member",
                        MemberPluralLabel = "Members",
                        MemberNickname = "Haragangian",
                        AlumniLabel = "Alumni",
                        MembershipLabel = "Membership",
                        MembershipTypeLabels = new()
                        {
                            ["Founding"] = "Founding Member",
                            ["Executive"] = "Executive Member",
                            ["General"] = "General Member",
                            ["Associate"] = "Associate Member",
                            ["Honorary"] = "Honorary Member",
                            ["Advisory"] = "Advisory Member",
                            ["Guest"] = "Guest Member"
                        },
                        MemberCategoryLabels = new()
                        {
                            ["None"] = "None",
                            ["LifelongPatron"] = "Lifelong Patron",
                            ["Sponsor"] = "Sponsor",
                            ["Advisor"] = "Advisor",
                            ["Mentor"] = "Mentor",
                            ["Recruiter"] = "Recruiter",
                            ["Active"] = "Active",
                            ["Volunteer"] = "Volunteer",
                            ["Contributor"] = "Contributor",
                            ["Guest"] = "Guest",
                            ["Student"] = "Student"
                        },
                        EcRoleLabels = new()
                        {
                            ["None"] = "None",
                            ["President"] = "President",
                            ["VicePresident"] = "Vice President",
                            ["GeneralSecretary"] = "General Secretary",
                            ["OfficeSecretary"] = "Office Secretary",
                            ["JointSecretary1"] = "Joint Secretary (1)",
                            ["JointSecretary2"] = "Joint Secretary (2)",
                            ["Treasurer"] = "Treasurer",
                            ["MediaCulturalAndSportsSecretary"] = "Media Cultural & Sports Secretary",
                            ["OrganizationalSecretary"] = "Organizational Secretary",
                            ["InformationAndTechnologySecretary"] = "Information and Technology Secretary",
                            ["Member1"] = "Member-1",
                            ["Member2"] = "Member-2",
                            ["LawSecretary"] = "Law Secretary",
                            ["ImmediatePastPresident"] = "Immediate Past President",
                            ["InstitutionalRepresentative"] = "Institutional Representative"
                        },
                        Nav = new()
                        {
                            Administration = "ADMINISTRATION",
                            MyAccount = "MY ACCOUNT",
                            Community = "COMMUNITY",
                            MediaAndTools = "MEDIA & TOOLS",
                            AdminRoleLabel = "ADMINISTRATOR",
                            MemberRoleLabel = "ALUMNI MEMBER",
                            BatchPrefix = "Batch: "
                        },
                        EmailSubjects = new()
                        {
                            PasswordReset = "GHCAA Account Password Reset",
                            OtpVerification = "GHCAA Verification Code",
                            Welcome = "Welcome to GHCAA",
                            FeeReminder = "GHCAA Membership Fee Reminder"
                        }
                    },
                    ["bn"] = new()
                    {
                        OrgName = "সরকারি হারাগঙ্গা কলেজ প্রাক্তন ছাত্রছাত্রী সমিতি",
                        Tagline = "ঐতিহ্যের বিনিময়, জীবনের সমন্বয় ও সংহতির সেতুবন্ধন",
                        MemberLabel = "সদস্য",
                        MemberPluralLabel = "সদস্যগণ",
                        MemberNickname = "হারাগঙ্গিয়ান",
                        AlumniLabel = "প্রাক্তন ছাত্রছাত্রী",
                        MembershipLabel = "সদস্যপদ",
                        MembershipTypeLabels = new()
                        {
                            ["Founding"] = "প্রতিষ্ঠাতা সদস্য",
                            ["Executive"] = "নির্বাহী সদস্য",
                            ["General"] = "সাধারণ সদস্য",
                            ["Associate"] = "সহযোগী সদস্য",
                            ["Honorary"] = "সম্মানসূচক সদস্য",
                            ["Advisory"] = "উপদেষ্টা সদস্য",
                            ["Guest"] = "অতিথি সদস্য"
                        },
                        MemberCategoryLabels = new()
                        {
                            ["None"] = "কোনোটি নয়",
                            ["LifelongPatron"] = "আজীবন পৃষ্ঠপোষক",
                            ["Sponsor"] = "স্পনসর",
                            ["Advisor"] = "উপদেষ্টা",
                            ["Mentor"] = "মেন্টর",
                            ["Recruiter"] = "নিয়োগকর্তা",
                            ["Active"] = "সক্রিয়",
                            ["Volunteer"] = "স্বেচ্ছাসেবক",
                            ["Contributor"] = "অবদানকারী",
                            ["Guest"] = "অতিথি",
                            ["Student"] = "ছাত্র"
                        },
                        EcRoleLabels = new()
                        {
                            ["None"] = "কোনোটি নয়",
                            ["President"] = "সভাপতি",
                            ["VicePresident"] = "সহ-সভাপতি",
                            ["GeneralSecretary"] = "সাধারণ সম্পাদক",
                            ["OfficeSecretary"] = "কার্যালয় সম্পাদক",
                            ["JointSecretary1"] = "যুগ্ম সম্পাদক (১)",
                            ["JointSecretary2"] = "যুগ্ম সম্পাদক (২)",
                            ["Treasurer"] = "কোষাধ্যক্ষ",
                            ["MediaCulturalAndSportsSecretary"] = "মিডিয়া, সাংস্কৃতিক ও ক্রীড়া সম্পাদক",
                            ["OrganizationalSecretary"] = "সাংগঠনিক সম্পাদক",
                            ["InformationAndTechnologySecretary"] = "তথ্য ও প্রযুক্তি সম্পাদক",
                            ["Member1"] = "সদস্য-১",
                            ["Member2"] = "সদস্য-২",
                            ["LawSecretary"] = "আইন সম্পাদক",
                            ["ImmediatePastPresident"] = "সাবেক সভাপতি",
                            ["InstitutionalRepresentative"] = "প্রাতিষ্ঠানিক প্রতিনিধি"
                        },
                        Nav = new()
                        {
                            Administration = "প্রশাসন",
                            MyAccount = "আমার অ্যাকাউন্ট",
                            Community = "সম্প্রদায়",
                            MediaAndTools = "মিডিয়া ও সরঞ্জাম",
                            AdminRoleLabel = "প্রশাসক",
                            MemberRoleLabel = "প্রাক্তন সদস্য",
                            BatchPrefix = "ব্যাচ: "
                        },
                        EmailSubjects = new()
                        {
                            PasswordReset = "GHCAA অ্যাকাউন্ট পাসওয়ার্ড রিসেট",
                            OtpVerification = "GHCAA যাচাইকরণ কোড",
                            Welcome = "GHCAA-তে স্বাগতম",
                            FeeReminder = "GHCAA সদস্যপদ ফি স্মারক"
                        }
                    }
                }
            }
        };
    }
}

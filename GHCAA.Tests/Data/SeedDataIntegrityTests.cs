using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using FluentAssertions;
using GHCAA.Domain.Models;
using NUnit.Framework;

namespace GHCAA.Tests.Data
{
    // Regression coverage for the "Category" vs "ArticleCategory" bug: LoadSeed<T> deserializes
    // with System.Text.Json's default options, which SILENTLY IGNORES any JSON key that doesn't
    // match a property on T (no exception, no warning) — so a stale/renamed field name in a seed
    // file never surfaces until someone notices the wrong data live. Every one of these files is
    // deserialized into a real EF entity at boot via ApplicationDbContext.LoadSeed<T>; this test
    // parses each file's raw JSON keys and asserts every one matches a real public property on
    // its target type (case-insensitively, matching System.Text.Json's own default matching),
    // so a future rename/typo fails a test instead of silently defaulting a column in production.
    [TestFixture]
    public class SeedDataIntegrityTests
    {
        private static readonly (string File, Type EntityType)[] SeedFiles =
        {
            ("lookups.json", typeof(LookupItem)),
            ("email_templates.json", typeof(EmailTemplate)),
            ("roles.json", typeof(Role)),
            ("members.json", typeof(Member)),
            ("users.json", typeof(User)),
            ("payment_histories.json", typeof(PaymentHistory)),
            ("fee_configs.json", typeof(MembershipFeeConfig)),
            ("payment_configurations.json", typeof(PaymentConfiguration)),
            ("ec_periods.json", typeof(ECPeriod)),
            ("ec_members.json", typeof(ECMember)),
            ("news.json", typeof(NewsPost)),
            ("events.json", typeof(AlumniEvent)),
            ("jobs.json", typeof(JobOpportunity)),
            ("themes.json", typeof(SpecialDayTheme)),
            ("galleries.json", typeof(EventGallery)),
            ("photos.json", typeof(EventPhoto)),
            ("academic_records.json", typeof(AcademicRecord)),
            ("professional_records.json", typeof(ProfessionalRecord)),
            ("membership_histories.json", typeof(MembershipHistory)),
            ("membership_dues.json", typeof(MembershipDue)),
            ("financial_records.json", typeof(FinancialRecord)),
            ("file_uploads.json", typeof(FileUpload)),
            ("saved_payment_methods.json", typeof(SavedPaymentMethod)),
            ("site_content.json", typeof(SiteContent)),
        };

        private static string SeedDirectory()
        {
            var dir = AppDomain.CurrentDomain.BaseDirectory;
            for (var i = 0; i < 6; i++)
            {
                var candidate = Path.Combine(dir, "GHCAA.Infrastructure", "Data", "Seed");
                if (Directory.Exists(candidate)) return candidate;
                dir = Path.GetFullPath(Path.Combine(dir, ".."));
            }
            throw new DirectoryNotFoundException("Could not locate GHCAA.Infrastructure/Data/Seed from test output directory.");
        }

        [TestCaseSource(nameof(SeedFiles))]
        public void SeedFile_EveryJsonKey_MatchesARealEntityProperty((string File, Type EntityType) seed)
        {
            var path = Path.Combine(SeedDirectory(), seed.File);
            File.Exists(path).Should().BeTrue($"{seed.File} should exist under Data/Seed");

            var json = File.ReadAllText(path);
            using var doc = JsonDocument.Parse(json);
            doc.RootElement.ValueKind.Should().Be(JsonValueKind.Array, $"{seed.File} is expected to be a JSON array of {seed.EntityType.Name}");

            var validNames = new HashSet<string>(
                seed.EntityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(p => p.Name),
                StringComparer.OrdinalIgnoreCase);

            var unknownKeys = new List<string>();
            foreach (var record in doc.RootElement.EnumerateArray())
            {
                foreach (var prop in record.EnumerateObject())
                {
                    if (!validNames.Contains(prop.Name))
                        unknownKeys.Add(prop.Name);
                }
            }

            unknownKeys.Should().BeEmpty(
                $"every key in {seed.File} must match a public property on {seed.EntityType.Name} — " +
                "System.Text.Json silently drops unmatched keys instead of failing, which is exactly " +
                "how the 'Category' (should be 'ArticleCategory') bug reached production undetected.");
        }
    }
}

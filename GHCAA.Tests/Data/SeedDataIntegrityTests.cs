using System.Text.Json;
using FluentAssertions;
using NUnit.Framework;

namespace GHCAA.Tests.Data
{
    // The May 2026 alumni CSV import (Members/AcademicRecords/PaymentHistories/
    // ProfessionalRecords/Users/UserRoles, seeded both via
    // GHCAA.Infrastructure/Data/Migrations/PgSql/20260828120117_AddMay2026AlumniRegistrationBatch.cs
    // for legacy databases and via these same Data/Seed/*.json files for a fresh EnsureCreated
    // database) already broke referential/uniqueness invariants once this session — a batch member
    // collided with an existing Email/MobileNo/NID/TransactionId and tripped a unique-constraint
    // violation in production. These tests load the seed JSON directly (no DB involved — this is
    // exactly the same data the migration's InsertData calls hard-code) and assert the invariants a
    // unique index or foreign key would enforce, so a future batch import that reintroduces a
    // collision or a dangling reference fails fast in CI instead of on a live deploy.
    [TestFixture]
    public class SeedDataIntegrityTests
    {
        private static readonly string SeedDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Seed");

        private static List<JsonElement> LoadArray(string fileName)
        {
            var path = Path.Combine(SeedDir, fileName);
            File.Exists(path).Should().BeTrue($"seed file {fileName} should be copied to the test output directory");
            using var doc = JsonDocument.Parse(File.ReadAllBytes(path));
            return doc.RootElement.EnumerateArray().Select(e => e.Clone()).ToList();
        }

        private static int GetInt(JsonElement e, string prop) => e.GetProperty(prop).GetInt32();

        private static string? GetString(JsonElement e, string prop)
        {
            var p = e.GetProperty(prop);
            return p.ValueKind == JsonValueKind.Null ? null : p.GetString();
        }

        [Test]
        public void Members_ShouldHave_UniqueIds()
        {
            var members = LoadArray("members.json");
            var ids = members.Select(m => GetInt(m, "Id")).ToList();

            ids.Should().OnlyHaveUniqueItems("MemberConfiguration has no explicit PK duplicate guard beyond the DB itself; a duplicate Id here means two seed rows fight over one primary key");
        }

        [Test]
        public void Members_ShouldHave_UniqueEmails()
        {
            var members = LoadArray("members.json");
            var emails = members.Select(m => GetString(m, "Email")!.Trim().ToLowerInvariant()).ToList();

            emails.Should().OnlyHaveUniqueItems("MemberConfiguration.HasIndex(m => m.Email).IsUnique() will reject the seed batch otherwise");
        }

        [Test]
        public void Members_ShouldHave_UniqueMobileNumbers()
        {
            var members = LoadArray("members.json");
            var phones = members.Select(m => GetString(m, "MobileNo")!.Trim()).ToList();

            phones.Should().OnlyHaveUniqueItems("MemberConfiguration.HasIndex(m => m.MobileNo).IsUnique() will reject the seed batch otherwise");
        }

        [Test]
        public void Members_ShouldHave_UniqueNIDs()
        {
            var members = LoadArray("members.json");
            var nids = members.Select(m => GetString(m, "NID")!.Trim()).ToList();

            nids.Should().OnlyHaveUniqueItems("MemberConfiguration.HasIndex(m => m.NID).IsUnique() will reject the seed batch otherwise");
        }

        [Test]
        public void PaymentHistories_ShouldHave_UniqueTransactionIds()
        {
            var payments = LoadArray("payment_histories.json");
            var transactionIds = payments.Select(p => GetString(p, "TransactionId")!.Trim()).ToList();

            transactionIds.Should().OnlyHaveUniqueItems("PaymentHistoryConfiguration.HasIndex(p => p.TransactionId).IsUnique() is a global (not per-member) constraint");
        }

        [Test]
        public void PaymentHistories_ShouldAllReference_AKnownMember()
        {
            var members = LoadArray("members.json");
            var memberIds = members.Select(m => GetInt(m, "Id")).ToHashSet();

            var payments = LoadArray("payment_histories.json");
            var orphaned = payments.Where(p => !memberIds.Contains(GetInt(p, "MemberId")))
                .Select(p => GetInt(p, "MemberId")).ToList();

            orphaned.Should().BeEmpty("every PaymentHistory.MemberId must point at a seeded Member (PaymentHistoryConfiguration cascades on Member, so a dangling FK breaks insertion)");
        }

        [Test]
        public void AcademicRecords_ShouldAllReference_AKnownMember()
        {
            var members = LoadArray("members.json");
            var memberIds = members.Select(m => GetInt(m, "Id")).ToHashSet();

            var records = LoadArray("academic_records.json");
            var orphaned = records.Where(r => !memberIds.Contains(GetInt(r, "MemberId")))
                .Select(r => GetInt(r, "MemberId")).ToList();

            orphaned.Should().BeEmpty("every AcademicRecord.MemberId must point at a seeded Member");
        }

        [Test]
        public void ProfessionalRecords_ShouldAllReference_AKnownMember()
        {
            var members = LoadArray("members.json");
            var memberIds = members.Select(m => GetInt(m, "Id")).ToHashSet();

            var records = LoadArray("professional_records.json");
            var orphaned = records.Where(r => !memberIds.Contains(GetInt(r, "MemberId")))
                .Select(r => GetInt(r, "MemberId")).ToList();

            orphaned.Should().BeEmpty("every ProfessionalRecord.MemberId must point at a seeded Member");
        }

        [Test]
        public void Users_ShouldHave_UniqueIds_And_UniqueUsernames_And_AllReference_AKnownMember()
        {
            var members = LoadArray("members.json");
            var memberIds = members.Select(m => GetInt(m, "Id")).ToHashSet();

            var users = LoadArray("users.json");
            var userIds = users.Select(u => GetInt(u, "Id")).ToList();
            var usernames = users.Select(u => GetString(u, "Username")!.Trim().ToLowerInvariant()).ToList();
            var orphaned = users.Where(u => !memberIds.Contains(GetInt(u, "MemberId")))
                .Select(u => GetInt(u, "MemberId")).ToList();

            userIds.Should().OnlyHaveUniqueItems();
            usernames.Should().OnlyHaveUniqueItems("Users.Username is the login key and must be unique across the whole seed batch");
            orphaned.Should().BeEmpty("every User.MemberId must point at a seeded Member");
        }

        [Test]
        public void UserRoles_ShouldAllReference_AKnownUser_AndAKnownRole()
        {
            var users = LoadArray("users.json");
            var userIds = users.Select(u => GetInt(u, "Id")).ToHashSet();

            var roles = LoadArray("roles.json");
            var roleIds = roles.Select(r => GetInt(r, "Id")).ToHashSet();

            var userRoles = LoadArray("user_roles.json");

            var orphanedUsers = userRoles.Where(ur => !userIds.Contains(GetInt(ur, "UsersId")))
                .Select(ur => GetInt(ur, "UsersId")).ToList();
            var orphanedRoles = userRoles.Where(ur => !roleIds.Contains(GetInt(ur, "RolesId")))
                .Select(ur => GetInt(ur, "RolesId")).ToList();

            orphanedUsers.Should().BeEmpty("every UserRoles.UsersId must point at a seeded User");
            orphanedRoles.Should().BeEmpty("every UserRoles.RolesId must point at a seeded Role");
        }
    }
}

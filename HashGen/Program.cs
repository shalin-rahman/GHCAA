using System;
using System.Net.Http;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.AspNetCore.SignalR.Client;
using Npgsql;

const string connStr = "Host=localhost;Port=5432;Database=GHCAADB_v2;Username=postgres;Password=postgres;SslMode=Prefer";

var shalinHash = BCrypt.Net.BCrypt.HashPassword("Shalin@2024!", 11);
var demoHash = BCrypt.Net.BCrypt.HashPassword("DemoPass123!", 11);

if (args.Length > 0 && args[0] == "--apply")
{
    await using var conn = new NpgsqlConnection(connStr);
    await conn.OpenAsync();

    int memberId;
    await using (var resolve = new NpgsqlCommand("""
        SELECT m."Id" FROM "Members" m
        WHERE m."IsArchived" = false AND m."Status" = 1
          AND NOT EXISTS (
            SELECT 1 FROM "Users" u
            WHERE u."MemberId" = m."Id" AND u."IsArchived" = false
          )
        ORDER BY m."Id" LIMIT 1
        """, conn))
    {
        var result = await resolve.ExecuteScalarAsync();
        memberId = result is int id ? id : 0;
    }

    if (memberId == 0)
    {
        await using var createMember = new NpgsqlCommand("""
            INSERT INTO "Members" (
                "Id", "FullName", "FatherName", "MotherName", "DateOfBirth", "Gender", "BloodGroup",
                "NID", "MobileNo", "Email", "EmailVerified", "PresentAddress", "PermanentAddress",
                "EmergencyContactName", "EmergencyContactRelation", "EmergencyContactPhone",
                "Status", "AppliedDate", "ApprovedDate", "MembershipNumber", "IsArchived",
                "LastUpdateDate", "MembershipType", "Category", "HasAcceptedTerms", "IsVerified",
                "IsMobilePublic", "IsEmailPublic", "IsAddressPublic", "IsNIDPublic", "IsFamilyPublic",
                "HasAcceptedGdpr", "IsProfileComplete", "ContributionPoints",
                "NotifyEventCreation", "NotifyParticipationApproval", "NotifyRegistrationUpdate", "NotifyRelevantUpdates", "NotifyCommitteeChanges"
            )
            SELECT 9998, 'Demo User', 'Father', 'Mother', '1995-01-01', 0, 2,
                '9999999999', '01999999999', 'demo_user@ghcaa.local', true, 'Munshiganj', 'Munshiganj',
                'Emergency', 'Family', '01999999998',
                1, NOW() AT TIME ZONE 'UTC', NOW() AT TIME ZONE 'UTC', 'GHC-DEMO-0001', false,
                NOW() AT TIME ZONE 'UTC', 3, 0, true, true,
                false, false, false, false, false,
                false, true, 0,
                true, true, true, true, true
            WHERE NOT EXISTS (SELECT 1 FROM "Members" WHERE "Id" = 9998)
            """, conn);
        await createMember.ExecuteNonQueryAsync();
        memberId = 9998;
    }

    Console.WriteLine($"demo_user MemberId={memberId}");

    await using (var updateShalin = new NpgsqlCommand("""
        UPDATE "Users"
        SET "PasswordHash" = @hash, "FailedLoginAttempts" = 0, "LockoutUntil" = NULL
        WHERE "Username" = 'shalin'
        """, conn))
    {
        updateShalin.Parameters.AddWithValue("hash", shalinHash);
        Console.WriteLine($"shalin updated: {await updateShalin.ExecuteNonQueryAsync()} row(s)");
    }

    await using (var upsertDemo = new NpgsqlCommand("""
        INSERT INTO "Users" ("Id", "Username", "PasswordHash", "MemberId", "CreatedAt", "IsActive", "IsArchived", "MustChangePassword", "SecurityStamp", "FailedLoginAttempts")
        SELECT 9999, 'demo_user', @hash, @memberId, NOW() AT TIME ZONE 'UTC', true, false, false, gen_random_uuid()::text, 0
        WHERE NOT EXISTS (SELECT 1 FROM "Users" WHERE "Username" = 'demo_user');

        UPDATE "Users"
        SET "PasswordHash" = @hash,
            "MemberId" = COALESCE("MemberId", @memberId),
            "FailedLoginAttempts" = 0,
            "LockoutUntil" = NULL,
            "IsActive" = true
        WHERE "Username" = 'demo_user';

        INSERT INTO "UserRoles" ("RolesId", "UsersId")
        SELECT 3, u."Id"
        FROM "Users" u
        WHERE u."Username" = 'demo_user'
          AND NOT EXISTS (
            SELECT 1 FROM "UserRoles" ru WHERE ru."UsersId" = u."Id" AND ru."RolesId" = 3
          );
        """, conn))
    {
        upsertDemo.Parameters.AddWithValue("hash", demoHash);
        upsertDemo.Parameters.AddWithValue("memberId", memberId);
        Console.WriteLine($"demo_user upserted: {await upsertDemo.ExecuteNonQueryAsync()} row(s)");
    }

    await using (var seedForum = new NpgsqlCommand("""
        INSERT INTO "ForumCategories" ("Name", "Description", "SortOrder", "IsActive")
        SELECT 'General Discussion', 'Local dev smoke-test category', 1, true
        WHERE NOT EXISTS (SELECT 1 FROM "ForumCategories")
        """, conn))
    {
        Console.WriteLine($"forum category seeded: {await seedForum.ExecuteNonQueryAsync()} row(s)");
    }

    Console.WriteLine("Done.");
}
else if (args.Length > 0 && args[0] == "--signalr-test")
{
    using var http = new HttpClient { BaseAddress = new Uri("http://localhost:5087") };
    var loginJson = """{"username":"demo_user","password":"DemoPass123!"}""";
    var loginResp = await http.PostAsync("/api/auth/login", new StringContent(loginJson, System.Text.Encoding.UTF8, "application/json"));
    loginResp.EnsureSuccessStatusCode();
    var doc = System.Text.Json.JsonDocument.Parse(await loginResp.Content.ReadAsStringAsync());
    var token = doc.RootElement.GetProperty("token").GetString()!;

    async Task TestHub(string name, string path)
    {
        var conn = new HubConnectionBuilder()
            .WithUrl($"http://localhost:5087{path}", o => o.AccessTokenProvider = () => Task.FromResult<string?>(token))
            .Build();
        await conn.StartAsync();
        Console.WriteLine($"{name}: connected");
        await conn.StopAsync();
        await conn.DisposeAsync();
    }

    await TestHub("ChatHub", "/api/hubs/chat");
    await TestHub("NotificationHub", "/api/hubs/notifications");
    Console.WriteLine("SignalR smoke OK.");
}
else
{
    Console.WriteLine("Shalin:" + shalinHash);
    Console.WriteLine("Demo:" + demoHash);
    Console.WriteLine("Run with --apply to update local DB.");
}

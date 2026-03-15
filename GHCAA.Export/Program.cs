using Dapper;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Text.Json;
using System.Text.Json.Serialization;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;

var config = new ConfigurationBuilder()
    .AddJsonFile(@"c:\Users\HabiburRahmanShalin\workstation\shaleen\shalin\GHC\Application\GHCAA\GHCAA.API\appsettings.json")
    .Build();

var connString = config.GetConnectionString("PgSqlConnection");
using var conn = new NpgsqlConnection(connString);
conn.Open();

var seedDir = @"c:\Users\HabiburRahmanShalin\workstation\shaleen\shalin\GHC\Application\GHCAA\GHCAA.Infrastructure\Data\Seed";
var photoSourceDir = @"C:\Users\HabiburRahmanShalin\workstation\shaleen\shalin\GHC\alumni\Photo-582";
var photoTargetRoot = @"c:\Users\HabiburRahmanShalin\workstation\shaleen\shalin\GHC\Application\GHCAA\GHCAA.API\wwwroot\uploads\members\seed";

Directory.CreateDirectory(seedDir);
Directory.CreateDirectory(photoTargetRoot);

var jsonOptions = new JsonSerializerOptions 
{ 
    WriteIndented = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
};

async Task Export(string table, string fileName, string? where = null)
{
    var sql = $"SELECT * FROM \"{table}\"";
    if (!string.IsNullOrEmpty(where)) sql += $" WHERE {where}";
    var data = await conn.QueryAsync(sql);
    var json = JsonSerializer.Serialize(data, jsonOptions);
    await File.WriteAllTextAsync(Path.Combine(seedDir, fileName), json);
    Console.WriteLine($"Exported {table} to {fileName}");
}

Console.WriteLine("Starting Direct Export...");

// Configs
await Export("Roles", "roles.json");
await Export("Lookups", "lookups.json");
await Export("EmailTemplates", "email_templates.json");
await Export("MembershipFeeConfigs", "fee_configs.json");
await Export("PaymentConfigurations", "payment_configurations.json");
await Export("SpecialDayThemes", "themes.json");

// Governance
await Export("ECPeriods", "ec_periods.json");
await Export("ECMembers", "ec_members.json");

// Members (Handle photos)
var members = (await conn.QueryAsync("SELECT * FROM \"Members\" WHERE \"FullName\" NOT LIKE '%John Doe%'")).ToList();
var memberIds = members.Select(m => (int)m.Id).ToHashSet();

Console.WriteLine("Processing member photos...");
int photoCount = 0;
foreach(var m in members)
{
    string nid = m.NID?.ToString() ?? "";
    if (string.IsNullOrEmpty(nid)) continue;

    string[] exts = { ".jpeg", ".jpg", ".png" };
    string? src = null;
    foreach(var e in exts) {
        var p = Path.Combine(photoSourceDir, nid + e);
        if (File.Exists(p)) { src = p; break; }
    }

    if (src != null) {
        var targetFile = $"{nid}.jpg";
        var dest = Path.Combine(photoTargetRoot, targetFile);
        try {
            using var img = await Image.LoadAsync(src);
            var enc = new JpegEncoder { Quality = 85 };
            using var ms = new MemoryStream();
            await img.SaveAsJpegAsync(ms, enc);
            if (ms.Length > 351 * 1024) enc = new JpegEncoder { Quality = 70 };
            await img.SaveAsJpegAsync(dest, enc);
            // In Dapper/dynamic, we might need to handle the dictionary if it's dynamic
            var mDict = (IDictionary<string, object>)m;
            mDict["PhotoPath"] = $"uploads/members/seed/{targetFile}";
            photoCount++;
        } catch { }
    }
}
await File.WriteAllTextAsync(Path.Combine(seedDir, "members.json"), JsonSerializer.Serialize(members, jsonOptions));
Console.WriteLine($"Exported Members with {photoCount} photos");

// Users & UserRoles
var users = await conn.QueryAsync("SELECT * FROM \"Users\"");
var filteredUsers = users.Where(u => u.MemberId != null && memberIds.Contains((int)u.MemberId)).ToList();
await File.WriteAllTextAsync(Path.Combine(seedDir, "users.json"), JsonSerializer.Serialize(filteredUsers, jsonOptions));
Console.WriteLine($"Exported {filteredUsers.Count} users");

var userRoles = await conn.QueryAsync("SELECT * FROM \"UserRoles\"");
var exportedUserIds = filteredUsers.Select(u => (int)u.Id).ToHashSet();
var filteredUserRoles = userRoles.Where(ur => exportedUserIds.Contains((int)ur.UsersId)).ToList();
await File.WriteAllTextAsync(Path.Combine(seedDir, "user_roles.json"), JsonSerializer.Serialize(filteredUserRoles, jsonOptions));
Console.WriteLine($"Exported {filteredUserRoles.Count} UserRoles");

// Related Data
await Export("AcademicRecords", "academic_records.json", "\"MemberId\" IN (" + string.Join(",", memberIds) + ")");
await Export("ProfessionalRecords", "professional_records.json", "\"MemberId\" IN (" + string.Join(",", memberIds) + ")");
await Export("MembershipHistories", "membership_histories.json", "\"MemberId\" IN (" + string.Join(",", memberIds) + ")");
await Export("MembershipDues", "membership_dues.json", "\"MemberId\" IN (" + string.Join(",", memberIds) + ")");
await Export("PaymentHistories", "payment_histories.json", "\"MemberId\" IN (" + string.Join(",", memberIds) + ")");
await Export("FinancialRecords", "financial_records.json");

// Content
await Export("AlumniEvents", "events.json");
await Export("EventGalleries", "galleries.json");
await Export("EventPhotos", "photos.json");
await Export("NewsPosts", "news.json");
await Export("JobOpportunities", "jobs.json");
await Export("FileUploads", "file_uploads.json", "\"MemberId\" IN (" + string.Join(",", memberIds) + ")");

Console.WriteLine("Direct Export Completed!");

param (
    [string]$Username = "shalin"
)

$RootPath = $PSScriptRoot
$ConnectionString = "Host=localhost;Port=5432;Database=GHCAADB_v2;Username=postgres;Password=postgres;SslMode=Prefer;Trust Server Certificate=True;"

# Use npsql (if installed) or just run psql if available
# Since this is Windows, psql might be in PATH if PostgreSQL is installed.
# Alternatively, I can use a small C# script to query the database.

$code = @"
using System;
using Npgsql;

public class Program {
    public static void Main() {
        string connString = "Host=localhost;Port=5432;Database=GHCAADB_v2;Username=postgres;Password=postgres;SslMode=Prefer;Trust Server Certificate=True;";
        using (var conn = new NpgsqlConnection(connString)) {
            conn.Open();
            using (var cmd = new NpgsqlCommand("SELECT \"Id\", \"Username\", \"PasswordHash\" FROM \"Users\"", conn)) {
                using (var reader = cmd.ExecuteReader()) {
                    while (reader.Read()) {
                        Console.WriteLine($"ID: {reader[0]}, Username: {reader[1]}");
                    }
                }
            }
        }
    }
}
"@

# I don't want to deal with dependencies for a small check.
# I'll just check if psql exists.
if (Get-Command psql -ErrorAction SilentlyContinue) {
    & psql -h localhost -U postgres -d GHCAADB_v2 -c "SELECT \"Id\", \"Username\" FROM \"Users\" WHERE \"Username\" = '$Username';"
} else {
    Write-Host "psql not found. Trying to list users via JSON seed files again."
    # If the user is being seeded, it MUST be in one of the files.
}

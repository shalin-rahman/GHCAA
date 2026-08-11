# Apply local test password fixes (ENV-005) via Npgsql
$ErrorActionPreference = "Stop"
$repoRoot = Split-Path $PSScriptRoot -Parent
$connStr = "Host=localhost;Port=5432;Database=GHCAADB_v2;Username=postgres;Password=postgres;SslMode=Prefer"
$sqlPath = Join-Path $PSScriptRoot "fix-local-test-passwords.sql"

dotnet build "$repoRoot\GHCAA.Infrastructure\GHCAA.Infrastructure.csproj" -v q | Out-Null
$npgsqlDll = Get-ChildItem -Path $repoRoot -Recurse -Filter "Npgsql.dll" |
    Where-Object { $_.FullName -match "net9\.0" } | Select-Object -First 1

if (-not $npgsqlDll) { throw "Npgsql.dll not found after build." }

Add-Type -Path $npgsqlDll.FullName

$sql = Get-Content $sqlPath -Raw
$conn = New-Object Npgsql.NpgsqlConnection($connStr)
$conn.Open()
try {
    $resolveCmd = $conn.CreateCommand()
    $resolveCmd.CommandText = @'
SELECT m."Id" FROM "Members" m
LEFT JOIN "Users" u ON u."MemberId" = m."Id" AND u."IsArchived" = false
WHERE m."IsArchived" = false AND m."Status" = 1 AND u."Id" IS NULL
ORDER BY m."Id" LIMIT 1;
'@
    $memberId = $resolveCmd.ExecuteScalar()
    if (-not $memberId) {
        $memberId = 200
        Write-Host "No orphan member found; using MemberId=$memberId" -ForegroundColor Yellow
    } else {
        Write-Host "Linking demo_user to MemberId=$memberId" -ForegroundColor Green
    }

    $sql = $sql.Replace('MemberId", 1,', "MemberId"", $memberId,")
    $sql = $sql.Replace('COALESCE("MemberId", 1)', "COALESCE(""MemberId"", $memberId)")

    $cmd = $conn.CreateCommand()
    $cmd.CommandText = $sql
    [void]$cmd.ExecuteNonQuery()
    Write-Host "Password fix applied successfully." -ForegroundColor Green
} finally {
    $conn.Close()
}

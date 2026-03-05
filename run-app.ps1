param (
    [switch]$SkipTests,
    [switch]$NoTest,
    [ValidateSet("PgSql","MySql","Sqlite","")]
    [string]$DatabaseProvider = ""
)

$RootPath = $PSScriptRoot

# ── Banner ─────────────────────────────────────────────────────────────────
Clear-Host
Write-Host ""
Write-Host "  ============================================================" -ForegroundColor DarkCyan
Write-Host "    GHCAA Platform  |  Launcher" -ForegroundColor Cyan
Write-Host "  ============================================================" -ForegroundColor DarkCyan
Write-Host ""

# ── Database Provider Override ──────────────────────────────────────────────
if ($DatabaseProvider -ne "") {
    $env:GHCAA__DatabaseProvider = $DatabaseProvider
    Write-Host "  [DB] Provider overridden to: $DatabaseProvider" -ForegroundColor Yellow
    Write-Host ""
}

# ── Stop Existing Processes ─────────────────────────────────────────────────
Write-Host "  Stopping any existing GHCAA processes..." -ForegroundColor DarkGray
Get-Process dotnet -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Get-Process node -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Write-Host "  [OK] Workspace cleared" -ForegroundColor Green
Write-Host ""

Start-Sleep -Seconds 2

# ── Resolve Provider ────────────────────────────────────────────────────────
$activeProvider = $DatabaseProvider
if (-not $activeProvider) {
    $appSettingsPath = Join-Path $RootPath "GHCAA.API\appsettings.json"
    if (Test-Path $appSettingsPath) {
        $config = Get-Content $appSettingsPath | ConvertFrom-Json
        $activeProvider = $config.DatabaseProvider
    }
}
if (-not $activeProvider) { $activeProvider = "PgSql" }

# ── Database Migration ──────────────────────────────────────────────────────
Write-Host "  Applying database migrations for $activeProvider..." -ForegroundColor Yellow
$contextName = "${activeProvider}ApplicationDbContext"
Push-Location $RootPath
dotnet ef database update --context $contextName --project GHCAA.Infrastructure --startup-project GHCAA.API
if ($LASTEXITCODE -eq 0) {
    Write-Host "  [OK] Database is up to date" -ForegroundColor Green
} else {
    Write-Host "  [!!] Migration failed. Check ConnectionStrings." -ForegroundColor Red
}
Pop-Location
Write-Host ""

# ── Unit Tests ──────────────────────────────────────────────────────────────
$skipAllTests = $SkipTests -or $NoTest
if (-not $skipAllTests) {
    Write-Host "  Running Backend Unit Tests..." -ForegroundColor Yellow
    Push-Location $RootPath
    dotnet test GHCAA.Tests/GHCAA.Tests.csproj --logger "console;verbosity=normal"
    Pop-Location
    Write-Host ""
}

# ── Launch API ──────────────────────────────────────────────────────────────
Write-Host "  Launching Backend API  (https://localhost:7084)..." -ForegroundColor Yellow
Start-Process dotnet -ArgumentList "run --project GHCAA.API/GHCAA.API.csproj --launch-profile https" -WorkingDirectory $RootPath

Start-Sleep -Seconds 3

# ── Launch Frontend ─────────────────────────────────────────────────────────
Write-Host "  Launching Frontend    (http://localhost:4200)..." -ForegroundColor Yellow
Start-Process cmd -ArgumentList "/c npm start" -WorkingDirectory "$RootPath\GHCAA.Web"

# ── Summary ─────────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "  ============================================================" -ForegroundColor DarkCyan
Write-Host "    Services are starting in separate windows." -ForegroundColor Green
Write-Host "  ============================================================" -ForegroundColor DarkCyan
Write-Host "    API      : https://localhost:7084/swagger"
Write-Host "    Web      : http://localhost:4200"
Write-Host "    Admin    : shalin / shalin" -ForegroundColor Yellow
Write-Host "  ============================================================" -ForegroundColor DarkCyan
Write-Host ""

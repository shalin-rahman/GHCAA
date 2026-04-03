param (
    [switch]$RunTests,
    [ValidateSet("PgSql","MySql","Sqlite","")]
    [string]$DatabaseProvider = ""
)

$ToolsPath = $PSScriptRoot
$RootPath = Split-Path $ToolsPath -Parent

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
Write-Host "  [OK] Close any existing terminal windows or stop the processes." -ForegroundColor Yellow
& "$ToolsPath\stop-app.ps1" -Force
Write-Host "  [OK] Workspace cleared for new session" -ForegroundColor Green
Write-Host ""

Start-Sleep -Seconds 1

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
if ($RunTests) {
    Write-Host "  Running Backend Unit Tests..." -ForegroundColor Yellow
    Push-Location $RootPath
    dotnet test GHCAA.Tests/GHCAA.Tests.csproj --logger "console;verbosity=normal"
    if ($LASTEXITCODE -ne 0) {
        Write-Host "  [!!] Backend unit tests failed! Aborting startup sequence." -ForegroundColor Red
        Pop-Location
        exit $LASTEXITCODE
    }
    Pop-Location
    Write-Host ""
}

# ── Launch API ──────────────────────────────────────────────────────────────
Write-Host "  Launching Backend API  (https://localhost:7214)..." -ForegroundColor Yellow
Start-Process dotnet -ArgumentList "run --project GHCAA.API/GHCAA.API.csproj --launch-profile https" -WorkingDirectory $RootPath

Start-Sleep -Seconds 5

# ── Launch Frontend ─────────────────────────────────────────────────────────
Write-Host "  Launching Frontend    (http://localhost:4200)..." -ForegroundColor Yellow
Start-Process cmd -ArgumentList "/c npm start" -WorkingDirectory "$RootPath\GHCAA.Web"

# ── Launch Mobile Portal ────────────────────────────────────────────────────
Write-Host "  Launching Mobile App  (Flutter Emulator/Device)..." -ForegroundColor Yellow
Start-Process cmd -ArgumentList "/c detect-and-run-mobile.bat" -WorkingDirectory $ToolsPath

# ── Summary ─────────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "  ============================================================" -ForegroundColor DarkCyan
Write-Host "    Services are starting in separate windows." -ForegroundColor Green
Write-Host "  ============================================================" -ForegroundColor DarkCyan
Write-Host "    API      : https://localhost:7214/swagger"
Write-Host "    Web      : http://localhost:4200"
Write-Host "    Mobile   : Deployed via GHCAA.Mobile Engine"
Write-Host "    Admin    : shalin / shalin" -ForegroundColor Yellow
Write-Host "  ============================================================" -ForegroundColor DarkCyan
Write-Host ""

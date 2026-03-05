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
$dotnetStopped = $false
$nodeStopped   = $false

Get-Process dotnet -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
if ($?) { $dotnetStopped = $true }

Get-Process node -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
if ($?) { $nodeStopped = $true }

if ($dotnetStopped) { Write-Host "  [OK] Stopped .NET processes"  -ForegroundColor Green  }
else                { Write-Host "  [--] No .NET processes found"  -ForegroundColor DarkGray }
if ($nodeStopped)   { Write-Host "  [OK] Stopped Node processes"   -ForegroundColor Green  }
else                { Write-Host "  [--] No Node processes found"   -ForegroundColor DarkGray }

Start-Sleep -Seconds 2
Write-Host ""

# ── Database Migration ──────────────────────────────────────────────────────
Write-Host "  Applying database migrations..." -ForegroundColor Yellow
Push-Location $RootPath
dotnet ef database update --project GHCAA.Infrastructure --startup-project GHCAA.API
if ($LASTEXITCODE -eq 0) {
    Write-Host "  [OK] Database is up to date" -ForegroundColor Green
} else {
    Write-Host "  [!!] Migration failed — check ConnectionStrings in appsettings.json" -ForegroundColor Red
    Write-Host "       Set DatabaseProvider to PgSql, MySql, or Sqlite" -ForegroundColor DarkYellow
    Write-Host "       Continuing to launch anyway..." -ForegroundColor DarkGray
}
Pop-Location
Write-Host ""

# ── Unit Tests ──────────────────────────────────────────────────────────────
$skipAllTests = $SkipTests -or $NoTest
if (-not $skipAllTests) {
    Write-Host "  Running Backend Unit Tests..." -ForegroundColor Yellow
    Push-Location $RootPath
    dotnet test GHCAA.Tests/GHCAA.Tests.csproj --logger "console;verbosity=normal"
    $beExit = $LASTEXITCODE
    Pop-Location

    if ($beExit -eq 0) {
        Write-Host "  [OK] All backend tests passed" -ForegroundColor Green
    } else {
        Write-Host "  [!!] Some backend tests FAILED. Review before continuing." -ForegroundColor Red
        Read-Host "  Press Enter to continue anyway or Ctrl+C to abort"
    }
    Write-Host ""

    Write-Host "  Running Frontend Unit Tests..." -ForegroundColor Yellow
    Push-Location "$RootPath\GHCAA.Web"
    cmd /c npm run test -- --watch=false
    $feExit = $LASTEXITCODE
    Pop-Location

    if ($feExit -eq 0) {
        Write-Host "  [OK] All frontend tests passed" -ForegroundColor Green
    } else {
        Write-Host "  [!!] Some frontend tests FAILED. Review before continuing." -ForegroundColor Red
        Read-Host "  Press Enter to continue anyway or Ctrl+C to abort"
    }
    Write-Host ""
} else {
    Write-Host "  [--] Tests skipped (-SkipTests)" -ForegroundColor DarkGray
    Write-Host ""
}

# ── Launch API ──────────────────────────────────────────────────────────────
Write-Host "  Launching Backend API  (https://localhost:7084)..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList @(
    "-NoExit",
    "-Command",
    "& { `$Host.UI.RawUI.WindowTitle='GHCAA - API'; Write-Host 'GHCAA API Server' -ForegroundColor Cyan; Set-Location '$RootPath'; dotnet run --project GHCAA.API/GHCAA.API.csproj --launch-profile https }"
)

Start-Sleep -Seconds 3

# ── Launch Frontend ─────────────────────────────────────────────────────────
Write-Host "  Launching Frontend    (http://localhost:4200)..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList @(
    "-NoExit",
    "-Command",
    "& { `$Host.UI.RawUI.WindowTitle='GHCAA - Web'; Write-Host 'GHCAA Angular Frontend' -ForegroundColor Cyan; Set-Location '$RootPath\GHCAA.Web'; cmd /c npm start }"
)

# ── Summary ─────────────────────────────────────────────────────────────────
Write-Host ""
Write-Host "  ============================================================" -ForegroundColor DarkCyan
Write-Host "    Services are starting in separate windows." -ForegroundColor Green
Write-Host "  ============================================================" -ForegroundColor DarkCyan
Write-Host "    API      " -NoNewline; Write-Host "https://localhost:7084"        -ForegroundColor Cyan
Write-Host "    Swagger  " -NoNewline; Write-Host "https://localhost:7084/swagger" -ForegroundColor Cyan
Write-Host "    Frontend " -NoNewline; Write-Host "http://localhost:4200"          -ForegroundColor Cyan
Write-Host "  ------------------------------------------------------------" -ForegroundColor DarkCyan
Write-Host "    Admin Login  :  shalin / shalin" -ForegroundColor Yellow
Write-Host "  ------------------------------------------------------------" -ForegroundColor DarkCyan
Write-Host "    Usage: .\run-app.ps1 [-SkipTests] [-DatabaseProvider PgSql|MySql|Sqlite]"
Write-Host "           .\stop-app.ps1  — to stop all services"
Write-Host "  ============================================================" -ForegroundColor DarkCyan
Write-Host ""

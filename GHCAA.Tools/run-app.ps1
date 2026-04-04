param (
    [switch]$RunTests,
    [switch]$NoWeb,      # Skip Angular (npm start)
    [switch]$NoMobile,   # Skip Flutter launcher (detect-and-run-mobile.bat)
    [ValidateSet("PgSql","MySql","Sqlite","")]
    [string]$DatabaseProvider = ""
)

$ToolsPath = $PSScriptRoot
$RootPath = Split-Path $ToolsPath -Parent
$ApiPath = Join-Path $RootPath "GHCAA.API"
$WebPath = Join-Path $RootPath "GHCAA.Web"

if (-not (Test-Path $ApiPath)) {
    Write-Error "GHCAA.API folder not found at: $ApiPath (run from repo with GHCAA.Tools inside it)."
    exit 1
}
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Error "'dotnet' not found on PATH. Install .NET SDK 9 and retry."
    exit 1
}

# -- Banner
Clear-Host
Write-Host ""
Write-Host "  ============================================================" -ForegroundColor DarkCyan
Write-Host "    GHCAA Platform  |  Launcher" -ForegroundColor Cyan
Write-Host "  ============================================================" -ForegroundColor DarkCyan
Write-Host ""

# -- Database Provider Override
if ($DatabaseProvider -ne "") {
    $env:DatabaseProvider = $DatabaseProvider
    Write-Host "  [DB] Provider overridden to: $DatabaseProvider (env: DatabaseProvider)" -ForegroundColor Yellow
    Write-Host ""
} else {
    # Avoid a stale DatabaseProvider from an earlier run in this same PowerShell session
    Remove-Item Env:DatabaseProvider -ErrorAction SilentlyContinue
}

# -- Secrets / .env hint
$envFile = Join-Path $ApiPath ".env"
$envExample = Join-Path $ApiPath ".env.example"
if (-not (Test-Path $envFile)) {
    Write-Host "  [--] No GHCAA.API\.env - API loads DotNetEnv from the API folder cwd." -ForegroundColor Yellow
    Write-Host "      Copy .env.example -> .env or run: .\GHCAA.Tools\configure-local-env.ps1" -ForegroundColor DarkGray
    Write-Host "      Or use: cd GHCAA.API ; dotnet user-secrets list" -ForegroundColor DarkGray
    Write-Host ""
}

# -- Stop Existing Processes
Write-Host "  Stopping any existing GHCAA processes..." -ForegroundColor DarkGray
& "$ToolsPath\stop-app.ps1" -Force
Write-Host "  [OK] Workspace cleared for new session" -ForegroundColor Green
Write-Host ""

Start-Sleep -Seconds 1

# -- Resolve Provider
$activeProvider = $DatabaseProvider
if (-not $activeProvider) {
    $appSettingsPath = Join-Path $ApiPath "appsettings.json"
    if (Test-Path $appSettingsPath) {
        $config = Get-Content $appSettingsPath -Raw | ConvertFrom-Json
        $activeProvider = $config.DatabaseProvider
    }
}
if (-not $activeProvider) { $activeProvider = "PgSql" }

# -- Database Migration
Write-Host "  Applying database migrations for $activeProvider..." -ForegroundColor Yellow
$contextName = "${activeProvider}ApplicationDbContext"
Push-Location $RootPath
try {
    dotnet ef database update --context $contextName --project GHCAA.Infrastructure --startup-project GHCAA.API
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  [OK] Database is up to date" -ForegroundColor Green
    } else {
        Write-Host "  [!!] Migration failed. Check ConnectionStrings or DATABASE_URL in .env / User Secrets." -ForegroundColor Red
    }
} finally {
    Pop-Location
}
Write-Host ""

# -- Unit Tests
if ($RunTests) {
    Write-Host "  Running Backend Unit Tests..." -ForegroundColor Yellow
    Push-Location $RootPath
    try {
        dotnet test GHCAA.Tests/GHCAA.Tests.csproj --logger "console;verbosity=normal"
        if ($LASTEXITCODE -ne 0) {
            Write-Host "  [!!] Backend unit tests failed! Aborting startup sequence." -ForegroundColor Red
            exit $LASTEXITCODE
        }
    } finally {
        Pop-Location
    }
    Write-Host ""
}

# -- Launch API (cwd = GHCAA.API so DotNetEnv finds .env)
Write-Host "  Launching Backend API  (https://localhost:7214, http://localhost:5087)..." -ForegroundColor Yellow
Write-Host "  [i] Working directory: GHCAA.API (for .env loading)" -ForegroundColor DarkGray
# When overriding DatabaseProvider, use cmd so the child dotnet process always sees the variable
# (Start-Process env inheritance can be inconsistent across hosts).
if ($DatabaseProvider -ne "") {
    $dp = $DatabaseProvider
    Start-Process -FilePath "cmd.exe" -ArgumentList @(
        "/c",
        "cd /d `"$ApiPath`" && set DatabaseProvider=$dp&& dotnet run --launch-profile https"
    )
} else {
    Start-Process -FilePath "dotnet" -ArgumentList @("run", "--launch-profile", "https") -WorkingDirectory $ApiPath
}

Start-Sleep -Seconds 5

# -- Launch Frontend
if ($NoWeb) {
    Write-Host "  [--] Skipping Web (-NoWeb): start manually with: cd GHCAA.Web ; npm start" -ForegroundColor DarkGray
} elseif (Test-Path $WebPath) {
    Write-Host "  Launching Frontend    (http://localhost:4200)..." -ForegroundColor Yellow
    Start-Process cmd -ArgumentList "/c npm start" -WorkingDirectory $WebPath
} else {
    Write-Host "  [--] GHCAA.Web not found; skipping Angular (path: $WebPath)" -ForegroundColor Yellow
}

# -- Launch Mobile Portal
if ($NoMobile) {
    Write-Host "  [--] Skipping Mobile (-NoMobile): run GHCAA.Tools\detect-and-run-mobile.bat when needed" -ForegroundColor DarkGray
} else {
    Write-Host "  Launching Mobile App  (Flutter Emulator/Device)..." -ForegroundColor Yellow
    # run-app-mobile.bat skips its own `dotnet run` when this is set (avoids duplicate API on 7214/5087)
    Start-Process cmd -ArgumentList "/c", "set GHCA_API_ALREADY_STARTED=1&& detect-and-run-mobile.bat" -WorkingDirectory $ToolsPath
}

# -- Summary
Write-Host ""
Write-Host "  ============================================================" -ForegroundColor DarkCyan
Write-Host "    Services are starting in separate windows." -ForegroundColor Green
Write-Host "  ============================================================" -ForegroundColor DarkCyan
Write-Host "    API      : https://localhost:7214/swagger"
if ($NoWeb) {
    Write-Host "    Web      : (skipped -NoWeb; run npm start in GHCAA.Web to start Angular)" -ForegroundColor DarkGray
} else {
    Write-Host "    Web      : http://localhost:4200"
}
if ($NoMobile) {
    Write-Host "    Mobile   : (skipped; run detect-and-run-mobile.bat manually)" -ForegroundColor DarkGray
} else {
    Write-Host "    Mobile   : Flutter (detect-and-run-mobile.bat; API not started twice)"
}
Write-Host ""
Write-Host "    Secrets  : GHCAA.API\.env or dotnet user-secrets (see GHCAA.Tools\README.md)" -ForegroundColor DarkGray
Write-Host "  ============================================================" -ForegroundColor DarkCyan
Write-Host ""

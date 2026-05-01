# GHCAA Unified Visual & Functional Test Runner
# Usage:
#   ./scripts/visual-check.ps1                    -> Run all tests (compare to baselines)
#   ./scripts/visual-check.ps1 -UpdateBaselines   -> Capture new baseline screenshots
#   ./scripts/visual-check.ps1 -VisualOnly        -> Skip E2E, run visual snapshot tests only
#   ./scripts/visual-check.ps1 -E2EOnly           -> Skip visual, run functional E2E tests only
#   ./scripts/visual-check.ps1 -Suite admin       -> Run only the admin visual suite

param (
    [switch]$UpdateBaselines = $false,
    [switch]$VisualOnly = $false,
    [switch]$E2EOnly = $false,
    [string]$Suite = ""
)

$ErrorActionPreference = "Stop"
$Root = Split-Path -Parent $PSScriptRoot
$WebDir = Join-Path $Root "GHCAA.Web"
$MobileDir = Join-Path $Root "GHCAA.Mobile"

Write-Host ""
Write-Host "====================================================" -ForegroundColor Cyan
Write-Host "   GHCAA Quality Gate - Visual & E2E Test Runner    " -ForegroundColor Cyan
Write-Host "====================================================" -ForegroundColor Cyan
Write-Host ""

# Auto-detect Flutter if not in PATH
if (!(Get-Command flutter -ErrorAction SilentlyContinue)) {
    $commonFlutterPaths = @("C:\src\flutter\bin", "C:\flutter\bin", "$env:LOCALAPPDATA\Flutter\bin")
    foreach ($path in $commonFlutterPaths) {
        if (Test-Path $path) {
            $env:Path = "$path;$env:Path"
            Write-Host "ENV: Flutter detected at $path and added to session PATH." -ForegroundColor DarkGray
            break
        }
    }
}

# Set environment for visual seed data and isolated SQLite database
$dbPath = Join-Path $Root "visual_test.db"
$env:ASP_SEED_PROFILE = "Visual"
$env:DatabaseProvider = "Sqlite"
$env:SqliteConnection = "Data Source=$dbPath"
$env:AppSettings__RecreateDatabaseOnStartup = "true"

# Clean up previous test database if it exists
if (Test-Path $dbPath) {
    Remove-Item $dbPath -Force
    Write-Host "CLEAN: Removed previous test database." -ForegroundColor DarkGray
}

Write-Host "CONFIG: ASP_SEED_PROFILE=Visual - using static seed data" -ForegroundColor DarkGray
Write-Host "CONFIG: DatabaseProvider=Sqlite - using temporary isolated DB at $dbPath" -ForegroundColor DarkGray
Write-Host ""

# Start the API server in the background
Write-Host ">> Starting API server (SQLite mode)..." -ForegroundColor Yellow
$ApiProj = Join-Path $Root "GHCAA.API/GHCAA.API.csproj"
$OutLog = Join-Path $Root "api_stdout.txt"
$ErrLog = Join-Path $Root "api_stderr.txt"
if (Test-Path $OutLog) { Remove-Item $OutLog }
if (Test-Path $ErrLog) { Remove-Item $ErrLog }
$ApiProcess = Start-Process dotnet -ArgumentList "run --project `"$ApiProj`" --no-build" -PassThru -WindowStyle Hidden -RedirectStandardOutput $OutLog -RedirectStandardError $ErrLog
$ApiId = $ApiProcess.Id

# Wait for API to be ready (health check)
$maxRetries = 30
$retryCount = 0
$apiReady = $false
while (-not $apiReady -and $retryCount -lt $maxRetries) {
    try {
        $response = Invoke-WebRequest -Uri "http://127.0.0.1:5087/health" -UseBasicParsing -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) { $apiReady = $true }
    } catch {
        # Wait and retry
    }
    if (-not $apiReady) {
        $retryCount++
        Start-Sleep -Seconds 2
        Write-Host "   Waiting for API... ($retryCount/$maxRetries)" -ForegroundColor DarkGray
    }
}

if (-not $apiReady) {
    Write-Host "   [FAIL] API failed to start in time." -ForegroundColor Red
    Stop-Process -Id $ApiId -Force
    exit 1
}
Write-Host "   [READY] API is up at http://localhost:5087" -ForegroundColor Green
Write-Host ""

# Initialize exit codes
$WebVisualExit = 0
$WebE2EExit = 0
$MobileVisualExit = 0
$MobileE2EExit = 0

# ──────────────────────────────────────────────
# WEB VISUAL TESTS (Playwright)
# ──────────────────────────────────────────────
if (-not $E2EOnly) {
    Write-Host ">> [1/4] Web Visual Snapshot Tests (Playwright)" -ForegroundColor Yellow

    $playwrightArgs = "tests/visual"
    if ($Suite -ne "") { $playwrightArgs = "tests/visual/$Suite.spec.ts" }
    if ($UpdateBaselines) { $playwrightArgs += " --update-snapshots" }

    Push-Location $WebDir
    cmd /c "npx playwright test $playwrightArgs --reporter=html"
    $WebVisualExit = $LASTEXITCODE
    Pop-Location

    if ($WebVisualExit -ne 0) {
        Write-Host "   [FAIL] Web visual tests FAILED (exit $WebVisualExit). Check playwright-report/" -ForegroundColor Red
    } else {
        Write-Host "   [PASS] Web visual tests passed." -ForegroundColor Green
    }
    Write-Host ""
}

# ──────────────────────────────────────────────
# WEB E2E TESTS (Playwright)
# ──────────────────────────────────────────────
if (-not $VisualOnly) {
    Write-Host ">> [2/4] Web Functional E2E Tests (Playwright)" -ForegroundColor Yellow

    Push-Location $WebDir
    cmd /c "npx playwright test tests/e2e --reporter=html"
    $WebE2EExit = $LASTEXITCODE
    Pop-Location

    if ($WebE2EExit -ne 0) {
        Write-Host "   [FAIL] Web E2E tests FAILED (exit $WebE2EExit). Check playwright-report/" -ForegroundColor Red
    } else {
        Write-Host "   [PASS] Web E2E tests passed." -ForegroundColor Green
    }
    Write-Host ""
}

# ──────────────────────────────────────────────
# MOBILE VISUAL TESTS (Flutter Goldens)
# ──────────────────────────────────────────────
if (-not $E2EOnly) {
    Write-Host ">> [3/4] Mobile Visual Snapshot Tests (Flutter Goldens)" -ForegroundColor Yellow

    $flutterTestArgs = "test/comprehensive_visual_freeze_test.dart"
    if ($UpdateBaselines) { $flutterTestArgs += " --update-goldens" }

    Push-Location $MobileDir
    $FlutterCmd = "flutter test $flutterTestArgs"
    Invoke-Expression $FlutterCmd
    $MobileVisualExit = $LASTEXITCODE
    Pop-Location

    if ($MobileVisualExit -ne 0) {
        Write-Host "   [FAIL] Mobile visual tests FAILED (exit $MobileVisualExit)." -ForegroundColor Red
    } else {
        Write-Host "   [PASS] Mobile visual tests passed." -ForegroundColor Green
    }
    Write-Host ""
}

# ──────────────────────────────────────────────
# MOBILE E2E TESTS (Flutter Integration)
# ──────────────────────────────────────────────
if (-not $VisualOnly) {
    Write-Host ">> [4/4] Mobile Functional E2E Tests (Flutter Integration)" -ForegroundColor Yellow

    Push-Location $MobileDir
    flutter test integration_test/ -d windows
    $MobileE2EExit = $LASTEXITCODE
    Pop-Location

    if ($MobileE2EExit -ne 0) {
        Write-Host "   [FAIL] Mobile E2E tests FAILED (exit $MobileE2EExit)." -ForegroundColor Red
    } else {
        Write-Host "   [PASS] Mobile E2E tests passed." -ForegroundColor Green
    }
    Write-Host ""
}

# ──────────────────────────────────────────────
# SUMMARY
# ──────────────────────────────────────────────
Write-Host "====================================================" -ForegroundColor Cyan
Write-Host "                    SUMMARY                         " -ForegroundColor Cyan
Write-Host "====================================================" -ForegroundColor Cyan

$allPassed = $true
if (-not $E2EOnly    -and $WebVisualExit    -ne 0) { Write-Host "  [FAIL] Web Visual     : FAILED" -ForegroundColor Red;   $allPassed = $false }
if (-not $VisualOnly -and $WebE2EExit       -ne 0) { Write-Host "  [FAIL] Web E2E        : FAILED" -ForegroundColor Red;   $allPassed = $false }
if (-not $E2EOnly    -and $MobileVisualExit -ne 0) { Write-Host "  [FAIL] Mobile Visual  : FAILED" -ForegroundColor Red;   $allPassed = $false }
if (-not $VisualOnly -and $MobileE2EExit    -ne 0) { Write-Host "  [FAIL] Mobile E2E     : FAILED" -ForegroundColor Red;   $allPassed = $false }

if ($allPassed) {
    Write-Host "  [SUCCESS] ALL TESTS PASSED - Platform is stable." -ForegroundColor Green
} else {
    Write-Host ""
    Write-Host "  TIP: To update baselines after an approved design change:" -ForegroundColor DarkGray
    Write-Host "       ./scripts/visual-check.ps1 -UpdateBaselines" -ForegroundColor DarkGray

    # Cleanup test database on exit
    if (Test-Path $dbPath) { Remove-Item $dbPath -Force }
    if ($ApiId) { Stop-Process -Id $ApiId -Force }
    exit 1
}

# Cleanup test database on success
if (Test-Path $dbPath) { Remove-Item $dbPath -Force }
if ($ApiId) { Stop-Process -Id $ApiId -Force }
Write-Host "CLEAN: Removed temporary test database and stopped API server." -ForegroundColor DarkGray

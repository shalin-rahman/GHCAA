# GHCAA Visual & E2E Test Suite Orchestrator
# This script prepares the test environment, runs all tests, and cleans up.

param(
    [switch]$KeepData = $false,
    [string]$Project = "GHCAA.Web"
)

$toolsDir = Split-Path $MyInvocation.MyCommand.Path
$root = Split-Path $toolsDir

# 1. Setup Test Data
Write-Host "--- Step 1: Setting up Test Data ---" -ForegroundColor Cyan
& "$toolsDir\setup-test-data.ps1"

# 2. Run API in background with Visual Seed Profile
Write-Host "--- Step 2: Starting API with 'Visual' profile ---" -ForegroundColor Cyan
$env:ASP_SEED_PROFILE = "Visual"
$env:AppSettings__RecreateDatabaseOnStartup = "true"
$env:ASPNETCORE_ENVIRONMENT = "Development"

# Start API in a new window or background
$apiProcess = Start-Process dotnet -ArgumentList "run --project $root\GHCAA.API\GHCAA.API.csproj" -PassThru -NoNewWindow
Write-Host "API started (PID: $($apiProcess.Id))" -ForegroundColor Green

# 3. Wait for API to be ready
Write-Host "Waiting for API to be healthy..." -ForegroundColor Yellow
$timeout = 60
$waited = 0
while ($waited -lt $timeout) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5087/health" -UseBasicParsing -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) { break }
    } catch { }
    Start-Sleep -Seconds 2
    $waited += 2
}

if ($waited -ge $timeout) {
    Write-Host "ERROR: API failed to start within timeout." -ForegroundColor Red
    Stop-Process -Id $apiProcess.Id -Force
    exit 1
}

# 4. Run Playwright Tests
Write-Host "--- Step 3: Running Playwright Tests ---" -ForegroundColor Cyan
Push-Location "$root\GHCAA.Web"
cmd.exe /c "npm run test:e2e"
$testResult = $LASTEXITCODE
Pop-Location

# 5. Cleanup
Write-Host "--- Step 4: Cleanup ---" -ForegroundColor Cyan
Stop-Process -Id $apiProcess.Id -Force
Write-Host "API stopped." -ForegroundColor Green

if (-not $KeepData) {
    & "$toolsDir\cleanup-test-data.ps1"
}

if ($testResult -eq 0) {
    Write-Host "TEST SUITE PASSED" -ForegroundColor Green
} else {
    Write-Host "TEST SUITE FAILED (Exit Code: $testResult)" -ForegroundColor Red
}

exit $testResult

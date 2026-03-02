$RootPath = Get-Location
param (
    [switch]$SkipTests
)

Write-Host "Starting GHCAA Platform..." -ForegroundColor Cyan

if (-not $SkipTests) {
    # Run tests
    Write-Host "Running Backend Unit Tests..." -ForegroundColor Yellow
    dotnet test GHCAA.Tests/GHCAA.Tests.csproj
    if ($LASTEXITCODE -ne 0) {
        Write-Host "!! Backend unit tests failed. Aborting startup !!" -ForegroundColor Red
        pause
        exit
    }
    Write-Host "OK - Backend tests passed." -ForegroundColor Green
    Write-Host ""

    # Run frontend tests
    Write-Host "Running Frontend Unit Tests..." -ForegroundColor Yellow
    pushd GHCAA.Web
    cmd /c npm run test -- --watch=false
    $feExitCode = $LASTEXITCODE
    popd
    if ($feExitCode -ne 0) {
        Write-Host "!! Frontend unit tests failed. Aborting startup !!" -ForegroundColor Red
        pause
        exit
    }
    Write-Host "OK - Frontend tests passed." -ForegroundColor Green
    Write-Host ""
} else {
    Write-Host "Skipping tests as requested." -ForegroundColor DarkGray
}

# Start API in a new window
Write-Host "Starting API..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd `"$RootPath`"; dotnet run --project GHCAA.API/GHCAA.API.csproj"

# Start Frontend in a new window
Write-Host "Starting Frontend..." -ForegroundColor Gold
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd `"$RootPath\GHCAA.Web`"; cmd /c npm start"

Write-Host "API and Frontend are starting in separate windows." -ForegroundColor Green
Write-Host "Default Admin: shalin / shalin"

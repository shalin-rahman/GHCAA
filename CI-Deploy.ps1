# GHCAA CI/CD Build Script

$ErrorActionPreference = "Stop"
$RootPath = Get-Location
$PublishPath = "$RootPath\publish"

Write-Host "Initialing build process..." -ForegroundColor Cyan

# 1. Clean Environment
Write-Host "Cleaning build directory..." -ForegroundColor Yellow
if (Test-Path $PublishPath) { Remove-Item -Recurse -Force $PublishPath }
New-Item -ItemType Directory -Path $PublishPath | Out-Null

# 1.5 Run Backend Tests
Write-Host "Running Backend Unit Tests..." -ForegroundColor Yellow
dotnet test GHCAA.Tests/GHCAA.Tests.csproj
if ($LASTEXITCODE -ne 0) {
    Write-Error "Backend build aborted: Unit tests failed."
}
Write-Host "OK - All backend tests passed." -ForegroundColor Green
Write-Host ""

# 2. Backend Build
Write-Host "Building API..." -ForegroundColor Yellow
Set-Location "$RootPath\GHCAA.API"
dotnet restore
dotnet publish -c Release -o "$PublishPath\api" --no-restore

# 2.5 Database Migrations
Write-Host "Applying database migrations..." -ForegroundColor Yellow
Set-Location "$RootPath\GHCAA.API"
dotnet ef database update --project ../GHCAA.Infrastructure/GHCAA.Infrastructure.csproj --startup-project GHCAA.API.csproj

# 3. Frontend Build
Write-Host "Building frontend..." -ForegroundColor Yellow
Set-Location "$RootPath\GHCAA.Web"
cmd /c npm install
cmd /c npm run build

# 4. Packaging
Write-Host "Packaging build..." -ForegroundColor Yellow
$FrontendDist = "$RootPath\GHCAA.Web\dist\GHCAA.Web"

if (Test-Path $FrontendDist) {
    Copy-Item -Path "$FrontendDist\*" -Destination "$PublishPath\api\wwwroot" -Recurse -Force
    Write-Host "Frontend files copied to API wwwroot." -ForegroundColor Green
} else {
    Write-Error "Build failed: Frontend dist folder not found."
}

# 5. Summary
Write-Host "Build completed successfully." -ForegroundColor Green
Write-Host "Output directory: $PublishPath\api"
Write-Host "To start: cd `"$PublishPath\api`" ; dotnet GHCAA.API.dll"

Set-Location $RootPath

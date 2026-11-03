# GHCAA CI/CD Build Script

$ErrorActionPreference = "Stop"
$ToolsPath = $PSScriptRoot
$RootPath = Split-Path $ToolsPath -Parent
$PublishPath = "$RootPath\publish"

# PowerShell 5.1 does not stop on a non-zero exit from dotnet or npm, so every
# native call goes through this check.
function Assert-LastExit([string]$Step) {
    if ($LASTEXITCODE -ne 0) {
        Set-Location $RootPath
        throw "$Step failed with exit code $LASTEXITCODE."
    }
}

Write-Host "Starting build process..." -ForegroundColor Cyan

# 1. Clean Environment
Write-Host "Cleaning build directory..." -ForegroundColor Yellow
if (Test-Path $PublishPath) { Remove-Item -Recurse -Force $PublishPath }
New-Item -ItemType Directory -Path $PublishPath | Out-Null

# 1.5 Run Backend Tests
Write-Host "Running Backend Unit Tests..." -ForegroundColor Yellow
dotnet test "$RootPath\GHCAA.Tests\GHCAA.Tests.csproj"
Assert-LastExit "Backend unit tests"
Write-Host "OK - All backend tests passed." -ForegroundColor Green
Write-Host ""

# 2. Backend Build
# No migration step here. MigrationBootstrapper applies migrations when the API
# starts (docs/adr/0002-migrations-apply-automatically-at-startup.md).
Write-Host "Building API..." -ForegroundColor Yellow
Set-Location "$RootPath\GHCAA.API"
dotnet restore
Assert-LastExit "dotnet restore"
dotnet publish -c Release -o "$PublishPath\api" --no-restore
Assert-LastExit "dotnet publish"

# 3. Frontend Build
Write-Host "Building frontend..." -ForegroundColor Yellow
Set-Location "$RootPath\GHCAA.Web"
# Remove the old build so a failed build cannot leave stale files to package.
$WebDist = "$RootPath\GHCAA.Web\dist"
if (Test-Path $WebDist) { Remove-Item -Recurse -Force $WebDist }
cmd /c npm ci
Assert-LastExit "npm ci"
cmd /c npm run build
Assert-LastExit "npm run build"

# 4. Packaging
Write-Host "Packaging build..." -ForegroundColor Yellow
$FrontendDist = "$WebDist\GHCAA.Web\browser"

if (Test-Path $FrontendDist) {
    Copy-Item -Path "$FrontendDist\*" -Destination "$PublishPath\api\wwwroot" -Recurse -Force
    Write-Host "Frontend files copied to API wwwroot." -ForegroundColor Green
} else {
    Set-Location $RootPath
    throw "Build failed: Frontend dist folder not found."
}

# 5. Summary
Write-Host "Build completed successfully." -ForegroundColor Green
Write-Host "Output directory: $PublishPath\api"
Write-Host "To start: cd `"$PublishPath\api`" ; dotnet GHCAA.API.dll"

Set-Location $RootPath

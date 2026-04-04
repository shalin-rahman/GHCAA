# Creates GHCAA.API/.env from .env.example if .env does not exist.
# Edit .env with real secrets; the file is gitignored.

$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$apiDir = Join-Path $repoRoot "GHCAA.API"
$example = Join-Path $apiDir ".env.example"
$target = Join-Path $apiDir ".env"

if (-not (Test-Path $example)) {
    Write-Error "Missing $example"
    exit 1
}

if (Test-Path $target) {
    Write-Host "Already exists: $target - not overwriting. Edit it or delete it to recreate from the example."
    exit 0
}

Copy-Item -Path $example -Destination $target
Write-Host "Created $target"
Write-Host "1. Edit that file and set Jwt__Key (at least 32 chars), database, Gmail, etc."
Write-Host '2. Run the API from GHCAA.API (dotnet run) so DotNetEnv loads .env, or set variables in User Secrets or your IDE.'

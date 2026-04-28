# GHCAA Test Data Cleanup Script
# This script removes the 'Visual' seed directory to revert to standard seed data.

$visualDir = "GHCAA.Infrastructure\Data\Seed\Visual"

if (Test-Path $visualDir) {
    Remove-Item -Recurse -Force $visualDir
    Write-Host "Reverted to standard seed data (Visual directory removed)." -ForegroundColor Green
} else {
    Write-Host "Visual directory not found. Already clean." -ForegroundColor Yellow
}

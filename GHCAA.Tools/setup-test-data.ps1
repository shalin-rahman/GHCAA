# GHCAA Test Data Setup Script
# This script merges custom test data into the 'Visual' seed directory for automated testing.

$seedDir = "GHCAA.Infrastructure\Data\Seed"
$visualDir = "$seedDir\Visual"
$testDataDir = "GHCAA.Tools\TestData"

if (!(Test-Path $visualDir)) {
    New-Item -ItemType Directory -Path $visualDir | Out-Null
    Write-Host "Created Visual seed directory." -ForegroundColor Green
}

$filesToMerge = @("members.json", "users.json", "user_roles.json", "academic_records.json", "professional_records.json", "payment_histories.json")

foreach ($file in $filesToMerge) {
    $basePath = Join-Path $seedDir $file
    $testPath = Join-Path $testDataDir ("test-" + $file)
    $targetPath = Join-Path $visualDir $file

    if (Test-Path $testPath) {
        Write-Host "Processing $file..." -ForegroundColor Cyan
        
        $testData = Get-Content $testPath | ConvertFrom-Json
        
        if (Test-Path $basePath) {
            $baseData = Get-Content $basePath | ConvertFrom-Json
            # Merge arrays
            $mergedData = $baseData + $testData
            $mergedData | ConvertTo-Json -Depth 100 | Set-Content $targetPath
            Write-Host "Merged base and test data for $file." -ForegroundColor Green
        } else {
            $testData | ConvertTo-Json -Depth 100 | Set-Content $targetPath
            Write-Host "Copied test data for $file (no base found)." -ForegroundColor Yellow
        }
    }
}

Write-Host "Test data setup complete. Run with ASP_SEED_PROFILE=Visual to use this data." -ForegroundColor Green

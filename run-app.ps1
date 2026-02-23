$RootPath = Get-Location

Write-Host "Starting GHCAA Platform..." -ForegroundColor Cyan

# Start API in a new window
Write-Host "Starting API..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd `"$RootPath`"; dotnet run --project GHCAA.API/GHCAA.API.csproj"

# Start Frontend in a new window
Write-Host "Starting Frontend..." -ForegroundColor Gold
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd `"$RootPath\GHCAA.Web`"; cmd /c npm start"

Write-Host "API and Frontend are starting in separate windows." -ForegroundColor Green
Write-Host "Default Admin: shalin / shalin"

$RootPath = Get-Location

Write-Host "🚀 Starting GHCAA Platform..." -ForegroundColor Cyan

# Start API in a new window
Write-Host "📡 Launching Backend API (PostgreSQL)..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd `"$RootPath`"; dotnet run --project GHCAA.API/GHCAA.API.csproj"

# Start Frontend in a new window
Write-Host "🎨 Launching Frontend Web..." -ForegroundColor Gold
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd `"$RootPath\GHCAA.Web`"; cmd /c npm start"

Write-Host "✅ Both services are launching in separate windows." -ForegroundColor Green
Write-Host "Admin Login: shalin / shalin" -ForegroundColor White

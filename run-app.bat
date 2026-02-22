@echo off
echo Starting GHCAA Platform...

echo 📡 Launching Backend API (PostgreSQL)...
start powershell -NoExit -Command "dotnet run --project GHCAA.API/GHCAA.API.csproj --launch-profile https"

echo Launching Frontend Web...
start powershell -NoExit -Command "cd GHCAA.Web; cmd /c npm start"

echo Both services are launching in separate windows.
echo Admin Login: shalin / shalin
pause

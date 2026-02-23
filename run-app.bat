@echo off
echo ============================================
echo   GHCAA Platform Launcher
echo ============================================
echo.

:: Kill existing processes to avoid port conflicts
echo Closing any running instances...

:: Kill dotnet processes (API)
taskkill /F /IM dotnet.exe >nul 2>&1
if %errorlevel%==0 (echo   [OK] Stopped existing .NET processes) else (echo   [--] No .NET processes found)

:: Kill node processes (Angular dev server)
taskkill /F /IM node.exe >nul 2>&1
if %errorlevel%==0 (echo   [OK] Stopped existing Node processes) else (echo   [--] No Node processes found)

timeout /t 2 /nobreak >nul
echo.

:: Apply pending EF Core migrations
echo Applying database migrations...
dotnet ef database update --project GHCAA.Infrastructure --startup-project GHCAA.API
if %errorlevel%==0 (
    echo   [OK] Database is up to date
) else (
    echo   [!!] Migration failed - check connection string
    echo   Continuing to launch anyway...
)
echo.

echo Launching Backend API...
start powershell -NoExit -Command "dotnet run --project GHCAA.API/GHCAA.API.csproj --launch-profile https"

echo Launching Frontend Web...
start powershell -NoExit -Command "cd GHCAA.Web; cmd /c npm start"

echo.
echo ============================================
echo   Both services are launching.
echo   Admin Login: shalin / shalin
echo ============================================
pause

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

set SKIP_TESTS=0
if "%1"=="/notest" set SKIP_TESTS=1

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

if %SKIP_TESTS%==0 (
    echo Running Backend Unit Tests...
    dotnet test GHCAA.Tests/GHCAA.Tests.csproj --logger "console;verbosity=normal"
    if %errorlevel%==0 (
        echo   [OK] All backend tests passed
    ) else (
        echo   [!!] Some backend tests failed! 
        echo   Please review the failures before continuing.
        pause
    )
    echo.

    :: Run frontend unit tests
    echo Running Frontend Unit Tests...
    cd GHCAA.Web
    cmd /c npm run test -- --watch=false
    if %errorlevel%==0 (
        echo   [OK] All frontend tests passed
    ) else (
        echo   [!!] Some frontend tests failed! 
        echo   Please review the failures before continuing.
        pause
    )
    cd ..
    echo.
) else (
    echo Skipping tests as requested.
)

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

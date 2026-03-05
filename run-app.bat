@echo off
setlocal enabledelayedexpansion
echo.
echo  ============================================================
echo    GHCAA Platform  ^|  Launcher
echo  ============================================================
echo.

:: ── Parse Arguments ──────────────────────────────────────────
set SKIP_TESTS=0
set DB_PROVIDER=
for %%A in (%*) do (
    if /i "%%A"=="/notest"   set SKIP_TESTS=1
    if /i "%%A"=="/mysql"    set DB_PROVIDER=MySql
    if /i "%%A"=="/pgsql"    set DB_PROVIDER=PgSql
    if /i "%%A"=="/sqlite"   set DB_PROVIDER=Sqlite
)

:: If a provider was passed, override appsettings for this session
if not "%DB_PROVIDER%"=="" (
    echo   [DB] Using database provider: %DB_PROVIDER%
    set GHCAA__DatabaseProvider=%DB_PROVIDER%
)

:: ── Stop existing processes ───────────────────────────────────
echo   Stopping any existing GHCAA processes...
taskkill /F /FI "IMAGENAME eq dotnet.exe" /FI "WINDOWTITLE eq GHCAA*" >nul 2>&1
taskkill /F /IM dotnet.exe >nul 2>&1
if %errorlevel%==0 (echo   [OK] Stopped .NET processes) else (echo   [--] No .NET processes found)
taskkill /F /IM node.exe >nul 2>&1
if %errorlevel%==0 (echo   [OK] Stopped Node processes) else (echo   [--] No Node processes found)
timeout /t 2 /nobreak >nul
echo.

:: ── Database Migration ────────────────────────────────────────
echo   Applying database migrations...
dotnet ef database update --project GHCAA.Infrastructure --startup-project GHCAA.API
if %errorlevel%==0 (
    echo   [OK] Database is up to date
) else (
    echo   [!!] Migration failed - check your connection string in appsettings.json
    echo   [!!] Set DatabaseProvider to PgSql, MySql, or Sqlite
    echo        Continuing to launch anyway...
)
echo.

:: ── Tests ─────────────────────────────────────────────────────
if %SKIP_TESTS%==0 (
    echo   Running Backend Unit Tests...
    dotnet test GHCAA.Tests/GHCAA.Tests.csproj --logger "console;verbosity=normal"
    if %errorlevel%==0 (
        echo   [OK] All backend tests passed
    ) else (
        echo   [!!] Some backend tests FAILED. Review before continuing.
        pause
    )
    echo.

    echo   Running Frontend Unit Tests...
    cd GHCAA.Web
    cmd /c npm run test -- --watch=false
    if %errorlevel%==0 (
        echo   [OK] All frontend tests passed
    ) else (
        echo   [!!] Some frontend tests FAILED. Review before continuing.
        pause
    )
    cd ..
    echo.
) else (
    echo   [--] Skipping tests ^(/notest^)
    echo.
)

:: ── Launch Services ───────────────────────────────────────────
echo   Launching Backend API  ^(https://localhost:7084^)...
start "GHCAA - API" powershell -NoExit -Command "Write-Host 'GHCAA API' -ForegroundColor Cyan; dotnet run --project GHCAA.API/GHCAA.API.csproj --launch-profile https"

timeout /t 2 /nobreak >nul

echo   Launching Frontend    ^(http://localhost:4200^)...
start "GHCAA - Web" powershell -NoExit -Command "Write-Host 'GHCAA Frontend' -ForegroundColor Cyan; Set-Location GHCAA.Web; cmd /c npm start"

echo.
echo  ============================================================
echo    Services are starting in separate windows.
echo  ============================================================
echo    API      https://localhost:7084
echo    Swagger  https://localhost:7084/swagger
echo    Frontend http://localhost:4200
echo  ------------------------------------------------------------
echo    Admin Login :  shalin  /  shalin
echo  ------------------------------------------------------------
echo    Arguments:
echo      /notest   - skip unit tests
echo      /mysql    - use MySQL database
echo      /pgsql    - use PostgreSQL database  (default)
echo      /sqlite   - use SQLite database
echo  ============================================================
echo.
pause

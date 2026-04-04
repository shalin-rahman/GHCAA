@echo off
echo.
echo  ============================================================
echo    GHCAA Platform  ^|  Stop All Services
echo  ============================================================
echo.
echo   Stopping GHCAA services (API, Web, Mobile dev)...
echo   See GHCAA.Tools\README.md for PowerShell options (-IncludeBrowsers, -PortsOnly).
echo.

:: ── Stop .NET API ─────────────────────────────────────────────
echo   Stopping .NET processes (GHCAA API)...
taskkill /F /IM dotnet.exe >nul 2>&1
if %errorlevel%==0 (
    echo   [OK] .NET processes stopped
) else (
    echo   [--] No .NET processes were running
)

:: ── Stop Node / Angular Frontend ──────────────────────────────
echo   Stopping Node processes (Angular frontend)...
taskkill /F /IM node.exe >nul 2>&1
if %errorlevel%==0 (
    echo   [OK] Node processes stopped
) else (
    echo   [--] No Node processes were running
)

:: ── Stop Flutter / Dart (Mobile) ──────────────────────────────
echo   Stopping Flutter and Dart processes...
taskkill /F /IM flutter.exe >nul 2>&1
taskkill /F /IM dart.exe >nul 2>&1
if %errorlevel%==0 (
    echo   [OK] Flutter/Dart processes stopped
) else (
    echo   [--] No mobile processes were running
)

:: ── Release locked ports 7214 / 5087 / 4200 ───────────────────
echo   Releasing ports 7214, 5087 and 4200...
for /f "tokens=5" %%P in ('netstat -ano ^| findstr ":7214 " 2^>nul') do (
    taskkill /F /PID %%P >nul 2>&1
)
for /f "tokens=5" %%P in ('netstat -ano ^| findstr ":5087 " 2^>nul') do (
    taskkill /F /PID %%P >nul 2>&1
)
for /f "tokens=5" %%P in ('netstat -ano ^| findstr ":4200 " 2^>nul') do (
    taskkill /F /PID %%P >nul 2>&1
)
echo   [OK] Ports cleared

echo.
echo  ============================================================
echo    All GHCAA services stopped.
echo  ============================================================
echo    Restart: GHCAA.Tools\run-app.bat
echo.
pause

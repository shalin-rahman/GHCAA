@echo off
setlocal enableextensions
cd /d "%~dp0"

echo [GHCAA] Initializing GHCAA Portal Launcher...

:: System Check
where flutter >nul 2>nul
if %errorlevel% neq 0 (
    echo [ERROR] Flutter SDK not found in PATH! 
    echo Please install Flutter or add it to your System Environment Variables.
    pause
    exit /b
)

echo [GHCAA] Sanitizing development environment...
taskkill /F /IM flutter.exe 2>nul
taskkill /F /IM dart.exe 2>nul

echo [GHCAA] Starting Global API Engine (ASP.NET Core)...
start "GHCAA API Engine" dotnet run --project GHCAA.API --launch-profile https

echo [GHCAA] Waiting for API to stabilize...
timeout /t 5 /nobreak >nul

echo [GHCAA] Synchronizing Mobile Portal Dependencies...
cd GHCAA.Mobile
call flutter pub get

echo [GHCAA] Ready for Deployment.
echo [GHCAA] Note: For Android emulators, ensure .env is BASE_API_URL=https://10.0.2.2:7214
echo [GHCAA] For physical devices, use your local network IP.

call flutter run
pause

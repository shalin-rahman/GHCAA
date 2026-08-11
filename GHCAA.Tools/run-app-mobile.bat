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

if defined GHCA_API_ALREADY_STARTED (
    echo [GHCAA] Full-stack mode: API already running from run-app.ps1 — skipping second dotnet run.
    echo [GHCAA] Stopping prior Flutter/Dart only ^(not Chrome/Edge; Angular may be open^)...
    taskkill /F /IM flutter.exe 2>nul
    taskkill /F /IM dart.exe 2>nul
    echo [GHCAA] Brief wait for API warmup...
    timeout /t 2 /nobreak >nul
    goto :sync_mobile
)

echo [GHCAA] Sanitizing development environment...
taskkill /F /IM flutter.exe 2>nul
taskkill /F /IM dart.exe 2>nul
taskkill /F /IM msedge.exe 2>nul
taskkill /F /IM chrome.exe 2>nul

echo [GHCAA] Starting Global API Engine (ASP.NET Core)...
start "GHCAA API Engine" dotnet run --project ../GHCAA.API --launch-profile https

echo [GHCAA] Waiting for API to stabilize...
timeout /t 5 /nobreak >nul

:sync_mobile
echo [GHCAA] Synchronizing Mobile Portal Dependencies...
cd ../GHCAA.Mobile
call flutter pub get

echo [GHCAA] Ready for Deployment.
echo [GHCAA] Fixed Port Allocation: 50071
echo [GHCAA] Note: For Android emulators, ensure .env is BASE_API_URL=https://10.0.2.2:7214

echo [GHCAA] Searching for available target platforms (Chrome, Edge)...
flutter devices | findstr /i "chrome" >nul
if %errorlevel% == 0 (
    echo [GHCAA] Deploying to Google Chrome on Port 50071...
    call flutter run -d chrome --web-port 50071
) else (
    flutter devices | findstr /i "edge" >nul
    if %errorlevel% == 0 (
        echo [GHCAA] Chrome not detected. Falling back to Microsoft Edge on Port 50071...
        call flutter run -d edge --web-port 50071
    ) else (
        echo [GHCAA] No specific web target found. Launching default portal...
        call flutter run
    )
)
pause

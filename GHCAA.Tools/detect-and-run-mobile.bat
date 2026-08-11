@echo off
setlocal
:: Check standard install locations first
if exist "C:\src\flutter\bin\flutter.bat" set "FLUTTER_BIN_DIR=C:\src\flutter\bin" & goto :found
if exist "C:\flutter\bin\flutter.bat" set "FLUTTER_BIN_DIR=C:\flutter\bin" & goto :found
if exist "%USERPROFILE%\flutter\bin\flutter.bat" set "FLUTTER_BIN_DIR=%USERPROFILE%\flutter\bin" & goto :found

:: Full drive search as fallback (expensive)
echo [GHCAA] Performing deep-scan on C:\ drive... this might take a minute...
for /f "delims=" %%i in ('dir /s /b "C:\flutter.bat" 2^>nul') do (
    set "FLUTTER_BIN_DIR=%%~dpi"
    goto :found
)

echo [ERROR] Could not find 'flutter' in the most common folders.
echo [HELP] Please ensure you have downloaded the Flutter SDK from flutter.dev.
pause
exit /b

:found
echo [SUCCESS] Found Flutter SDK at: %FLUTTER_BIN_DIR%
echo [GHCAA] Automatically updating PATH for this session...
set "PATH=%FLUTTER_BIN_DIR%;%PATH%"

echo [GHCAA] Launching Portal Engine...
cd /d "%~dp0"
call run-app-mobile.bat
pause

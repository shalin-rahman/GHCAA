@echo off
echo ====================================================
echo   Running GHCAA Integration (E2E) Tests
echo ====================================================
powershell -ExecutionPolicy Bypass -File "%~dp0scripts\visual-check.ps1" -E2EOnly
pause

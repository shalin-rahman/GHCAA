@echo off
echo ====================================================
echo   Running GHCAA Visual Snapshot Tests
echo ====================================================
powershell -ExecutionPolicy Bypass -File "%~dp0scripts\visual-check.ps1" -VisualOnly
pause

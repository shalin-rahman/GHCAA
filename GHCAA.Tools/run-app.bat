@echo off
echo Running run-app.ps1...
powershell.exe -ExecutionPolicy Bypass -File "%~dp0run-app.ps1" %*
pause

@echo off
rem The registration and approval walkthrough. Same options as run-workflow-walkthrough.bat:
rem   run-membership-walkthrough.bat [keep] [headed] [slow]
rem With no arguments, as when double-clicked, it runs headed and waits for a key at the end.
setlocal
if "%~1"=="" (
  call "%~dp0run-workflow-walkthrough.bat" membership headed
  pause
  exit /b
)
call "%~dp0run-workflow-walkthrough.bat" membership %*
exit /b %ERRORLEVEL%

@echo off
rem The event participation and approval walkthrough. Same options as run-workflow-walkthrough.bat:
rem   run-event-walkthrough.bat [keep] [headed] [slow]
rem With no arguments, as when double-clicked, it runs headed and waits for a key at the end.
setlocal
if "%~1"=="" (
  call "%~dp0run-workflow-walkthrough.bat" event headed
  pause
  exit /b
)
call "%~dp0run-workflow-walkthrough.bat" event %*
exit /b %ERRORLEVEL%

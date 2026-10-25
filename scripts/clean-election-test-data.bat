@echo off
rem 84.52. Deletes every election the local test runs made, with their EC periods, votes,
rem notifications, emails and codes, and puts the borrowed accounts back.
rem The walkthrough leaves its election in place after a run, so this is how it goes.
rem Rows are found by the IDs the runs recorded and by the script-only titles, so real
rem elections and periods stay. The report in GHCAA.Web\walkthrough-report is not touched.
rem
rem   clean-election-test-data.bat

setlocal EnableExtensions
set "ROOT=%~dp0.."
set "WEB=%ROOT%\GHCAA.Web"
set "STATE_DIR=%TEMP%\ghcaa-local-election"

where node >nul 2>&1 || (echo Node.js was not found. & exit /b 1)
python -c "import psycopg2" >nul 2>&1 || (
  echo Python with psycopg2 is needed. Run: python -m pip install psycopg2-binary
  exit /b 1
)

echo Deleting the local election test data...
pushd "%WEB%"
call npm run test:e2e:walkthrough:clean
set "RESULT=%ERRORLEVEL%"
popd

if exist "%STATE_DIR%\walkthrough-checkpoint.json" del /q "%STATE_DIR%\walkthrough-checkpoint.json"
if not "%RESULT%"=="0" echo The clean-up failed. The message above names the step.
if "%RESULT%"=="0" echo Done. The local database has no election test data left.
exit /b %RESULT%

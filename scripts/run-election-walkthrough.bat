@echo off
rem 84.52. Runs the local election walkthrough and opens its report.
rem Starts the API and the web dev server in their own windows when they are not up.
rem
rem   run-election-walkthrough.bat [mode] [keep] [headed] [slow] [demo]
rem
rem   setup    (default) sets up a new election up to the campaign phase and saves a checkpoint
rem   resume   carries on from the checkpoint: polling, votes, close, count, results, recounts
rem   full     everything in one go, setup included
rem   clean    deletes all election test data and restores the borrowed accounts,
rem            the same as clean-election-test-data.bat
rem   restore  only restores the borrowed accounts
rem
rem Every mode first stops a walkthrough run left over from before and checks the tools it
rem needs, installing psycopg2, the npm packages and Playwright's Chromium when missing.
rem setup and full then run clean, so each starts from no election test data. resume does
rem not, because it carries on with the election setup made.
rem
rem The election stays in the database after a run, until the next setup, full or clean.
rem
rem   keep     also leave the borrowed accounts on the test password after the run
rem   headed   show the browser windows
rem   slow     headed, with 400 ms between actions, for a live demo
rem   demo     headed, and moves the polling dates to now in the local database instead
rem            of waiting for them. "full demo" shows the whole election.
rem
rem Input data is in GHCAA.Web\tests\e2e\election-walkthrough\scenario.json. Set
rem WALKTHROUGH_SCENARIO to a copy of it to run with other names, seats or votes.

setlocal EnableExtensions
set "ROOT=%~dp0.."
set "WEB=%ROOT%\GHCAA.Web"
set "API_HEALTH=http://localhost:5087/health/live"
set "WEB_URL=http://localhost:4200"
set "CHECKPOINT=%TEMP%\ghcaa-local-election\walkthrough-checkpoint.json"

rem Values left in the shell from an earlier run would change what this run does.
set "WALKTHROUGH_STAGE="
set "WALKTHROUGH_FRESH="
set "WALKTHROUGH_KEEP="
set "WALKTHROUGH_HEADED="
set "WALKTHROUGH_SLOWMO="
set "WALKTHROUGH_DEMO="

set "MODE=setup"
for %%A in (%*) do (
  if /i "%%A"=="setup" set "MODE=setup"
  if /i "%%A"=="resume" set "MODE=resume"
  if /i "%%A"=="full" set "MODE=full"
  if /i "%%A"=="clean" set "MODE=clean"
  if /i "%%A"=="restore" set "MODE=restore"
  if /i "%%A"=="keep" set "WALKTHROUGH_KEEP=1"
  if /i "%%A"=="headed" set "WALKTHROUGH_HEADED=1"
  if /i "%%A"=="demo" (
    set "WALKTHROUGH_HEADED=1"
    set "WALKTHROUGH_DEMO=1"
  )
  if /i "%%A"=="slow" (
    set "WALKTHROUGH_HEADED=1"
    set "WALKTHROUGH_SLOWMO=400"
  )
)

call :stop_earlier
call :deps || exit /b 1

if /i "%MODE%"=="clean" goto clean
if /i "%MODE%"=="restore" goto restore

if /i "%MODE%"=="resume" if not exist "%CHECKPOINT%" (
  echo No setup checkpoint was found. Run "%~nx0 setup" first.
  exit /b 1
)

if /i not "%MODE%"=="resume" (
  echo Deleting the election test data from earlier runs...
  call :clean_data || (echo The clean step failed. Its output above names the cause. & exit /b 1)
)

call :ensure "%API_HEALTH%" "GHCAA API" "%ROOT%\GHCAA.API" "dotnet run --launch-profile https" 180 || exit /b 1
call :ensure "%WEB_URL%" "GHCAA Web" "%WEB%" "npm run start" 240 || exit /b 1

rem The web proxy can keep failing after the API was restarted under it.
for /f %%S in ('curl -s -o nul -w "%%{http_code}" -m 15 "%WEB_URL%/api/config"') do set "PROXY=%%S"
if not "%PROXY%"=="200" (
  echo The web server cannot reach the API: GET /api/config through %WEB_URL% gave %PROXY%.
  echo Close the "GHCAA Web" window or stop npm run start, then run this again.
  exit /b 1
)

if /i "%MODE%"=="setup" (
  set "WALKTHROUGH_STAGE=setup"
  set "WALKTHROUGH_FRESH=1"
)
if /i "%MODE%"=="full" set "WALKTHROUGH_FRESH=1"

echo.
echo Running the walkthrough: %MODE%. The setup stage takes about 3 minutes and the
echo resume stage waits for the polling window in scenario.json, or about 2 minutes with demo.
echo If it stops on an email error, add the IP it names at
echo https://app.brevo.com/security/authorised_ips and run again.
echo.
pushd "%WEB%"
call npm run test:e2e:walkthrough
set "RESULT=%ERRORLEVEL%"
popd

if exist "%WEB%\walkthrough-report\index.html" start "" "%WEB%\walkthrough-report\index.html"
if not "%RESULT%"=="0" echo The run failed. The report shows the step it stopped at.
if /i "%MODE%"=="setup" if "%RESULT%"=="0" echo Setup is done. Run "%~nx0 resume" to carry on.
exit /b %RESULT%

:clean
call :clean_data
exit /b %ERRORLEVEL%

:clean_data
pushd "%WEB%"
call npm run test:e2e:walkthrough:clean
set "RESULT=%ERRORLEVEL%"
popd
if exist "%CHECKPOINT%" del /q "%CHECKPOINT%"
exit /b %RESULT%

rem An earlier run that was closed or failed can leave its Playwright runner and browser
rem windows open, still logged in and holding the election. taskkill /T takes the browsers too.
rem Only the runner's own two commands match, so an editor or other tool opened on the
rem walkthrough folder is left alone.
:stop_earlier
for /f %%P in ('powershell -NoProfile -Command "Get-CimInstance Win32_Process | Where-Object { $_.Name -eq 'node.exe' -and ($_.CommandLine -like '*playwright.walkthrough.config*' -or $_.CommandLine -like '*election-walkthrough*tasks.mjs*') } | ForEach-Object { $_.ProcessId }"') do (
  echo Stopping an earlier walkthrough run, process %%P.
  taskkill /PID %%P /T /F >nul 2>&1
)
exit /b 0

:deps
for %%T in (curl dotnet node npm python) do (
  where %%T >nul 2>&1 || (echo %%T was not found on PATH. Install it and run this again. & exit /b 1)
)
python -c "import psycopg2" >nul 2>&1 || (
  echo Installing psycopg2-binary for the database scripts...
  python -m pip install psycopg2-binary || exit /b 1
)
if not exist "%WEB%\node_modules\@playwright\test" (
  echo Installing the web packages...
  pushd "%WEB%"
  call npm ci || (popd & exit /b 1)
  popd
)
rem clean and restore open no browser. Quick when the browser is already there.
if /i "%MODE%"=="clean" exit /b 0
if /i "%MODE%"=="restore" exit /b 0
pushd "%WEB%"
call npx playwright install chromium || (popd & exit /b 1)
popd
exit /b 0

:restore
pushd "%WEB%"
call npm run test:e2e:walkthrough:restore
set "RESULT=%ERRORLEVEL%"
popd
exit /b %RESULT%

rem :ensure url title folder command seconds
rem Starts the command in a new window when the url does not answer, then waits for it.
:ensure
curl -s -f -o nul -m 5 "%~1" && exit /b 0
echo Starting %~2 in a new window...
start "%~2" /d "%~3" cmd /k "%~4"
set /a "LEFT=%~5"
:ensure_wait
timeout /t 5 /nobreak >nul
curl -s -f -o nul -m 5 "%~1" && (echo %~2 is up. & exit /b 0)
set /a "LEFT-=5"
if %LEFT% gtr 0 goto ensure_wait
echo %~2 did not answer at %~1. Check its window for the error.
exit /b 1

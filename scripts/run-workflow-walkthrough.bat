@echo off
rem Runs the membership or the event walkthrough on the local stack and opens its report.
rem Starts the API and the web dev server in their own windows when they are not up.
rem
rem   run-workflow-walkthrough.bat membership|event [keep] [headed] [slow]
rem   run-workflow-walkthrough.bat restore
rem
rem   membership  two applications (web and API), email codes, the fee marked received,
rem               one approved and one rejected, the new member's first login
rem   event       a paid event with a seat limit and a waitlist, member and guest
rem               registrations, approve and reject, the pass, check-in by ticket code
rem   restore     puts back the accounts the two walkthroughs borrowed
rem
rem   keep     leave the borrowed accounts on the test password after the run
rem   headed   show the browser windows
rem   slow     headed, with 400 ms between actions, for a live demo
rem
rem Each run first removes what its earlier runs made: archived applicants for membership,
rem deleted events for event. Input data is in the scenario.json beside each test.
rem The election walkthrough has its own script, run-election-walkthrough.bat.

setlocal EnableExtensions
set "ROOT=%~dp0.."
set "WEB=%ROOT%\GHCAA.Web"
set "API_HEALTH=http://localhost:5087/health/live"
set "WEB_URL=http://localhost:4200"
set "WALKTHROUGH_KEEP="
set "WALKTHROUGH_HEADED="
set "WALKTHROUGH_SLOWMO="

set "FLOW="
for %%A in (%*) do (
  if /i "%%A"=="membership" set "FLOW=membership"
  if /i "%%A"=="event" set "FLOW=event"
  if /i "%%A"=="restore" set "FLOW=restore"
  if /i "%%A"=="keep" set "WALKTHROUGH_KEEP=1"
  if /i "%%A"=="headed" set "WALKTHROUGH_HEADED=1"
  if /i "%%A"=="slow" (
    set "WALKTHROUGH_HEADED=1"
    set "WALKTHROUGH_SLOWMO=400"
  )
)
if "%FLOW%"=="" (
  echo Say which walkthrough: %~nx0 membership^|event [keep] [headed] [slow], or %~nx0 restore.
  exit /b 2
)

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

if /i "%FLOW%"=="restore" goto restore

pushd "%WEB%"
call npx playwright install chromium || (popd & exit /b 1)
popd

call :ensure "%API_HEALTH%" "GHCAA API" "%ROOT%\GHCAA.API" "dotnet run --launch-profile https" 180 || exit /b 1
call :ensure "%WEB_URL%" "GHCAA Web" "%WEB%" "npm run start" 240 || exit /b 1

for /f %%S in ('curl -s -o nul -w "%%{http_code}" -m 15 "%WEB_URL%/api/config"') do set "PROXY=%%S"
if not "%PROXY%"=="200" (
  echo The web server cannot reach the API: GET /api/config through %WEB_URL% gave %PROXY%.
  echo Close the "GHCAA Web" window or stop npm run start, then run this again.
  exit /b 1
)

echo.
echo Running the %FLOW% walkthrough. It takes a few minutes.
echo If it stops on an email error, add the IP it names at
echo https://app.brevo.com/security/authorised_ips and run again.
echo.
pushd "%WEB%"
call npm run test:e2e:%FLOW%
set "RESULT=%ERRORLEVEL%"
popd

if exist "%WEB%\walkthrough-report-%FLOW%\index.html" start "" "%WEB%\walkthrough-report-%FLOW%\index.html"
if not "%RESULT%"=="0" echo The run failed. The report shows the step it stopped at.
exit /b %RESULT%

:restore
pushd "%WEB%"
call npm run test:e2e:workflow:restore
set "RESULT=%ERRORLEVEL%"
popd
exit /b %RESULT%

rem :ensure url title folder command seconds
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

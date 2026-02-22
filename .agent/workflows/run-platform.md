# Run Platform Workflow

1. Start API (PostgreSQL)
// turbo
run_command:
  CommandLine: "dotnet run --project GHCAA.API/GHCAA.API.csproj"
  Cwd: "c:/Users/HabiburRahmanShalin/workstation/shaleen/shalin/GHC/Application/GHCAA/"
  SafeToAutoRun: true

2. Start Web Frontend (Angular)
// turbo
run_command:
  CommandLine: "npm start"
  Cwd: "c:/Users/HabiburRahmanShalin/workstation/shaleen/shalin/GHC/Application/GHCAA/GHCAA.Web/"
  SafeToAutoRun: true

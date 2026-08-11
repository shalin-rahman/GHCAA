# GHCAA.Tools

PowerShell helpers for local development. Run from any directory; scripts resolve the repo root from `$PSScriptRoot`.

## Scripts

### `run-app.ps1`

Starts the stack in **separate windows**:

1. Runs `stop-app.ps1 -Force` to clear prior processes.
2. Runs `dotnet ef database update` (provider from `appsettings.json` or `-DatabaseProvider`).
3. Optionally runs unit tests (`-RunTests`).
4. Starts **API** with working directory **`GHCAA.API`** so **DotNetEnv** can load **`GHCAA.API/.env`**.
5. Starts **Angular** (`npm start` in `GHCAA.Web`).
6. Runs **`detect-and-run-mobile.bat`** if present (Flutter).

**Usage**

```powershell
.\GHCAA.Tools\run-app.ps1
.\GHCAA.Tools\run-app.ps1 -DatabaseProvider Sqlite
.\GHCAA.Tools\run-app.ps1 -RunTests
.\GHCAA.Tools\run-app.ps1 -NoWeb          # API (+ mobile) only; start Angular yourself: cd GHCAA.Web ; npm start
.\GHCAA.Tools\run-app.ps1 -NoMobile      # API (+ web) only; skip Flutter launcher
.\GHCAA.Tools\run-app.ps1 -NoWeb -NoMobile   # Backend only
```

`run-app.bat` passes extra arguments through to the script (e.g. `run-app.bat -NoWeb`).

**URLs** (see `GHCAA.API/Properties/launchSettings.json` profile `https`):

- API HTTPS: `https://localhost:7214` (Swagger: `/swagger`)
- API HTTP: `http://localhost:5087`
- Web: `http://localhost:4200`

**Database provider override**

Sets **`DatabaseProvider`** for `dotnet ef` and for the API process (must match `DependencyInjection`: `PgSql`, `MySql`, or `Sqlite`). If you omit `-DatabaseProvider`, any stale `DatabaseProvider` in the **same PowerShell window** is cleared so the API uses `appsettings.json` again.

When you pass `-DatabaseProvider`, the API is started via **`cmd.exe`** so that variable is always applied to `dotnet run` reliably.

**Secrets**

- Prefer **`GHCAA.API/.env`** (copy from `.env.example`) or **User Secrets** in `GHCAA.API`.
- If `.env` is missing, the launcher prints a short reminder.

### `stop-app.ps1`

Stops **dotnet**, **node**, **dart**, related tooling, and releases ports **7214**, **5087**, **4200**. Port cleanup collects PIDs from `netstat` in a **single-element-safe** way (avoids a PowerShell `foreach` pitfall on one PID).

**Usage**

```powershell
.\GHCAA.Tools\stop-app.ps1              # prompts for confirmation
.\GHCAA.Tools\stop-app.ps1 -Force         # no prompt (used by run-app)
.\GHCAA.Tools\stop-app.ps1 -PortsOnly    # only free ports, milder
.\GHCAA.Tools\stop-app.ps1 -Force -IncludeBrowsers   # also closes Chrome/Edge (aggressive)
```

### `configure-local-env.ps1`

Copies **`GHCAA.API/.env.example`** → **`GHCAA.API/.env`** if `.env` does not exist. Edit `.env` with real values; `.env` is gitignored.

### Batch wrappers

- **`run-app.bat`** — `powershell -ExecutionPolicy Bypass -File run-app.ps1`
- **`stop-app.bat`** — stops dotnet/node/flutter and clears ports; also attempts to close **Chrome/Edge** (use only if you accept closing all browser windows).

## Related documentation

- Repo **[README.md](../README.md)** — secrets, CORS, DataProtection, handover checklist.
- **[GHCAA.API/.env.example](../GHCAA.API/.env.example)** — template for local environment variables.

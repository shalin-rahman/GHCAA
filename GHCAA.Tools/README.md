# GHCAA.Tools

Scripts for local development and a few one-off database repairs. Run from any directory; scripts resolve the repo root from `$PSScriptRoot`.

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

Sets **`DatabaseProvider`** for `dotnet ef` and for the API process (`PgSql` or `Sqlite`; MySQL was dropped, see `docs/adr/0006-drop-mysql-provider.md`). `Sqlite` is for tests and `ASP_SEED_PROFILE=Visual` only. If you omit `-DatabaseProvider`, any stale `DatabaseProvider` in the **same PowerShell window** is cleared so the API uses `appsettings.json` again.

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

### `CI-Deploy.ps1`

Builds a release package in `publish/`: runs backend tests, publishes the API, deletes the old `GHCAA.Web/dist`, runs `npm ci` and `npm run build` in `GHCAA.Web`, and copies `dist/GHCAA.Web/browser` into `publish/api/wwwroot`. Any failed step stops the script. It does not touch a database: `MigrationBootstrapper` applies migrations when the API starts (`docs/adr/0002-migrations-apply-automatically-at-startup.md`).

### `detect-and-run-mobile.bat`, `run-app-mobile.bat`

Start the API (unless `GHCA_API_ALREADY_STARTED=1`) and the Flutter app from `GHCAA.Mobile`.

### One-off repair and diagnostic scripts

Not part of the normal workflow. Several hard-code local credentials or database names, so read them before running.

- **`test_registration.ps1`**, **`test_login.ps1`**, **`test_login.bat`** call a running local API.
- **`list_users.cs`**, **`db_diag.cs`**, **`db_sync.cs`**, **`fix_shalin_login.csx`** are single-use diagnostics. This folder has no project file, so the `.cs` and `.csx` files are never compiled. They still use the old `ApplicationDbContext` and `DefaultConnection` names.
- **`sync.sql`**, **`restore_admin.sql`**, **`reset_superadmin_password.sql`**, **`delete_imported_members.sql`** are PostgreSQL scripts for a local database. Back up first.
- Visual regression runs use the Playwright suite in `GHCAA.Web`.
- **`TestData/`** holds six JSON fixtures that the removed setup and cleanup scripts used. Nothing reads them now; 99.8 in `docs/TODO.md` keeps the decision on them open.

## Related documentation

- Repo **[README.md](../README.md)** — secrets, CORS, DataProtection, handover checklist.
- **[GHCAA.API/.env.example](../GHCAA.API/.env.example)** — template for local environment variables.
